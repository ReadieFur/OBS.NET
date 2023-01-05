#include "Bootstrapper.h"
#include <obs-module.h>

OBS_DECLARE_MODULE();

obs_dotnet_bootstrapper_shared::Bootstrapper bootstrapper(L"obs-dotnet.dll");

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
    //Return the name of the plugin.
    return "obs-dotnet";
}

const char* obs_module_description(void)
{
    //Return the description of the plugin.
    return "A .NET runtime host for OBS plugins.";
}

const char* obs_module_author(void)
{
    //Return the author of the plugin.
    return "ReadieFur";
}
