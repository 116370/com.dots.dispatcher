using System;

namespace DOTS.Dispatcher.Runtime
{
    public enum ECSType
    {
        BeginSimulationEntityCommandBufferSystem = 0,
        EndSimulationEntityCommandBufferSystem = 1,
        BeginInitializationEntityCommandBufferSystem = 2,
        EndInitializationEntityCommandBufferSystem = 3,
        BeginFixedStepSimulationEntityCommandBufferSystem = 4,
        EndFixedStepSimulationEntityCommandBufferSystem = 5,
        BeginVariableRateSimulationEntityCommandBufferSystem = 6,
        EndVariableRateSimulationEntityCommandBufferSystem = 7,
        BeginPresentationEntityCommandBufferSystem = 8,
    }

    /// <summary>
    /// Add Metadata to generate a system CleanUp{ComponentName}
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateCleanUpDestroySystemAttribute : Attribute
    {
        public Type UpdateAfter { get; set; }
        public Type UpdateBefore { get; set; }
        public Type UpdateInGroup { get; set; }

        public ECSType ecbType;
        public GenerateCleanUpDestroySystemAttribute()
        {
            this.ecbType = ECSType.BeginSimulationEntityCommandBufferSystem;
            UpdateInGroup = typeof(CleanupEventsSystemGroup);
        }
        public GenerateCleanUpDestroySystemAttribute(ECSType ecsType = ECSType.BeginSimulationEntityCommandBufferSystem)
        {
            this.ecbType = ecsType;
            UpdateInGroup = typeof(CleanupEventsSystemGroup);

        }
    }


    /// <summary>
    /// Add Metadata to generate a system CleanUp{ComponentName}
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateCleanUpDisableSystemAttribute : Attribute
    {
        public Type UpdateAfter { get; set; }
        public Type UpdateBefore { get; set; }
        public Type UpdateInGroup { get; set; }

        public ECSType ecbType;

        public GenerateCleanUpDisableSystemAttribute()
        {
            this.ecbType = ECSType.BeginSimulationEntityCommandBufferSystem;
            UpdateInGroup = typeof(CleanupEventsSystemGroup);
        }

        public GenerateCleanUpDisableSystemAttribute(ECSType ecsType = ECSType.BeginSimulationEntityCommandBufferSystem)
        {
            this.ecbType = ecsType;
            UpdateInGroup = typeof(CleanupEventsSystemGroup);

        }
    }
}
