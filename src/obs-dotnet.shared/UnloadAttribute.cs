using System;

namespace obs_dotnet.shared
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class UnloadAttribute : Attribute
    {
    }
}
