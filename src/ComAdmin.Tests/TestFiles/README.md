# TestFiles

These files are currently built from:

https://github.com/dwmkerr/scratch/ComAdminTestServers

| File | Type | COM Server | Description |
|------|------|------------|-------------|
| `NativeDll.dll` | Native Win32 Dll | None | There a no COM servers in this DLL. |
| `NativeAtlComServer.dll` | Native Win32 COM Server | `00000000-0000-0000-C0C0-000000000001` | Empty COM server. |
| `DotNetFrameworkComServer.dll` | .NET Framework COM Server | `00000000-0000-0000-C0C0-000000000002` | Empty COM server, version `0.1.2.3`, signed. |
| `DotNetCoreServer.comhost.dll` | .NET Core COM Server | `00000000-0000-0000-C0C0-000000000003` | Empty COM server. |

Helpers:

```ps
cd \\Mac\Home\repos\github\dwmkerr\dotnet-com-admin\src\ComAdmin.Tests\TestFiles\

C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /codebase .\DotNetFrameworkComServer.dll

C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /regfile:InstallDotNetCoreComServer.comhost.dll .\DotNetCoreComServer.dll 
```
