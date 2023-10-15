using Prototype;
using System.Globalization;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    public struct EventCommandBuffer
    {
        private EntityCommandBuffer buffer;

        public EventCommandBuffer(Allocator allocator)
        {
            buffer = new EntityCommandBuffer(allocator);
        }

        public EventCommandBuffer(EntityCommandBuffer ecb)
        {
            buffer = ecb;
        }

        public ParallelWriter AsParallelWriter()
        {
            return new ParallelWriter(buffer.AsParallelWriter());
        }
        public struct ParallelWriter
        {
            private EntityCommandBuffer.ParallelWriter buffer;
            public ParallelWriter(EntityCommandBuffer.ParallelWriter bufferIn)
            {
                buffer = bufferIn;
            }

            public void PostEvent<T>(int sortKey, T data = default) where T : unmanaged, IDestroyableECSEvent, IComponentData
            {
                var e = buffer.CreateEntity(sortKey);
                buffer.AddComponent(sortKey, e, data);
                buffer.AddComponent<DisaptcherClenupDestroy>(sortKey, e);
            }

            public void PostEvent<T>(int sortKey, Entity e, T data = default) where T : unmanaged, IDisableableECSEvent, IComponentData, IEnableableComponent
            {
                buffer.AddComponent(sortKey, e, data);
                buffer.SetComponentEnabled<T>(sortKey, e, true);

            }
            public void PostEvent<T>(int sortKey, Entity e) where T : unmanaged, IBufferElementData, IEnableableComponent
            {
                buffer.SetComponentEnabled<T>(sortKey, e, true);
            }

        }

       

        public void PostEvent<T>(T data = default) where T : unmanaged, IComponentData, IDestroyableECSEvent
        {
            var e = buffer.CreateEntity();
            buffer.AddComponent(e, data);
            buffer.AddComponent<DisaptcherClenupDestroy>(e);
        }

        public void PostEvent<T>(Entity e, T data = default) where T : unmanaged, IComponentData, IEnableableComponent, IDisableableECSEvent
        {          
            buffer.AddComponent(e, data);
            buffer.SetComponentEnabled<T>(e, true);

        }

        public void PostEvent<T>(Entity e) where T : unmanaged, IBufferElementData, IEnableableComponent, IDestroyableECSEvent
        {
            buffer.SetComponentEnabled<T>(e, true);
        }
    }
}