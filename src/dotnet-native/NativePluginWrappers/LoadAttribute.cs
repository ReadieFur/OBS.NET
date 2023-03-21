namespace OSBDotNetNativePlugin.NativePluginWrappers
{
#if USE_REFLECTION
    /// <summary>
    /// Declares the entrypoint for this plugin.
    /// Must be a static method with no parameters and a return type of bool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class LoadAttribute : Attribute {}
#endif
}
