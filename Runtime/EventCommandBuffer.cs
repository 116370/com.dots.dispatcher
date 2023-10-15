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
            buffer.AsParallelWriter();
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

            public void PostEvent<T>(int sortKey, T data = default) where T : unmanaged, IComponentData
            {
                var e = buffer.CreateEntity(sortKey);
                buffer.AddComponent(sortKey, e, data);
                buffer.AddComponent<DisaptcherClenup>(sortKey, e);

            }
        }

        public EventCommandBuffer(EntityCommandBuffer ecb)
        {
            buffer = ecb;
        }

        public void PostEvent<T>(T data = default) where T : unmanaged, IComponentData
        {
            var e = buffer.CreateEntity();
            buffer.AddComponent(e, data);
            buffer.AddComponent<DisaptcherClenup>(e);

        }

        public void Playback(EntityManager manager)
        {
            buffer.Playback(manager);
        }
    }
}