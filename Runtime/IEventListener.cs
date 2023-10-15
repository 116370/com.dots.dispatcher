using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    public interface IEventListener<T> where T : unmanaged, IComponentData
    {
        void OnEvent(Entity entity, in T data);
    }

    public interface IBufferEventListener<T> where T : unmanaged, IBufferElementData
    {
        void OnEvent(Entity entity, in DynamicBuffer<T> data);
    }
}
