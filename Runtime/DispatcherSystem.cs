using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using static DispatcherSystem;

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

    public void PostEvent<T>(T data = default) where T : unmanaged, IComponentData
    {
        var e = buffer.CreateEntity();
        buffer.AddComponent(e, data);

        var type = ComponentType.ReadWrite<T>();

        buffer.AddComponent<DisaptcherClenup>(e);

    }

    public void Playback(EntityManager manager)
    {
        buffer.Playback(manager);
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
internal partial class DispatcherGroup : ComponentSystemGroup { }

public struct DisaptcherClenup : IComponentData { }

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

        base.OnUpdate();

        foreach (var container in Mono.containers.Values) {
            container.Update();
        }

    }

    internal interface IDispatcherContainer
    {
        void Update();
    }

    internal class DispatcherContainer<T> : IDispatcherContainer where T : unmanaged, IComponentData
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
        internal static Dictionary<TypeIndex, List<object>> subscribers = new();
        internal static Dictionary<TypeIndex, IDispatcherContainer> containers = new();
        public static void Subscribe<T1>(IEventListener<T1> listener) where T1 : unmanaged, IComponentData
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

        public static void Unsubscribe<T1>(IEventListener<T1> listener) where T1 : unmanaged, IComponentData
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