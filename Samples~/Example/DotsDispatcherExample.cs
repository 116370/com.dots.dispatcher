using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class SimulationDispatcherSystem : DispatcherSystem
{
}

public struct ZeroSizeTestData : IComponentData
{
}

public struct TestData : IComponentData
{
    public int Value;
}

public struct TestData2 : IComponentData
{
    public FixedString32Bytes Value;
}


public partial class EventProducerSystem : SystemBase
{
    DispatcherSystem _dispatcherSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        _dispatcherSystem = World.GetExistingSystemManaged<SimulationDispatcherSystem>();
    }

    protected override void OnUpdate()
    {
        var dispatcherQueue = _dispatcherSystem.CreateDispatcherQueue<ZeroSizeTestData>();

        for (var index = 0; index < 0xFF; index++)
        {
            dispatcherQueue.Enqueue(default);
        }
    }
}

public partial class EventProducerJobSystem : SystemBase
{
    DispatcherSystem _dispatcherSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        _dispatcherSystem = World.GetExistingSystemManaged<SimulationDispatcherSystem>();
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var dispatcherQueue = _dispatcherSystem.CreateDispatcherQueue<TestData>().AsParallelWriter();
            dispatcherQueue.Enqueue(new TestData { Value = 12345 });

            var dispatcherQueue2 = _dispatcherSystem.CreateDispatcherQueue<TestData2>().AsParallelWriter();
            dispatcherQueue2.Enqueue(new TestData2 { Value = "asdasdasd" });

            // Since this system is producing events in Jobs, i.e. asynchronously, 
            // it must be declared as a dependency for the DispatcherSystem through this API call.
            _dispatcherSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

public partial struct EventConsumerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var data in SystemAPI.Query<TestData>())
        {
            Debug.Log("1: " + data.Value);
        }

        foreach (var data in SystemAPI.Query<TestData2>())
        {
            Debug.Log("2: " + data.Value);
        }
    }
}

public partial class EventConsumerSystem1 : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var data in SystemAPI.Query<TestData>())
        {
            Debug.Log("11: " + data.Value);
        }

        foreach (var data in SystemAPI.Query<TestData2>())
        {
            Debug.Log("22: " + data.Value);
        }
    }
}