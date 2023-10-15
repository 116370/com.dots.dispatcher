using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    [UpdateInGroup(typeof(DispatcherGroupSystem))]
    [UpdateAfter(typeof(DispatcherSystem))]
    public partial class MonoEventsSystemGroup : ComponentSystemGroup { }
}