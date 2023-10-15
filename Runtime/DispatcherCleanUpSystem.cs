using Unity.Burst;
using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    public struct DisaptcherClenupDestroy : IComponentData { }

    [UpdateInGroup(typeof(DispatcherGroupSystem))]
    public partial struct DispatcherCleanUpSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder().WithAll<DisaptcherClenupDestroy>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.DestroyEntity(query);
        }
    }
}