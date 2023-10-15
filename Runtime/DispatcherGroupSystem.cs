using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    internal partial class DispatcherGroupSystem : ComponentSystemGroup { }
}