using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial class DispatcherSystem : SystemBase
{
    readonly Dictionary<Type, IDispatcherContainer> _dictionary = new Dictionary<Type, IDispatcherContainer>();

    /// <summary>
    /// Creates a NativeQueue which can be used to enqueue Event Data to be created when the DispatcherSystem runs.
    /// </summary>
    /// <typeparam name="T">struct, IComponentData</typeparam>
    /// <returns>NativeQueue</returns>
    public NativeQueue<T> CreateDispatcherQueue<T>() where T : unmanaged, IComponentData
    {
        if (!_dictionary.TryGetValue(typeof(T), out var dispatcherContainer))
        {
            _dictionary.Add(typeof(T), dispatcherContainer = new DispatcherContainer<T>(this));
        }

        return ((DispatcherContainer<T>)dispatcherContainer).CreateDispatcherQueue();
    }

    public void PostEvent<T>(T data) where T : unmanaged, IComponentData
    {
        if (!_dictionary.TryGetValue(typeof(T), out var dispatcherContainer))
        {
            _dictionary.Add(typeof(T), dispatcherContainer = new DispatcherContainer<T>(this));
        }

        var eque = ((DispatcherContainer<T>)dispatcherContainer).CreateDispatcherQueue();
        eque.Enqueue(data);
    }

    /// <summary>
    /// Must be used if the DispatcherQueue is enqueueing data in Jobs, i.e. asynchronously.
    /// Guarantees that the DispatcherSystem will only run after its dependencies,
    /// which are determined by this function call.
    /// </summary>
    /// <param name="dependency">The event producer system dependency</param>
    public void AddJobHandleForProducer(JobHandle dependency)
    {
        Dependency = JobHandle.CombineDependencies(Dependency, dependency);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var dispatcherContainer in _dictionary.Values)
        {
            dispatcherContainer.Dispose();
        }

        _dictionary.Clear();
    }

    protected override void OnUpdate()
    {
        if (_dictionary.Count == 0)        
            return;
        

        foreach (var dispatcherContainer in _dictionary.Values)
        {
            dispatcherContainer.Update();
        }
    }


    interface IDispatcherContainer : IDisposable
    {
        void Update();
    }

    internal class DispatcherContainer<T> : IDispatcherContainer where T : unmanaged, IComponentData
    {
        readonly EntityManager _entityManager;
        private readonly TypeIndex _typeIndex;
        readonly EntityQuery _query;
        readonly List<NativeQueue<T>> _queueList;

        public DispatcherContainer(DispatcherSystem dispatcherSystem)
        {
            var componentType = ComponentType.ReadWrite<T>();

            _entityManager = dispatcherSystem.EntityManager;
            _query = dispatcherSystem.GetEntityQuery(componentType);
            _typeIndex = TypeManager.GetTypeIndex(typeof(T));
            _queueList = new List<NativeQueue<T>>();
        }

        public void Update()
        {
            _entityManager.DestroyEntity(_query);

            List<IEventListener<T>> listeners = null;

            if (Mono.subscribers.TryGetValue(_typeIndex, out var listMono))
            {
                listeners = listMono.OfType<IEventListener<T>>().ToList();
            }

            foreach (var queue in _queueList)
            {
                while (queue.Count != 0)
                {
                    var comp = queue.Dequeue();
                    var e = _entityManager.CreateEntity();
                    _entityManager.AddComponentData(e, comp);
                    ProduceMonoEvents(listeners, comp, e);
                }

                queue.Dispose();
            }

            _queueList.Clear();
        }

        private static void ProduceMonoEvents(List<IEventListener<T>> listeners, T comp, Entity e)
        {
            if (listeners != null)
            {
                foreach (var listener in listeners)
                {
                    listener.OnEvent(e, comp);
                }
            }
        }

        public void Dispose()
        {
            _query.CompleteDependency();

            foreach (var queue in _queueList)
            {
                queue.Dispose();
            }

            _queueList.Clear();
        }

        public NativeQueue<T> CreateDispatcherQueue(Allocator allocator = Allocator.TempJob)
        {
            var queue = new NativeQueue<T>(allocator);

            _queueList.Add(queue);

            return queue;
        }
    }

    public static class Mono
    {
        internal static Dictionary<TypeIndex, List<object>> subscribers = new();

        public static void Subscribe<T1>(IEventListener<T1> listener) where T1 : unmanaged, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex(typeof(T1));

            if (!subscribers.TryGetValue(typeIndex, out var list))
            {
                list = new List<object>();
                subscribers.Add(typeIndex, list);
            }

            list.Add(listener);
        }

        public static void Unsubscribe<T1>(IEventListener<T1> listener) where T1 : unmanaged, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex(typeof(T1));

            if (subscribers.TryGetValue(typeIndex, out var list))
            {
                list.Remove(listener);
            }
        }
    }
}