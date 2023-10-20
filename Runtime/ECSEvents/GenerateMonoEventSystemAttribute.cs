using System;

namespace DOTS.Dispatcher.Runtime
{
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateMonoEventSystemAttribute : Attribute { }
}
