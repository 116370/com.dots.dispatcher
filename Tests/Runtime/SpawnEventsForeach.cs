using DOTS.Dispatcher.Runtime;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Dispatcher.Tests.Runtime
{
    [DisableAutoCreation]
    public partial class SpawnEventsForeach : SystemBase
    {
        public int spawnTimes;
        protected override void OnUpdate()
        {
            if (spawnTimes == 0)
                return;
            Debug.Log($"SpawnEventsForeach {spawnTimes}");

            var eventCommandBuffer = SystemAPI.GetSingleton<DispatcherSystem.Singleton>().CreateEventBuffer(World.Unmanaged);
            for (int i = 0; i < spawnTimes; i++)
            {
                eventCommandBuffer.PostEvent<TestEventDestroyableComponent>();
            }
        }
    }
}