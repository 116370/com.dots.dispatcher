using DOTS.Dispatcher.Runtime;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DOTS.Dispatcher.Tests.Runtime
{
    [DisableAutoCreation]
    public partial class SpawnEventsInsideJob : SystemBase
    {
        public int spawnTimes;
        protected override void OnUpdate()
        {
            if (spawnTimes == 0)
                return;

            Debug.Log($"SpawnEventsInsideJob {spawnTimes}");
            var eventCommandBuffer = SystemAPI.GetSingleton<DispatcherSystem.Singleton>().CreateEventBuffer(World.Unmanaged);
            Dependency = new SpawnJob
            {
                spawnTimes = spawnTimes,
                buffer = eventCommandBuffer
            }.Schedule(Dependency);
            Dependency.Complete();

        }

        public partial struct SpawnJob : IJob
        {
            public EventCommandBuffer buffer;
            public int spawnTimes;

            public void Execute()
            {
                for (int i = 0; i < spawnTimes; i++)
                {
                    buffer.PostEvent<TestEvenComponent>();
                }
            }
        }
    }
}