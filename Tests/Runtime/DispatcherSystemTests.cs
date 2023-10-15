using DOTS.Dispatcher.Runtime;
using NUnit.Framework;
using System;
using Unity.Entities;
using Unity.Jobs;
using UnityEditor.Search;
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
                eventCommandBuffer.PostEvent<TestEvenComponent>();
            }
        }

    }

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

    public class DispatcherSystemTests : ECSTestsFixture
    {
        private DispatcherSystem dispatcherSystem;

        public override void Setup()
        {
            CreateDefaultWorld = true;

            base.Setup();

            var sys1 = m_World.AddSystemManaged(new SpawnEventsInsideJob());
            var sys2 = m_World.AddSystemManaged(new SpawnEventsInsideJobParallel());
            var sys3 = m_World.AddSystemManaged(new SpawnEventsForeach());

            var simulationSystemGroup = m_World.GetExistingSystemManaged<SimulationSystemGroup>();

            simulationSystemGroup.AddSystemToUpdateList(sys1);
            simulationSystemGroup.AddSystemToUpdateList(sys2);
            simulationSystemGroup.AddSystemToUpdateList(sys3);


            Assert.IsNotNull(simulationSystemGroup);

            dispatcherSystem = m_World.GetExistingSystemManaged<DispatcherSystem>();

            Assert.IsNotNull(dispatcherSystem);

        }


        [Test]
        public void TestMonoEventSupport()
        {
            var instanceGO = new GameObject("TestMonoEventSupportGO");
            var testEventC = instanceGO.AddComponent<TestMonoEvents>();
        
            var expected = 10;
            dispatcherSystem.PostEvent(new TestEvenComponent { testData = expected });

            Debug.Log("UpdateWorld");
            m_World.Update();

            Assert.AreEqual(expected, testEventC.lastEvetnData.testData);
         

            m_World.Update();
            GameObject.DestroyImmediate(instanceGO);

        }

        [Test]
        public void CleanupEntityEventsSystem()
        {
            var query = m_Manager.CreateEntityQuery(typeof(TestEvenComponent));

            var expected = 0;
            Assert.AreEqual(expected, query.CalculateEntityCount());

            dispatcherSystem.PostEvent<TestEvenComponent>();
            m_World.Update();
            expected = 1;
            Assert.AreEqual(expected, query.CalculateEntityCount());

            m_World.Update();
            expected = 0;
            Assert.AreEqual(expected, query.CalculateEntityCount());
        }

        [Test]
        public void PostEventInsideJob()
        {
            var expected = 0;
            var query = m_Manager.CreateEntityQuery(typeof(TestEvenComponent));
            m_World.Update();
            Assert.AreEqual(expected, query.CalculateEntityCount());

            var sys = m_World.GetExistingSystemManaged<SpawnEventsInsideJob>();
            expected = 10;
            sys.spawnTimes = expected;
            m_World.Update();

            sys.spawnTimes = 0;
            Assert.AreEqual(expected, query.CalculateEntityCount());

            expected = 0;
            m_World.Update();
            Assert.AreEqual(expected, query.CalculateEntityCount());
        }

        [Test]
        public void PostEventForeach()
        {
            var expected = 0;
            var query = m_Manager.CreateEntityQuery(typeof(TestEvenComponent));
            m_World.Update();
            Assert.AreEqual(expected, query.CalculateEntityCount());

            var sys = m_World.GetExistingSystemManaged<SpawnEventsForeach>();
            expected = 10;
            sys.spawnTimes = expected;
           
            m_World.Update();
            Debug.Log($"Assert {expected} {query.CalculateEntityCount()}");
            Assert.AreEqual(expected, query.CalculateEntityCount());

            sys.spawnTimes = 0;
            expected = 0;

            m_World.Update();
            Assert.AreEqual(expected, query.CalculateEntityCount());
        }


        [Test]
        public void PostEventInsideJobParallel()
        {
            var query = m_Manager.CreateEntityQuery(typeof(TestEvenComponent));
            var expected = 0;
            m_World.Update();
            Assert.AreEqual(expected, query.CalculateEntityCount());

            var sys = m_World.GetExistingSystemManaged<SpawnEventsInsideJobParallel>();
            expected = 10;
            sys.spawnTimes = expected;
            m_World.Update();
            sys.spawnTimes = 0;
            Assert.AreEqual(expected, query.CalculateEntityCount());
            expected = 0;
            m_World.Update();
            Assert.AreEqual(expected, query.CalculateEntityCount());
        }

    }
}