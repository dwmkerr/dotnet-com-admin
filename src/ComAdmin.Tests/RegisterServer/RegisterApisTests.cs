using System;
using System.IO;
using System.Reflection;
using ComAdmin.Registry;
using Microsoft.Win32;
using NUnit.Framework;

namespace ComAdmin.Tests.RegisterServer
{
    public class RegisterApisTests
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
        public void GetComServerRegistrationInfo_Correctly_Identifies_Unregistered_Server()
        {
            //  No servers, so we shouldn't find anything here...
            //  Provide the structure that would be present for a .NET Framework server.
            _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
                @"HKEY_CLASSES_ROOT",
                @"   CLSID"
            ));

            Assert.That(ComAdmin.GetComServerRegistrationInfo(new Guid("{00000000-1111-2222-3333-444444444444}")), Is.Null);
        }

        [Test]
        public void GetComServerRegistrationInfo_Correctly_Identifies_Dot_Net_Framework_Server()
        {
            //  Provide the structure that would be present for a .NET Framework server.
            _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
                @"HKEY_CLASSES_ROOT",
                @"   CLSID",
                @"      {8b3d441e-f71c-35b4-8cbb-66123a1cb4ca}",
                @"         (Default) = CopyDirectoryLocationHandler",
                @"         InprocServer32",
                @"            (Default) = mscoree.dll",
                @"            Assembly = CopyDirectoryLocationHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fc2d5bf7b4a9e4cb",
                @"            Class = CopyDirectoryLocationHandler.CopyDirectoryLocationHandler",
                @"            RuntimeVersion = v4.0.30319",
                @"            ThreadingModel = Both",
                @"            CodeBase = file:////Mac/Home/repos/github/dwmkerr/sharpshell/SharpShell/Samples/ContextMenu/CopyDirectoryLocationHandler/bin/Debug/CopyDirectoryLocationHandler.dll",
                @"            1.0.0.0",
                @"               Assembly = CopyDirectoryLocationHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fc2d5bf7b4a9e4cb",
                @"               Class = CopyDirectoryLocationHandler.CopyDirectoryLocationHandler",
                @"               RuntimeVersion = v4.0.30319",
                @"               ThreadingModel = Both",
                @"               CodeBase = file:////Mac/Home/repos/github/dwmkerr/sharpshell/SharpShell/Samples/ContextMenu/CopyDirectoryLocationHandler/bin/Debug/CopyDirectoryLocationHandler.dll"
            ));

            var registrationInfo = ComAdmin.GetComServerRegistrationInfo(new Guid("{8b3d441e-f71c-35b4-8cbb-66123a1cb4ca}"));
            Assert.That(registrationInfo.Clsid, Is.EqualTo(new Guid("{8b3d441e-f71c-35b4-8cbb-66123a1cb4ca}")));
            Assert.That(registrationInfo.ClassName, Is.EqualTo("CopyDirectoryLocationHandler"));
            Assert.That(registrationInfo.ComServerPath, Is.EqualTo("mscoree.dll"));
            Assert.That(registrationInfo.ThreadingModel, Is.EqualTo("Both"));
            Assert.That(registrationInfo.DotNetFrameworkServer.Assembly, Is.EqualTo("CopyDirectoryLocationHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fc2d5bf7b4a9e4cb"));
            Assert.That(registrationInfo.DotNetFrameworkServer.Class, Is.EqualTo("CopyDirectoryLocationHandler.CopyDirectoryLocationHandler"));
            Assert.That(registrationInfo.DotNetFrameworkServer.RuntimeVersion, Is.EqualTo("v4.0.30319"));
            Assert.That(registrationInfo.DotNetFrameworkServer.CodeBase, Is.EqualTo("file:////Mac/Home/repos/github/dwmkerr/sharpshell/SharpShell/Samples/ContextMenu/CopyDirectoryLocationHandler/bin/Debug/CopyDirectoryLocationHandler.dll"));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersion, Is.EqualTo("1.0.0.0"));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersion, Is.EqualTo("1.0.0.0"));
            Assert.That(registrationInfo.DotNetCoreServer, Is.Null);
        }

        [Test]
        public void GetComServerRegistrationInfo_Correctly_Identifies_Dot_Net_Core_Server()
        {
            //  Provide the structure that would be present for a .NET Framework server.
            _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
                @"HKEY_CLASSES_ROOT",
                @"   CLSID",
                @"      {93DEE2FF-1446-4119-A78D-60858BD38E9D}",
                @"         (Default) = CoreCLR COMHost Server",
                @"         InprocServer32",
                @"            (Default) = \\\\Mac\\Home\\repos\\github\\dwmkerr\\sharpshell\\SharpShell\\Samples\\ContextMenu\\CountLinesExtension\\bin\\Debug\\netcoreapp3.1\\CountLinesExtension.comhost.dll",
                @"            ThreadingModel = Both",
                @"            ProgID",
                @"               (Default) = CountLinesExtension.CountLinesExtension"
            ));

            var registrationInfo = ComAdmin.GetComServerRegistrationInfo(new Guid("{93DEE2FF-1446-4119-A78D-60858BD38E9D}"));
            Assert.That(registrationInfo.Clsid, Is.EqualTo(new Guid("{93DEE2FF-1446-4119-A78D-60858BD38E9D}")));
            Assert.That(registrationInfo.ClassName, Is.EqualTo(@"CoreCLR COMHost Server"));
            Assert.That(registrationInfo.ComServerPath, Is.EqualTo(@"\\\\Mac\\Home\\repos\\github\\dwmkerr\\sharpshell\\SharpShell\\Samples\\ContextMenu\\CountLinesExtension\\bin\\Debug\\netcoreapp3.1\\CountLinesExtension.comhost.dll"));
            Assert.That(registrationInfo.ThreadingModel, Is.EqualTo(@"Both"));
            Assert.That(registrationInfo.DotNetFrameworkServer, Is.Null);
            Assert.That(registrationInfo.DotNetCoreServer.ProgId, Is.EqualTo(@"CountLinesExtension.CountLinesExtension"));
        }

        [Test]
        public void GetComServerRegistrationInfo_Correctly_Identifies_Native_Server()
        {
            //  Provide the structure that would be present for a .NET Framework server.
            _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
                @"HKEY_CLASSES_ROOT",
                @"   CLSID",
                @"      {93CB110F-9189-4349-BD9F-392D9A4D0096}",
                @"         (Default) = Accessibility Control Panel",
                @"         InprocServer32",
                @"            (Default) = %SystemRoot%\System32\accessibilitycpl.dll",
                @"            ThreadingModel = Apartment"
            ));

            var registrationInfo = ComAdmin.GetComServerRegistrationInfo(new Guid("{93CB110F-9189-4349-BD9F-392D9A4D0096}"));
            Assert.That(registrationInfo.Clsid, Is.EqualTo(new Guid("{93CB110F-9189-4349-BD9F-392D9A4D0096}")));
            Assert.That(registrationInfo.ClassName, Is.EqualTo(@"Accessibility Control Panel"));
            Assert.That(registrationInfo.ComServerPath, Is.EqualTo(@"%SystemRoot%\System32\accessibilitycpl.dll"));
            Assert.That(registrationInfo.ThreadingModel, Is.EqualTo(@"Apartment"));
            Assert.That(registrationInfo.DotNetFrameworkServer, Is.Null);
            Assert.That(registrationInfo.DotNetCoreServer, Is.Null);
        }

        [Test]
        [Ignore("Work in Progress")]
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

        [Test]
        [Ignore("Work in Progress")]
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
            //  Assert.Throws<ClassAlreadyRegisteredException>(() =>
            //    ComAdmin.RegisterDotNetCoreComServer(RegistryView.Registry64, clsid, progId, proxyHostPath));
            Assert.Fail("work in progress");
        }
    }
}
