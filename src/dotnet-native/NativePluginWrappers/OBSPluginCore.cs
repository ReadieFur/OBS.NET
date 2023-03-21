using System.Runtime.InteropServices;

namespace OSBDotNetNativePlugin.NativePluginWrappers
{
    public static class OBSPluginCore
    {
        private static nint obs_module_pointer;

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_set_pointer))]
        private static void obs_module_set_pointer(nint pointer) =>
            obs_module_pointer = pointer;

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_load))]
#if USE_REFLECTION
        [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetExportedTypes()")]
#endif
        private static bool obs_module_load()
        {
            //I forgot you can't use reflection with AOT compilation.
#if USE_REFLECTION
            foreach (Type type in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (method.GetParameters().Length != 0 || method.ReturnType != typeof(bool))
                        continue;

                    foreach (Attribute attribute in method.GetCustomAttributes())
                    {
                        if (attribute is not LoadAttribute)
                            continue;

                        try { return method.Invoke(null, null) as bool? ?? false; }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to invoke load method {method.Name} in {type.Name}: {ex}");
                            return false;
                        }
                    }
                }
            }

            Logger.Error("No load method found.");
            return false;
#else
            return PluginInfo.OnLoad.Invoke();
#endif
        }

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_unload))]
#if USE_REFLECTION
        [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetExportedTypes()")]
#endif
        private static void obs_module_unload()
        {
#if USE_REFLECTION
            foreach (Type type in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (method.GetParameters().Length != 0 || method.ReturnType != typeof(void))
                        continue;

                    foreach (Attribute attribute in method.GetCustomAttributes())
                    {
                        if (attribute is not UnloadAttribute)
                            continue;

                        try { method.Invoke(null, null); }
                        catch (Exception ex) { Logger.Error($"Failed to invoke unload method {method.Name} in {type.Name}: {ex}"); }
                    }
                }
            }
#else
            PluginInfo.OnUnload?.Invoke();
#endif
        }

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_name))]
        private static unsafe byte* obs_module_name() =>
            (byte*)Marshal.StringToHGlobalAnsi(PluginInfo.Name).ToPointer();

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_description))]
        private static unsafe byte* obs_module_description() =>
            (byte*)Marshal.StringToHGlobalAnsi(PluginInfo.Description).ToPointer();

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_author))]
        private static unsafe byte* obs_module_author() =>
            (byte*)Marshal.StringToHGlobalAnsi(PluginInfo.Author).ToPointer();

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_ver))]
        private static uint obs_module_ver() =>
            (uint)PluginInfo.Version.Major << 24
            | (uint)PluginInfo.Version.Minor << 16
            | (uint)PluginInfo.Version.Revision;
    }
}
