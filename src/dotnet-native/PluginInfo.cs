namespace OSBDotNetNativePlugin
{
    public static class PluginInfo
    {
        #region Metadata
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public const string Name = "obs-dotnet-native-plugin";
        /// <summary>
        /// The description of the plugin.
        /// </summary>
        public const string Description = "A .NET native plugin for OBS Studio.";
        /// <summary>
        /// The author of the plugin.
        /// </summary>
        public const string Author = "ReadieFur";
        /// <summary>
        /// The plugin version/
        /// </summary>
        internal const string _Version = "1.0.0.0";
        #endregion

#if !USE_REFLECTION
        #region Callbacks
        /// <summary>
        /// Declares the method to be called when the plugin is loaded.
        /// Returns true if the plugin loaded successfully, false otherwise.
        /// Method must be public, static and have no parameters.
        /// Must not be null.
        /// </summary>
        internal static Func<bool> OnLoad = Plugin.Load;
        /// <summary>
        /// Declares the method to be called when the plugin is unloaded.
        /// Method must be public, static have no parameters and return void.
        /// Can be null.
        /// </summary>
        internal static Action? OnUnload = Plugin.Unload;
        #endregion
#endif

        #region Misc
        //Wraps the string _Version variable into a Version object.
        public static readonly Version Version = new Version(_Version);
        #endregion
    }
}
