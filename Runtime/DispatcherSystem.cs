using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    internal interface IDispatcherContainer
    {
        void Update();
    }

    [UpdateAfter(typeof(DispatcherCleanUpSystem))]
    [UpdateInGroup(typeof(DispatcherGroupSystem))]
    public partial class DispatcherSystem : EntityCommandBufferSystem
    {

        public unsafe struct Singleton : IComponentData, IECBSingleton
        {
            internal UnsafeList<EntityCommandBuffer>* pendingBuffers;
            internal AllocatorManager.AllocatorHandle allocator;

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

        protected override void OnUpdate()
        {
            base.OnUpdate();

            //foreach (var container in Mono.containers.Values)
            //{
            //    container.Update();
            //}
        }

        public void PostEvent<T>(T eventData = default) where T : unmanaged, IComponentData, IDestroyableECSEvent
        {
            SystemAPI.GetSingleton<Singleton>().CreateEventBuffer(World.Unmanaged).PostEvent(eventData);
        }

        internal class DispatcherContainer<T> : IDispatcherContainer where T : unmanaged, IComponentData, IECSEvent
        {
            private readonly TypeIndex _typeIndex;
            readonly EntityQuery _query;

            public DispatcherContainer(DispatcherSystem dispatcherSystem)
            {
                var componentType = ComponentType.ReadWrite<T>();
                _query = dispatcherSystem.GetEntityQuery(componentType);
                _typeIndex = TypeManager.GetTypeIndex(typeof(T));
            }

            public void Update()
            {

                var getArray = _query.ToEntityArray(Allocator.Temp);
                var dataArray = _query.ToComponentDataArray<T>(Allocator.Temp);

                List<IEventListener<T>> listeners = null;

                if (Mono.subscribers.TryGetValue(_typeIndex, out var listMono))
                {
                    listeners = listMono.OfType<IEventListener<T>>().ToList();
                }

                for (int i = 0; i < getArray.Length; i++)
                {
                    foreach (var item in listeners)
                    {
                        item.OnEvent(getArray[i], dataArray[i]);
                    }
                }
            }
        }

        public static class Mono
        {
            public static Dictionary<TypeIndex, List<object>> subscribers = new();
            internal static Dictionary<TypeIndex, IDispatcherContainer> containers = new();
            public static void Subscribe<T1>(IEventListener<T1> listener) where T1 : unmanaged, IComponentData, IECSEvent
            {
                var typeIndex = TypeManager.GetTypeIndex(typeof(T1));

                if (!subscribers.TryGetValue(typeIndex, out var list))
                {
                    list = new List<object>();
                    subscribers.Add(typeIndex, list);
                }

                if (!containers.TryGetValue(typeIndex, out var container))
                {
                    var sys = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DispatcherSystem>();
                    containers.TryAdd(typeIndex, new DispatcherContainer<T1>(sys));
                }

                list.Add(listener);
            }

            public static void Unsubscribe<T1>(IEventListener<T1> listener) where T1 : unmanaged, IComponentData, IECSEvent
            {
                var typeIndex = TypeManager.GetTypeIndex(typeof(T1));

                if (subscribers.TryGetValue(typeIndex, out var list))
                {
                    list.Remove(listener);

                    if (list.Count == 0)
                    {
                        subscribers.Remove(typeIndex);
                        containers.TryGetValue(typeIndex, out var dispatcher);
                        containers.Remove(typeIndex);
                    }
                }
            }
        }
    }
}