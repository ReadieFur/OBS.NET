using System.Runtime.InteropServices;
using System.Text;

namespace native
{
    public partial class Class1
    {
        public static nint obs_module_pointer;

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_set_pointer))]
        public static void obs_module_set_pointer(nint pointer) =>
            obs_module_pointer = pointer;

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_ver))]
        public static uint obs_module_ver()
        {
            const uint major = 1;
            const uint minor = 0;
            const uint patch = 0;
            return (major << 24) | (minor << 16) | patch;
        }

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_load))]
        public static bool obs_module_load()
        {
            Log("Loading obs-dotnet-native module.");
            return true;
        }

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_unload))]
        public static void obs_module_unload() {}

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_name))]
        public static unsafe byte* obs_module_name() =>
            (byte*)Marshal.StringToHGlobalAnsi("obs-dotnet-native").ToPointer();

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_description))]
        public static unsafe byte* obs_module_description() =>
            (byte*)Marshal.StringToHGlobalAnsi("An OBS plugin built in C# and compiled to native code.").ToPointer();

        [UnmanagedCallersOnly(EntryPoint = nameof(obs_module_author))]
        public static unsafe byte* obs_module_author() =>
            (byte*)Marshal.StringToHGlobalAnsi("ReadieFur").ToPointer();

        [LibraryImport("obs")]
        public static unsafe partial void blogva(int log_level, sbyte* format, sbyte* args);

        public static unsafe void Log(string message)
        {
            var asciiBytes = Encoding.UTF8.GetBytes(message);
            fixed (byte* buffer = asciiBytes)
                blogva(300, (sbyte*)buffer, null);
        }
    }
}
