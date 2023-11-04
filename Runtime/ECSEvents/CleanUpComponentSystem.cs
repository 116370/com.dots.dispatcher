using Unity.Burst.Intrinsics;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T">IECSEvent component</typeparam>
    public partial class CleanUpComponentSystem<T, ECBSystem> : SystemBase
        where T : unmanaged, IEnableableComponent, IComponentData
        where ECBSystem : EntityCommandBufferSystem
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
            if (query.CalculateEntityCount() == 0)
                return;

            //var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var ecb = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer();

            Dependency = new CleanUpJob
            {
                ecb = ecb,
                entityHandle = SystemAPI.GetEntityTypeHandle()

            }.Schedule(query, Dependency);
          
        }

        [BurstCompile]
        public partial struct CleanUpJob : IJobChunk
        {
            public EntityCommandBuffer ecb;
            internal EntityTypeHandle entityHandle;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                var entities = chunk.GetNativeArray(entityHandle);

                while (enumerator.NextEntityIndex(out var i))
                {
                    ecb.SetComponentEnabled<T>(entities[i], false);
                }
            }
        }
    }
}