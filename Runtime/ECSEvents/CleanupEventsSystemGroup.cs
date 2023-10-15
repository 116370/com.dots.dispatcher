using DOTS.Dispatcher.Runtime;
using Unity.Entities;

namespace Prototype
{
    [UpdateInGroup(typeof(DispatcherGroupSystem))]
    [UpdateBefore(typeof(DispatcherCleanUpSystem))]
    public partial class CleanupEventsSystemGroup : ComponentSystemGroup { }
}