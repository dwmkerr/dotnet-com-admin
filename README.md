# dotnet-com-admin

[![Build status](https://ci.appveyor.com/api/projects/status/nueh3x0o7b7xsi4x?svg=true)](https://ci.appveyor.com/project/dwmkerr/dotnet-com-admin) [![codecov](https://codecov.io/gh/dwmkerr/dotnet-com-admin/branch/master/graph/badge.svg)](https://codecov.io/gh/dwmkerr/dotnet-com-admin)

The COM Admin library provides APIs to manage the installation and registration of .NET Framework and .NET Core COM Servers and Shell Extensions.

<!-- vim-markdown-toc GFM -->

* [Quick Start](#quick-start)
    * [Examining a File](#examining-a-file)
* [Developer Guide](#developer-guide)
* [Running the CLI](#running-the-cli)
* [Creating a Release](#creating-a-release)

<!-- vim-markdown-toc -->

## Quick Start

This guide shows how to perform common tasks programatically or with the CLI.

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
## Developer Guide

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
