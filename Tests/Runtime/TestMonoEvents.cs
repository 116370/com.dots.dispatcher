using DOTS.Dispatcher.Runtime;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Dispatcher.Tests.Runtime
{
    public struct TestEventDestroyableComponent : IComponentData, IDestroyableECSEvent
    {
        public int testData;
    }

    public class TestMonoEvents : MonoBehaviour, IEventListener<TestEventDestroyableComponent>
    {
        public TestEventDestroyableComponent lastEvetnData;
        public void OnEvent(Entity entity, in TestEventDestroyableComponent data)
        {
            lastEvetnData = data;
            Debug.Log("TestMonoEvents.OnEvent");
        }

        public void OnEnable()
        {
            Debug.Log("TestMonoEvents.OnEnable");
            DispatcherSystem.Mono.Subscribe(this);
        }

        public void OnDisable()
        {
            Debug.Log("TestMonoEvents.OnDisable");
            DispatcherSystem.Mono.Unsubscribe(this);
        }
    }
}