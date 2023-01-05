#pragma once
//The relative path (from the obs plugins folder) to your dotnet binary.
#define DOTNET_RELATIVE_ASSEMBLY_PATH L"obs-dotnet.dll"

//The namespace.type that contains your entrypoint method.
#define DOTNET_TYPE_NAME L"obs_dotnet.Plugin"

//The name of your entrypoint method.
#define DOTNET_METHOD_NAME L"Run"

//The name of the plugin.
#define PLUGIN_NAME "obs-dotnet"

//The plugin description.
#define PLUGIN_DESCRIPTION "A .NET runtime host template for obs plugins."

//The author of the plugin.
#define PLUGIN_AUTHOR "ReadieFur"

//TODO: Add extra options in here such as plugin version and dotnet version (possibly in the future detect these from the dotnet binary instead).
