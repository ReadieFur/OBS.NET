#pragma once

#include <stdio.h>
#include <string>
#include <chrono>
#include <thread>
#include <iostream>
#include <windows.h>
#include <metahost.h>
#include <atomic>

#pragma comment(lib, "mscoree.lib")

namespace obs_dotnet_bootstrapper_shared
{
	class Bootstrapper
	{
	private:
		bool constructorSuccess = FALSE;
		HANDLE pluginStateEvent;
		std::thread dotnetThread;
		std::wstring assemblyPath;
		std::wstring typeName;
		std::wstring methodName;
		std::wstring pluginStateEventName;

		int HostDotNetDLL();

	public:
		Bootstrapper(std::wstring relativeBinaryPath, std::wstring typeName = L"obs_dotnet.shared.Bootstrapper", std::wstring methodName = L"Run");

		bool Load();
		void Unload();
	};
}
