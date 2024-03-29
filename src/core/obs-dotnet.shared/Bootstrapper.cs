﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace obs_dotnet.shared
{
    public static class Bootstrapper
    {
#pragma warning disable CS8618
        private static EventWaitHandle pluginStateEvent;
        private static Task unloadWaitTask; //This is only used to keep the task in scope.
#pragma warning restore CS8618

        //This method is invoked by the native plugin boot-strapper.
        private static int Run(string pluginStateEventName)
        {
            //The first firing of this event will be when the plugin is loaded, the second will be when the plugin is unloaded.
            pluginStateEvent = EventWaitHandle.OpenExisting(pluginStateEventName);

            //Signal that the plugin has started.
            pluginStateEvent.Set();
            pluginStateEvent.Reset();

            //Get the static methods in this assembly.
            IEnumerable<MethodInfo> staticMethods = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            //Find the Load method.
            MethodInfo? loadMethod = staticMethods.FirstOrDefault(m => m.GetCustomAttribute<LoadAttribute>() != null);
            if (loadMethod == null)
                throw new Exception("Load method not found.");
            else if (loadMethod.ReturnType != typeof(bool))
                throw new Exception("Load method must return a bool.");

            //Run the load method.
            if (!(bool)loadMethod.Invoke(null, null))
                return -1;

            //Get the unload method for later.
            MethodInfo? unloadMethod = staticMethods.FirstOrDefault(m => m.GetCustomAttribute<UnloadAttribute>() != null);

            //Wait for the unload signal, we can wait indefinitely here.
            ManualResetEventSlim unloadEvent = new(false);
            unloadWaitTask = Task.Run(() =>
            {
                pluginStateEvent.WaitOne();
                if (unloadMethod != null)
                    unloadMethod.Invoke(null, null);
                unloadEvent.Set();
            });
            //We also want to wait here for the unload event to be set, this is to ensure that the plugin has finished unloading before the process exits.
            //We could exit the program as soon as the load method finished however there may be stuff inside of this method that we want to run,
            //instead of taking the typical approach where the program exits as soon as the main method as finished executing.
            unloadEvent.Wait();

            return 0;
        }
    }
}
