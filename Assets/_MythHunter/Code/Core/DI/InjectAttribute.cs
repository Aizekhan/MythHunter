using System;

namespace MythHunter.Core.DI
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectAttribute : Attribute
    {
    }
}
