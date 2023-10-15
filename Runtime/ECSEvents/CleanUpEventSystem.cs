using DOTS.Dispatcher.Runtime;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace Prototype
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T">IECSEvent component</typeparam>
    public partial class CleanUpComponentSystem<T> : SystemBase where T : unmanaged, IEnableableComponent, IComponentData
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            base.OnCreate();
            query = GetEntityQuery(ComponentType.ReadOnly<T>());
            query.SetChangedVersionFilter(typeof(T));
            
            RequireForUpdate(query);
        }

        protected override void OnUpdate()
        {
            Dependency = new CleanUpJob
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                entityHandle = SystemAPI.GetEntityTypeHandle()

            }.ScheduleParallel(query, Dependency);
        }

        [BurstCompile]
        public partial struct CleanUpJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            internal EntityTypeHandle entityHandle;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                var entities = chunk.GetNativeArray(entityHandle);
              
                while (enumerator.NextEntityIndex(out var i))
                {                  
                    ecb.SetComponentEnabled<T>(unfilteredChunkIndex, entities[i], false);
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">IECSEvent component</typeparam>
    public partial class CleanUpBufferSystem<T> : SystemBase where T : unmanaged, IEnableableComponent, IBufferElementData
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            base.OnCreate();
            query = GetEntityQuery(ComponentType.ReadOnly<T>());
            query.SetChangedVersionFilter(typeof(T));

            RequireForUpdate(query);
        }

        protected override void OnUpdate()
        {
            Dependency = new CleanUpJob
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                entityHandle = SystemAPI.GetEntityTypeHandle(),
                bufferHandle = SystemAPI.GetBufferTypeHandle<T>()


            }.ScheduleParallel(query, Dependency);
        }

        [BurstCompile]
        public partial struct CleanUpJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public EntityTypeHandle entityHandle;
            public BufferTypeHandle<T> bufferHandle;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                
                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                var entities = chunk.GetNativeArray(entityHandle);
                var accesorBuf = chunk.GetBufferAccessor(ref bufferHandle);
                while (enumerator.NextEntityIndex(out var i))
                {
                    accesorBuf[i].Clear();
                    ecb.SetComponentEnabled<T>(unfilteredChunkIndex, entities[i], false);
                }
            }
        }
    }

    
    //public struct Component : IComponentData
    //{

    //}

    //[UpdateInGroup(typeof(DOTS.Dispatcher.Runtime.MonoEventsSystemGroup))]
    //public partial class MonoEventSystem : SystemBase
    //{
    //    private EntityQuery query;
    //    private TypeIndex typeIndex;

    //    protected override void OnCreate()
    //    {
    //        base.OnCreate();
    //        query = GetEntityQuery(ComponentType.ReadOnly<Component>());
    //        typeIndex = TypeManager.GetTypeIndex(typeof(Component));          
    //        RequireForUpdate(query);
    //    }

    //    protected override void OnUpdate()
    //    {
    //        if (DispatcherSystem.Mono.subscribers.TryGetValue(typeIndex, out var subscriber))
    //        {
    //            foreach (var (compData, e) in SystemAPI.Query<Component>().WithEntityAccess())
    //            {
    //                foreach (var item1 in subscriber)
    //                {
    //                    ((IEventListener<Component>)item1).OnEvent(e, compData);
    //                }
    //            }
    //        }
    //    }
    //}
}
