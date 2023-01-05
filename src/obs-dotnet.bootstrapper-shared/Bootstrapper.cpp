#include "Bootstrapper.h"

//Used to locate this module on the filesystem.
void func(){}

obs_dotnet_bootstrapper_shared::Bootstrapper::Bootstrapper(std::wstring relativeBinaryPath, std::wstring typeName, std::wstring methodName) :
	typeName(typeName),
	methodName(methodName)
{
    srand(time(0));
    //The first firing of this event will be when the plugin is loaded, the second will be when the plugin is unloaded.
    do
    {
        if (pluginStateEvent != NULL)
        {
            CloseHandle(pluginStateEvent);
            pluginStateEvent = NULL;
        }
        int randomNumber = rand() % 1000 + 1;
        pluginStateEventName = relativeBinaryPath + std::to_wstring(randomNumber) + L"_event";
        pluginStateEvent = OpenEvent(EVENT_ALL_ACCESS, FALSE, pluginStateEventName.c_str());
    } while (pluginStateEvent != NULL);
    pluginStateEvent = CreateEvent(NULL, TRUE, FALSE, pluginStateEventName.c_str());

    HMODULE hModule;
    wchar_t path[MAX_PATH];
    //Get the handle to the current DLL.
    if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR)&func, &hModule))
        return;

    //Get the path of the DLL.
    if (!GetModuleFileNameW(hModule, path, sizeof(path)))
        return;

    //substring 8 characters from the end.
    std::wstring directory = path;
    directory = directory.substr(0, directory.find_last_of(L"\\/") + 1);
    directory += relativeBinaryPath;
    assemblyPath = directory.c_str();

    constructorSuccess = TRUE;
}

int obs_dotnet_bootstrapper_shared::Bootstrapper::HostDotNetDLL()
{
    ICLRMetaHost* pMetaHost = NULL;
    ICLRRuntimeInfo* pRuntimeInfo = NULL;
    ICLRRuntimeHost* pClrRuntimeHost = NULL;
    LPCWSTR _assemblyPath;

    //Get the .NET MetaHost.
    HRESULT hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
    if (FAILED(hr))
    {
        //printf("CLRCreateInstance failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

    //Get the .NET Runtime.
    hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&pRuntimeInfo);
    if (FAILED(hr))
    {
        //printf("ICLRMetaHost::GetRuntime failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

    //Check if the runtime is installed.
    BOOL fLoadable;
    hr = pRuntimeInfo->IsLoadable(&fLoadable);
    if (FAILED(hr))
    {
        //printf("ICLRRuntimeInfo::IsLoadable failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

    if (!fLoadable)
    {
        //printf(".NET runtime v4.0.30319 is not installed\n");
        goto Cleanup;
    }

    //Load the .NET runtime.
    hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pClrRuntimeHost);
    if (FAILED(hr))
    {
        //printf("ICLRRuntimeInfo::GetInterface failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

    hr = pClrRuntimeHost->Start();
    if (FAILED(hr))
    {
        //printf("ICLRRuntimeHost::Start failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

    //Execute the .NET assembly.
    //https://stackoverflow.com/questions/31874806/using-executeindefaultappdomain-to-call-the-main-function-c-sharp-console-applic
    DWORD dwRet;
    hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(
        assemblyPath.c_str(),
        typeName.c_str(),
        methodName.c_str(),
        pluginStateEventName.c_str(),
        &dwRet
    );
    if (FAILED(hr))
    {
        //printf("ICLRRuntimeHost::ExecuteInDefaultAppDomain failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

Cleanup:
    if (pClrRuntimeHost)
    {
        pClrRuntimeHost->Stop();
        pClrRuntimeHost->Release();
    }
    if (pRuntimeInfo)
    {
        pRuntimeInfo->Release();
    }
    if (pMetaHost)
    {
        pMetaHost->Release();
    }

    if (!FAILED(hr))
        return dwRet;
    else
        return -1;
}

bool obs_dotnet_bootstrapper_shared::Bootstrapper::Load()
{
    if (!constructorSuccess)
        return FALSE;

	dotnetThread = std::thread(&Bootstrapper::HostDotNetDLL, this);

    if (WaitForSingleObject(pluginStateEvent, 5000) == WAIT_TIMEOUT)
    {
        //std::cerr << "Error waiting for event: " << GetLastError() << std::endl;
        return FALSE;
    }
    else
    {
        //I probably don't need too, but to be safe against race conditions, I will reset the event on both sides.
        ResetEvent(pluginStateEvent);
    }

    return TRUE;
}

void obs_dotnet_bootstrapper_shared::Bootstrapper::Unload()
{
    SetEvent(pluginStateEvent);
    dotnetThread.join();
}
