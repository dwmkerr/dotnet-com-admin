# dotnet-com-admin

[![Build status](https://ci.appveyor.com/api/projects/status/nueh3x0o7b7xsi4x?svg=true)](https://ci.appveyor.com/project/dwmkerr/dotnet-com-admin) [![codecov](https://codecov.io/gh/dwmkerr/dotnet-com-admin/branch/master/graph/badge.svg)](https://codecov.io/gh/dwmkerr/dotnet-com-admin)

The COM Admin library provides APIs to manage the installation and registration of .NET Framework and .NET Core COM Servers and Shell Extensions.

The goal of this project is to allow you to manage the registration of servers _without_ having to rely on the [`regasm`](https://docs.microsoft.com/en-us/dotnet/framework/tools/regasm-exe-assembly-registration-tool) or [`regsvr32`](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/regsvr32) tools, and to provide more functionality than is currently provided by the [`RegistrationServices`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.registrationservices) class.

<!-- vim-markdown-toc GFM -->

* [Quick Start](#quick-start)
    * [Examining a File](#examining-a-file)
* [The Registry & Testing](#the-registry--testing)
* [Developer Guide](#developer-guide)
* [Running the CLI](#running-the-cli)
* [Creating a Release](#creating-a-release)

<!-- vim-markdown-toc -->

## Quick Start

This guide shows how to perform common tasks programatically or with the CLI.

| Component | NuGet Package | Package | Compatibility |
|-----------|---------------|---------|---------------|
| COM Admin APIs | `ComAdmin` | [![ComAdmin NuGet Package](https://img.shields.io/nuget/v/ComAdmin.svg)](https://www.nuget.org/packages/ComAdmin) | .NET Framework 4+, .NET Core 2.0+, .NET Standard 2.0+ |
| COM Admin CLI | `com-admin-cli` | [![com-admin-cli Nuget Package](https://img.shields.io/nuget/v/com-admin-cli.svg)](https://www.nuget.org/packages/com-admin-cli) | .NET Core 3.0+ |

Install the CLI with the following command:

```sh
dotnet tool install --global com-admin-cli

# Now you can run commands like:
com-admin examine ./some-assembly.dll
```

### Examining a File

You can examine a file to determine whether it contains Shell Extension COM servers. Note that this only works for .NET Framework or .NET Core Assemblies, not Native DLLs.

**CLI**

```
$ com-admin examine CopyDirectoryLocationHandler.dll
COM Admin CLI v0.1.0.0-alpha1, Copyright (c) Dave Kerr 2020
https://github/dwmkerr/dotnet-com-admin

  Examining 'CopyDirectoryLocationHandler.dll'...
    Source File            : CopyDirectoryLocationHandler.dll
    File Type              : DotNetFrameworkAssembly
    Version Type           : 1.0.0.0
    Processor Architecture : MSIL
    Framework              : .NETFramework,Version=v4.5
```

**Code**

```cs
using ComAdmin;

var examineResults = ComAdmin.Examine(@"./CopyDirectoryLocationHandler.dll");
Console.WriteLine($"File Type: {examineResults.FileType}"); // etc
```

## The Registry & Testing

This project is primarily used to register and manage COM servers, which means it uses the Windows Registry extensively. Modifying the registry directly would be problematic when running tests, as all changes would need to be cleaned up afterwards.

To support testing, this project supports a 'Testable Registry'. Instead of accessing the registry directly, _all_ APIs and classes use the `IRegistry` interface. By default, the implementation of this interface used in the project is the `WindowsRegistry` class, which is nothing more than a wrapper around the standard registry. However, this implementation can be swapped out for the `InMemoryRegistry` class.

The `InMemoryRegistry` class is a lightweight implementation of the registry, which essentially is just an in memory file. This is ideal for testing scenarios. This mechanism is **completely re-usable** for your own projects.

The code snippet below shows how you can use the `InMemoryRegistry` to test a scenario:

```cs
public class RegisterDotNetCoreComServerTests
{
    private InMemoryRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        //  When running a test, use the in-memory registry. Let the ComAdmin APIs know to use this instance instead
        //  of the default WindowsRegistry.
        _registry = new InMemoryRegistry();
        ComAdmin.SetRegistryImplementation(_registry);
    }

    [TearDown]
    public void TearDown()
    {
        //  Reset the service registry to the standard Windows Registry implementation.
        ComAdmin.SetRegistryImplementation(new WindowsRegistry());
    }

    [Test]
    public void RegisterDotNetCoreComServer_Correctly_Creates_A_Class_Entry()
    {
        //  Create a dummy guid and COM host and register the server as a .NET Core server with COM Admin.
        var clsid = new Guid("00000000-1111-2222-3333-444444444444");
        const string progId = "SomeComServerName.MyServer";
        const string proxyHostPath = @"c:\Some Folder\SomeServer.comhost.dll";
        ComAdmin.RegisterDotNetCoreComServer(RegistryView.Registry64, clsid, progId, proxyHostPath);

        //  Assert we have the expected structure
        var print = _registry.Print(RegistryView.Registry64);
        Assert.That(print, Is.EqualTo(string.Join(Environment.NewLine,
            @"HKEY_CLASSES_ROOT",
            @"   CLSID",
            @"      {00000000-1111-2222-3333-444444444444} = CoreCLR COMHost Server",
            @"        InProcServer32 = c:\Some Folder\SomeServer.comhost.dll",
            @"        ThreadinModel = Both",
            @"      ProgId = SomeComServerName.MyServer")
        ));
    }
}
```

You can pre-populate the registry with structure if needed. In this example, the test validates the behaviour of server registration when a class is already registered with the same class identifier:

```cs
[Test]
public void RegisterDotNetCoreComServer_Throws_If_A_Class_Is_Already_Registered_With_The_Same_Clsid()
{
    //  Pre-popoluate the registry with a server which clashes with the one we will register.
    _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
        @"HKEY_CLASSES_ROOT",
        @"   CLSID",
        @"      {00000000-1111-2222-3333-444444444444} = Some Existing Server")
    );

    //  Create a dummy guid and COM host and register the server as a .NET Core server with COM Admin.
    var clsid = new Guid("00000000-1111-2222-3333-444444444444");
    const string progId = "SomeComServerName.MyServer";
    const string proxyHostPath = @"c:\Some Folder\SomeServer.comhost.dll";
    
    //  Assert that we throw in this case.
    Assert.Throws<ClassAlreadyRegisteredException>(() =>
        ComAdmin.RegisterDotNetCoreComServer(RegistryView.Registry64, clsid, progId, proxyHostPath));
}
```

The `IRegistry`, `InMemoryRegistry` and `WindowsRegistry` classes are all fully documented and can be used in your own projects. The `ComAdmin.Tests` project has an extensive set of examples which show their usage in action.

One thing to be aware of is that the `SetRegistryImplementation` function is `static` and modifies a `static` member variable of the `ComAdmin` class. This means it is **not thread safe**. If threads are using the registry concurrently, they will not necessarily use the defined implementation. Set the implementation _before_ running multi-threaded code. Of course, if you want to use these classes in your own code, you can make them thread safe in your implementation if you prefer.

You may want to use Dependency Injection for Inversion of Control for the registry. This is completely supported for your projects. `ComAdmin` itself does not use an IoC container as it adds more complexity than I feel is needed for this one use case, but that doesn't prevent you from using containers like this yourself.

## Developer Guide

This section covers all of the material you should need to be able to build the code locally, customise it to your needs, or contribute to the project.

All source code is in the `src` directory. You can open the `./src/ComAdmin.sln` solution in Visual Studio or Code.

To build, test and package the project, just run:

| Command        | Usage                                                                                    |
|----------------|------------------------------------------------------------------------------------------|
| `init.ps1`     | Ensure your machine can run builds by installing necessary components such as `codecov`. |
| `dotnet build` | Build the library and CLI.                                                               |
| `dotnet test`  | Run the tests.                                                                           |
| `coverage.ps1` | Run the tests, generating a coverage report in `./artifacts`.                            |
| `dotnet pack`  | Build the NuGet packages.                                                                |

## Running the CLI

Run the CLI locally with the `dotnet run` command:

```
cd src/com-admin-cli;
dotnet run -- examine "./somefile.dll"
```

Install the local version of the tool with the following command:

```
dotnet tool install --global --add-source ./src/com-admin-cli/bin/Debug /src/com-admin-cli
```

You can find more examples on how to manage the local CLI tool in the [Tutorial: Create a .NET Core tool using the .NET Core CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create) guide.

## Creating a Release

To create a release, update the version number in the project files and create version tag. Then push - the build pipeline with publish the release
