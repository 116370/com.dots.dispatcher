using DOTS.Dispatcher.Runtime;
using Unity.Entities;

namespace DOTS.Dispatcher.Runtime
{
    [UpdateInGroup(typeof(DispatcherGroupSystem))]
    [UpdateBefore(typeof(DispatcherCleanUpSystem))]
    public partial class CleanupEventsSystemGroup : ComponentSystemGroup { }
}