using Unity.Entities;

public interface IEventListener<T> where T : unmanaged, IComponentData
{
    void OnEvent(Entity entity, in T data);
}
