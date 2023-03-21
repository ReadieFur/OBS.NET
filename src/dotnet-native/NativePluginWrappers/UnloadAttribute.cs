namespace OSBDotNetNativePlugin.NativePluginWrappers
{
#if USE_REFLECTION
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class UnloadAttribute : Attribute {}
#endif
}
