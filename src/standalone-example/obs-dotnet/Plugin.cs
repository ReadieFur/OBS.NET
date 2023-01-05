using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using obs_dotnet.shared;

[assembly: PluginInfo(IsStandalone = true)]

namespace obs_dotnet.standalone
{
    public class Plugin
    {
        [DllImport("obs.dll")]
        public static extern int obs_get_version();

        [Load]
        private static bool Load()
        {
            string testMessage = $"Hello, World. From C#!\nOBS Version: {obs_get_version()}";
            Console.WriteLine(testMessage);
            Task.Run(() => System.Windows.Forms.MessageBox.Show(testMessage));

            return true;
        }

        [Unload]
        private static void Unload()
        {
            string testMessage = $"Goodbye, World. From C#!";
            Console.WriteLine(testMessage);
            System.Windows.Forms.MessageBox.Show(testMessage);
        }
    }
}
