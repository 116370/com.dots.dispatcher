using System;

namespace Prototype
{
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class GenerateMonoEventSystemAttribute : Attribute { }
}
