#include "Bootstrapper.h"
#include <obs-module.h>

#define DOTNET_DLL L"obs-dotnet.dll"
#define PLUGIN_NAME "obs-dotnet"
#define PLUGIN_DESCRIPTION "A wrapper plugin for OBS that hosts a .NET runtime."
#define PLUGIN_AUTHOR "ReadieFur"

OBS_DECLARE_MODULE();

obs_dotnet_bootstrapper_shared::Bootstrapper bootstrapper(DOTNET_DLL);


bool obs_module_load(void)
{
    return bootstrapper.Load();
}

void obs_module_unload(void)
{
	bootstrapper.Unload();
}

const char* obs_module_name(void)
{
    // Return the name of the plugin
    return PLUGIN_NAME;
}

const char* obs_module_description(void)
{
    // Return the description of the plugin
    return PLUGIN_DESCRIPTION;
}

const char* obs_module_author(void)
{
	// Return the author of the plugin
	return PLUGIN_AUTHOR;
}
