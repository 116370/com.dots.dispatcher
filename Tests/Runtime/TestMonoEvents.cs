using DOTS.Dispatcher.Runtime;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Dispatcher.Tests.Runtime
{
    public struct TestEvenComponent : IComponentData
    {
        public int testData;
    }

    public class TestMonoEvents : MonoBehaviour, IEventListener<TestEvenComponent>
    {
        public TestEvenComponent lastEvetnData;
        public void OnEvent(Entity entity, in TestEvenComponent data)
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