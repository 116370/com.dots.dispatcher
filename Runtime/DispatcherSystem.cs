using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public struct EventCommandBuffer
{
    private EntityCommandBuffer buffer;
    public EventCommandBuffer(Allocator allocator)
    {
        buffer = new EntityCommandBuffer(allocator);
    }

    public EventCommandBuffer(EntityCommandBuffer ecb)
    {
        buffer = ecb;
    }

    public void PostEvent<T>(T data = default, PostMonoEvents postData = default) where T : unmanaged, IComponentData
    {
        var e = buffer.CreateEntity();
        buffer.AddComponent(e, data);
        buffer.AddComponent<DisaptcherClenup>(e);
        buffer.AddComponent(e, postData);

    }

    public void Playback(EntityManager manager)
    {
        buffer.Playback(manager);
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
internal partial class DispatcherGroup : ComponentSystemGroup { }

public struct DisaptcherClenup : IComponentData { }
public struct PostMonoEvents : IComponentData 
{
    public TypeIndex typeIndex;
}

[UpdateInGroup(typeof(DispatcherGroup))]
public partial struct DispatcherCleanUpSystem : ISystem
{
    private EntityQuery query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        query = SystemAPI.QueryBuilder().WithAll<DisaptcherClenup>().Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.DestroyEntity(query);
    }
}

[UpdateAfter(typeof(DispatcherCleanUpSystem))]
[UpdateInGroup(typeof(DispatcherGroup))]
public partial class DispatcherSystem : EntityCommandBufferSystem
{   

    public unsafe struct Singleton : IComponentData, IECBSingleton
    {
        internal UnsafeList<EntityCommandBuffer>* pendingBuffers;
        internal AllocatorManager.AllocatorHandle allocator;

        //public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
        //{

        //    var ecb = EntityCommandBufferSystem
        //        .CreateCommandBuffer(ref *pendingBuffers, allocator, world);
           
        //    return ecb;
        //}

        public EventCommandBuffer CreateEventBuffer(WorldUnmanaged world)
        {
            var ecb = EntityCommandBufferSystem
                .CreateCommandBuffer(ref *pendingBuffers, allocator, world);
            var eventBUffer = new EventCommandBuffer(ecb);
            return eventBUffer;
        }

        // Required by IECBSingleton
        public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
        {
            var ptr = UnsafeUtility.AddressOf(ref buffers);
            pendingBuffers = (UnsafeList<EntityCommandBuffer>*)ptr;
        }

        // Required by IECBSingleton
        public void SetAllocator(Allocator allocatorIn)
        {
            allocator = allocatorIn;
        }

        // Required by IECBSingleton
        public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
        {
            allocator = allocatorIn;
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        this.RegisterSingleton<Singleton>(ref PendingBuffers, World.Unmanaged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        //if (_dictionary.Count == 0)
        //    return;

        //foreach (var dispatcherContainer in _dictionary.Values)
        //{
        //    dispatcherContainer.Update();
        //}

        base.OnUpdate();

        //foreach (var (item , e) in SystemAPI.Query<PostMonoEvents>().WithEntityAccess())
        //{
        //    List<IEventListener<T>> listeners = null;

        //    if (Mono.subscribers.TryGetValue(item.typeIndex, out var listMono))
        //    {
        //        listeners = listMono.OfType<IEventListener<T>>().ToList();
        //        ProduceMonoEvents();
        //    }
        //}
       
    }

    private static void ProduceMonoEvents<T>(List<IEventListener<T>> listeners, T comp, Entity e) where T : unmanaged, IComponentData
    {
        if (listeners != null)
        {
            foreach (var listener in listeners)
            {
                listener.OnEvent(e, comp);
            }
        }
    }

    //interface IDispatcherContainer : IDisposable
    //{
    //    void Update();
    //}

    //internal class DispatcherContainer<T> : IDispatcherContainer where T : unmanaged, IComponentData
    //{
    //    readonly EntityManager _entityManager;
    //    private readonly TypeIndex _typeIndex;
    //    readonly EntityQuery _query;
    //    readonly List<NativeQueue<T>> _queueList;

    //    public DispatcherContainer(DispatcherSystem dispatcherSystem)
    //    {
    //        var componentType = ComponentType.ReadWrite<T>();

    //        _entityManager = dispatcherSystem.EntityManager;
    //        _query = dispatcherSystem.GetEntityQuery(componentType);
    //        _typeIndex = TypeManager.GetTypeIndex(typeof(T));
    //        _queueList = new List<NativeQueue<T>>();
    //    }

    //    public void Update()
    //    {
    //        _entityManager.DestroyEntity(_query);

    //        List<IEventListener<T>> listeners = null;

    //        if (Mono.subscribers.TryGetValue(_typeIndex, out var listMono))
    //        {
    //            listeners = listMono.OfType<IEventListener<T>>().ToList();
    //        }

    //        foreach (var queue in _queueList)
    //        {
    //            while (queue.Count != 0)
    //            {
    //                var comp = queue.Dequeue();
    //                var e = _entityManager.CreateEntity();
    //                _entityManager.AddComponentData(e, comp);
    //                ProduceMonoEvents(listeners, comp, e);
    //            }

    //            queue.Dispose();
    //        }

    //        _queueList.Clear();
    //    }

    //    private static void ProduceMonoEvents(List<IEventListener<T>> listeners, T comp, Entity e)
    //    {
    //        if (listeners != null)
    //        {
    //            foreach (var listener in listeners)
    //            {
    //                listener.OnEvent(e, comp);
    //            }
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        _query.CompleteDependency();

    //        foreach (var queue in _queueList)
    //        {
    //            queue.Dispose();
    //        }

    //        _queueList.Clear();
    //    }

    //    public NativeQueue<T> CreateDispatcherQueue(Allocator allocator = Allocator.TempJob)
    //    {
    //        var queue = new NativeQueue<T>(allocator);

    //        _queueList.Add(queue);

    //        return queue;
    //    }
    //}

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