using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Event component alive one frame loop 
/// </summary>
public struct PlayerHealthChanged : IComponentData
{
    public PlayerHeathC Value;
}

public partial class EventProducerSystem : SystemBase
{
    private Unity.Mathematics.Random rnd;

    protected override void OnCreate()
    {
        base.OnCreate();
        rnd = new Unity.Mathematics.Random(100);
        RequireForUpdate<PlayerHeathC>();
        RequireForUpdate<SampleSettingC>();

    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var playerHealth = SystemAPI.GetSingletonRW<PlayerHeathC>();
            var setting = SystemAPI.GetSingleton<SampleSettingC>();

            var eventCB = SystemAPI.GetSingleton<DispatcherSystem.Singleton>().CreateEventBuffer(World.Unmanaged);
            playerHealth.ValueRW.currentHealth = rnd.NextInt(1, playerHealth.ValueRW.maxHealth);

            switch (setting.value)
            {
                case SampleType.MainThread:
                    eventCB.PostEvent(new PlayerHealthChanged { Value = playerHealth.ValueRO });
                    Debug.Log($"Event posted from main thread {playerHealth.ValueRO.ToString()}");
                    break;
                case SampleType.Job:

                    Debug.Log($"Event posted from job {playerHealth.ValueRO.ToString()}");

                    new PostEventJob
                    {
                        buffer = eventCB,
                    }.Schedule();
                    break;
                case SampleType.JobParallel:
                    Debug.Log($"Event posted from paralel job {playerHealth.ValueRO.ToString()}");
                    new PostEventJobParallel
                    { 
                         buffer = eventCB.AsParallelWriter(),
                    }.ScheduleParallel();
                    break;
                default:
                    break;
            }        
        }
    }

    [WithChangeFilter(typeof(PlayerHeathC))]
    public partial struct PostEventJob : IJobEntity
    {
        public EventCommandBuffer buffer;
        [BurstCompile]
        public void Execute(in PlayerHeathC playerHealth)
        {
            buffer.PostEvent(new PlayerHealthChanged { Value = playerHealth });
        }

    }

    [WithChangeFilter(typeof(PlayerHeathC))]
    public partial struct PostEventJobParallel : IJobEntity
    {
        public EventCommandBuffer.ParallelWriter buffer;
        [BurstCompile]
        public void Execute(in PlayerHeathC playerHealth, [EntityIndexInQuery]int sortIndex)
        {
            buffer.PostEvent(sortIndex, new PlayerHealthChanged { Value = playerHealth });
        }

    }
}

public partial struct EventConsumerJobSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var eventCB = SystemAPI.GetSingleton<DispatcherSystem.Singleton>().CreateEventBuffer(state.WorldUnmanaged);
        foreach (var data in SystemAPI.Query<PlayerHealthChanged>())
        {
            Debug.Log($"EventConsumerJobSystem {data.Value.ToString()}");
        }
    }
}
