using System;

namespace Prototype
{
    public enum CleanUpMode
    {
        DisableComponent = 0,
        DestoryEntity = 1,
    }

    /// <summary>
    /// Add Metadata to generate a system CleanUp{ComponentName}
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateCleaUpSystemAttribute : Attribute
    {
        public CleanUpMode mode = CleanUpMode.DisableComponent;
        public CleanUpMode Mode => mode;
        public GenerateCleaUpSystemAttribute(CleanUpMode mode) 
        { 
            this.mode = mode;
        }

        public GenerateCleaUpSystemAttribute() { }
    }
}
