using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    public interface IECSEvent { }
    public interface IDestroyableECSEvent : IECSEvent { }
    public interface IDisableableECSEvent : IECSEvent, IEnableableComponent { }
}
