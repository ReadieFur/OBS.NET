using obs_dotnet.shared;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: PluginInfo(IsStandalone = true)]

#nullable enable
namespace obs_dotnet
{
    public static class Plugin
    {
        private static ConcurrentQueue<MethodInfo> assemblyUnloadMethods = new();

        [Load]
        //TODO: Add logging.
        //TODO: Tidy up if statments.
        private static bool Load()
        {
            //Process all of the dlls that are in the same directory as this dll and check if they can/should be loaded.
            foreach (string dll in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll"))
            {
                //Try to load the assembly.
                Assembly assembly;
                try { assembly = Assembly.LoadFile(dll); }
                catch { continue; }

                Type[] assemblyTypes = assembly.GetTypes();
                IEnumerable<MethodInfo> assemblyMethods = assemblyTypes.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

                //Check if the assembly has a compatiable obs_dotnet_shared_library version.
                Type? pluginInfoAttributeType = assemblyTypes.FirstOrDefault(t => t.Name == nameof(PluginInfoAttribute));
                if (pluginInfoAttributeType == null) continue;
                FieldInfo? assemblyVersionFieldInfo = pluginInfoAttributeType.GetField(nameof(PluginInfoAttribute.obs_dotnet_shared_library_version));
                if (assemblyVersionFieldInfo == null || assemblyVersionFieldInfo.FieldType != typeof(float)) continue;
                if (Math.Truncate((float)assemblyVersionFieldInfo.GetValue(null)) != Math.Truncate(PluginInfoAttribute.obs_dotnet_shared_library_version)) continue;

                //Check if the assembly has a PluginInfoAttribute instance.
                object? pluginInfoAttribute = assembly.GetCustomAttributes(pluginInfoAttributeType, false).FirstOrDefault();
                if (pluginInfoAttribute != null)
                {
                    //Check if the assembly is a standalone plugin (i.e. should not be loaded by obs-dotnet).
                    FieldInfo? isStandaloneFieldInfo = pluginInfoAttributeType.GetField(nameof(PluginInfoAttribute.IsStandalone));
                    if (isStandaloneFieldInfo == null || isStandaloneFieldInfo.FieldType != typeof(bool)) continue;
                    if ((bool)isStandaloneFieldInfo.GetValue(pluginInfoAttribute) == true) continue;
                }

                //Check if the assembly has a valid Load attribute and method.
                Type? loadAttributeType = assemblyTypes.FirstOrDefault(t => t.Name == nameof(LoadAttribute));
                if (loadAttributeType == null) continue;
                IEnumerable<MethodInfo> loadMethods = assemblyMethods.Where(m => m.GetCustomAttributes(loadAttributeType, false).Length > 0);
                if (loadMethods.Count() != 1) continue;
                MethodInfo loadMethod = loadMethods.First();
                if (loadMethod.ReturnType != typeof(bool) || loadMethod.GetParameters().Length != 0) continue;

                //Check if the assembly has an Unload attribute and method, if it does, make sure that it is valid.
                MethodInfo? unloadMethod = null;
                Type? unloadAttributeType = assemblyTypes.FirstOrDefault(t => t.Name == nameof(UnloadAttribute));
                if (unloadAttributeType != null)
                {
                    IEnumerable<MethodInfo> unloadMethods = assemblyMethods.Where(m => m.GetCustomAttributes(unloadAttributeType, false).Length > 0);
                    if (unloadMethods.Count() == 1)
                    {
                        MethodInfo _unloadMethod = unloadMethods.First();
                        if (_unloadMethod.ReturnType == typeof(void) && _unloadMethod.GetParameters().Length == 0)
                            unloadMethod = _unloadMethod;
                    }
                }

                //To follow a similar pattern to the native OBS plugin loader, I will load each compatiable .NET dll synchronously.
                try
                {
                    if (!(bool)loadMethod.Invoke(null, null)) throw new Exception("Load method returned false.");

                    if (unloadMethod != null) assemblyUnloadMethods.Enqueue(unloadMethod);
                }
                catch (Exception ex) { continue; }
            }

            return true;
        }

        [Unload]
        private static void Unload()
        {
            while (assemblyUnloadMethods.TryDequeue(out MethodInfo unloadMethod))
            {
                try { unloadMethod.Invoke(null, null); }
                catch {}
            }
        }
    }
}
