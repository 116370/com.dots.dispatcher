using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Dispatcher.Runtime
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
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            new CleanUpJob
            {
                ecb = ecb,
                entityHandle = SystemAPI.GetEntityTypeHandle()

            }.Run(query);

            ecb.Playback(EntityManager);
            ecb.Dispose();
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