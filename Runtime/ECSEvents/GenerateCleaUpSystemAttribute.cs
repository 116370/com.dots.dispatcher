using System;

namespace DOTS.Dispatcher.Runtime
{
    /// <summary>
    /// Add Metadata to generate a system CleanUp{ComponentName}
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateCleaUpDestroySystemAttribute : Attribute { }

    /// <summary>
    /// Add Metadata to generate a system CleanUp{ComponentName}
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateCleaUpDisableSystemAttribute : Attribute { }
}
