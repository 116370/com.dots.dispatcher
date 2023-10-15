using DOTS.Dispatcher.Runtime;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DOTS.Dispatcher.Tests.Runtime
{
    [DisableAutoCreation]
    public partial class SpawnEventsInsideJobParallel : SystemBase
    {
        public int spawnTimes;
        protected override void OnUpdate()
        {
            if (spawnTimes == 0)
                return;

            Debug.Log($"SpawnEventsInsideJobParallel {spawnTimes}");

            var eventCommandBuffer = SystemAPI.GetSingleton<DispatcherSystem.Singleton>().CreateEventBuffer(World.Unmanaged);
            Dependency = new SpawnJobParallel
            {
                buffer = eventCommandBuffer.AsParallelWriter()
            }.Schedule(spawnTimes, 2, Dependency);

            Dependency.Complete();
        }

        public partial struct SpawnJobParallel : IJobParallelFor
        {
            public EventCommandBuffer.ParallelWriter buffer;

            public void Execute(int index)
            {
                buffer.PostEvent<TestEvenComponent>(index);
            }
        }
    }
}