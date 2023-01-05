# OBS.NET  
This is a .NET runtime host for OBS Studio. It allows you to create OBS plugins using .NET!  
**NOTE:** To any non-programmers/standard users looking for plugins, this repository is for developers only, the releases do nothing by themselves!  
*NOTE:* This repository is a work in progress. While it should be usable, it is not yet feature complete.  

## How to use
There are two ways this can be used.  
The first is in a "standalone" mode where you compile your own C++ boot-strapper as one DLL and the C# code as another.  
The other method is to use the boot-strapper that I have already made and just compile your C# code into a DLL.  
Each have their own benefits, if you don't need to write your own C++ code that directly interfaces with the OBS plugin loader, you can use the second method for an easier setup. Don't worry, if you find you need to switch to the first method later on down the line, it is very easy to do so.  

### "Standalone"
This is the more complicated method as you need to compile your own C++ boot-strapper, this shouldn't be too difficult as I have provided a template which you can find in the `obs-dotnet.bootstrapper-shared` folder [here](./src/core/obs-dotnet.bootstrapper-shared/).  
To load a C# DLL, you can create a new instance of the `Bootstrapper` class, passing in the name of your DLL (and optionally the class and entry method) and then call the Load method.  
Inside of your C# DLL, you can again use the `Bootstrapper` as a template to initialize your plugin. If you choose to manually initialize your plugin on the C# side, your entrypoint method must be `static`, return an `integer` and take a single `string` parameter, this parameter is the name of the shared event that is used to signal to and from the native plugin, the event should only ever be fired twice, once by the C# program upon load and a second time by the C++ program when the C# dll should be unloaded.  

### Shared Boot-strapper
This is the easier method as you only need to create your C# DLL.  
To get started, download the latest release of the shared boot-strapper from the [releases](./releases/latest) page, (make sure you download both the `obs-dotnet.bootstrapper.dll` and the `obs-dotnet.dll` files). Now add these two files into your OBS plugins directory, typically found at `C:\Program Files\obs-studio\obs-plugins\64bit`.  
To get started add the shared C# project files found in the `obs-dotnet.shared` folder found [here](./src/core/obs-dotnet.shared/) to your project.  
Now inside of your C# project, create a new `static` method that return a `bool`, this boolean value will be used to determine if the plugin should be loaded or not. Above this newly created method, add the `LoadAttribute` attribute to signal to the boot-strapper that this is the entrypoint method.  
You can optionally add an unload method with the `UnloadAttribute` attribute, this method will be called when the plugin should be unloaded.  
*Note:* These two methods are blocking to all other .NET plugins, so if your plugin halts, none of the other .NET plugins will be able to load. This only applies to the shared boot-strapper, the standalone boot-strapper will not block other plugins from loading.  
To use your plugin inside of OBS, place the compiled DLL into the OBS plugins directory (for ease in my testing I used a symbolic link).  

## Debugging
If you wish to debug the C++ program, set your launch project to the C++ project. Alternatively, if you wish to debug the C# program, set your launch project to the C# project.  
For whichever project you set as your launch project, C++ or C#, inside of that projects launch settings, set your target application to your OBS executable. and make sure to set the working directory to the root OBS directory.  
