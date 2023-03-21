using OSBDotNetNativePlugin.NativePluginWrappers;

namespace OSBDotNetNativePlugin
{
    internal static class Plugin
    {
#if USE_REFLECTION
        [Load]
#endif
        public static bool Load()
        {
            Logger.Info("Hello, World!");
            return true;
        }

#if USE_REFLECTION
        [Unload]
#endif
        public static void Unload() {}
    }
}
