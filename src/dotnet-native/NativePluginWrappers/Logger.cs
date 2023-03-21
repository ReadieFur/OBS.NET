//#define USE_INTERNAL_LOG_LEVEL_HANDLER

using System.Runtime.InteropServices;
using System.Text;

namespace OSBDotNetNativePlugin.NativePluginWrappers
{
    internal static partial class Logger
    {
#if USE_INTERNAL_LOG_LEVEL_HANDLER
        public static ELogLevel logLevel =
#if DEBUG
            ELogLevel.DEBUG
#else
            ELogLevel.INFO
#endif
            ;
#endif

        [LibraryImport("obs")]
        private static unsafe partial void blogva(int log_level, sbyte* format, sbyte* args);

        private static unsafe void Log(string message, ELogLevel logLevel)
        {
#if USE_INTERNAL_LOG_LEVEL_HANDLER
            if (logLevel < Logger.logLevel)
                return;
#endif

            byte[] asciiBytes = Encoding.UTF8.GetBytes($"[{Enum.GetName(logLevel)} | {PluginInfo.Name}]: {message}");
            fixed (byte* buffer = asciiBytes)
                blogva((int)logLevel, (sbyte*)buffer, null);
        }

        public static void Error(string message) => Log(message, ELogLevel.ERROR);

        public static void Warning(string message) => Log(message, ELogLevel.WARNING);

        public static void Info(string message) => Log(message, ELogLevel.INFO);

        public static void Debug(string message) => Log(message, ELogLevel.DEBUG);
    }
}
