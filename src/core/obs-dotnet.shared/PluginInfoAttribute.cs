using System;

#nullable enable
namespace obs_dotnet.shared
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class PluginInfoAttribute : Attribute
    {
        public static float obs_dotnet_shared_library_version = 1.0f;

        public bool IsStandalone = false;
    }
}
