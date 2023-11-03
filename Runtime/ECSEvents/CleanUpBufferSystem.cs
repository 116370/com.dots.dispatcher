using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{

    /// <summary>
    /// </summary>
    /// <typeparam name="T">IECSEvent component</typeparam>
    public partial class CleanUpBufferSystem<T, ECBSystem> : SystemBase
        where T : unmanaged, IEnableableComponent, IBufferElementData
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

            var ecb = World.GetOrCreateSystemManaged<ECBSystem>().CreateCommandBuffer();

            Dependency = new CleanUpJob
            {
                ecb = ecb,
                entityHandle = SystemAPI.GetEntityTypeHandle(),
                bufferHandle = SystemAPI.GetBufferTypeHandle<T>()


            }.Schedule(query, Dependency);

        }

        [BurstCompile]
        public partial struct CleanUpJob : IJobChunk
        {
            public EntityCommandBuffer ecb;
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
                    ecb.SetComponentEnabled<T>(entities[i], false);
                }
            }
        }
    }
}
