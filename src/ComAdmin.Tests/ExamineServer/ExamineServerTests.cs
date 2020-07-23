using System;
using System.IO;
using System.Reflection;
using ComAdmin.Registry;
using Microsoft.Win32;
using NUnit.Framework;

namespace ComAdmin.Tests.ExamineServer
{
    public class ExamineServerTests
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
        public void ExamineServer_Correctly_Identifies_Unregistered_Server()
        {
            //  No servers, so we shouldn't find anything here...
            //  Provide the structure that would be present for a .NET Framework server.
            _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
                @"HKEY_CLASSES_ROOT",
                @"   CLSID"
            ));

            Assert.That(ComAdmin.ExamineServer(new Guid("{00000000-1111-2222-3333-444444444444}")), Is.Null);
        }

        [Test]
        public void ExamineServer_Correctly_Identifies_Dot_Net_Framework_Server()
        {
            //  Provide the structure that would be present for a .NET Framework server. This is based on DotFrameworkComServer.
            _registry.AddStructure(RegistryView.Registry64, string.Join(Environment.NewLine,
                @"HKEY_CLASSES_ROOT",
                @"   CLSID",
                @"      {00000000-0000-0000-C0C0-000000000002}",
                @"         (Default) = DotNetFrameworkComServer.DotFrameworkComServer",
                @"         Implemented Categories",
                @"            {62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}",
                @"         InprocServer32",
                @"            (Default) = mscoree.dll",
                @"            Assembly = DotNetFrameworkComServer, Version=0.1.2.3, Culture=neutral, PublicKeyToken=4de4d67b1b2c36c0",
                @"            Class = DotNetFrameworkComServer.DotFrameworkComServer",
                @"            RuntimeVersion = v4.0.30319",
                @"            ThreadingModel = Both",
                @"            CodeBase = file://Mac/Home/repos/github/dwmkerr/dotnet-com-admin/src/ComAdmin.Tests/TestFiles/DotNetFrameworkComServer.DLL",
                @"            0.1.2.3",
                @"               Assembly = DotNetFrameworkComServer, Version=0.1.2.3, Culture=neutral, PublicKeyToken=4de4d67b1b2c36c0",
                @"               Class = DotNetFrameworkComServer.DotFrameworkComServer",
                @"               RuntimeVersion = v4.0.30319",
                @"               CodeBase = file://Mac/Home/repos/github/dwmkerr/dotnet-com-admin/src/ComAdmin.Tests/TestFiles/DotNetFrameworkComServer.DLL",
                @"         ProgId",
                @"            (Default) = DotNetFrameworkComServer.DotFrameworkComServer"
            ));

            var registrationInfo = ComAdmin.ExamineServer(new Guid("{00000000-0000-0000-C0C0-000000000002}"));
            Assert.That(registrationInfo.Clsid, Is.EqualTo(new Guid("{00000000-0000-0000-C0C0-000000000002}")));
            Assert.That(registrationInfo.ClassName, Is.EqualTo("DotNetFrameworkComServer.DotFrameworkComServer"));
            Assert.That(registrationInfo.ComServerPath, Is.EqualTo("mscoree.dll"));
            Assert.That(registrationInfo.ThreadingModel, Is.EqualTo("Both"));
            Assert.That(registrationInfo.DotNetFrameworkServer.RootAssemblyInfo.Assembly, Is.EqualTo("DotNetFrameworkComServer, Version=0.1.2.3, Culture=neutral, PublicKeyToken=4de4d67b1b2c36c0"));
            Assert.That(registrationInfo.DotNetFrameworkServer.RootAssemblyInfo.Class, Is.EqualTo("DotNetFrameworkComServer.DotFrameworkComServer"));
            Assert.That(registrationInfo.DotNetFrameworkServer.RootAssemblyInfo.RuntimeVersion, Is.EqualTo("v4.0.30319"));
            Assert.That(registrationInfo.DotNetFrameworkServer.RootAssemblyInfo.CodeBase, Is.EqualTo("file://Mac/Home/repos/github/dwmkerr/dotnet-com-admin/src/ComAdmin.Tests/TestFiles/DotNetFrameworkComServer.DLL"));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersions.Count, Is.EqualTo(1));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersions["0.1.2.3"].Assembly, Is.EqualTo("DotNetFrameworkComServer, Version=0.1.2.3, Culture=neutral, PublicKeyToken=4de4d67b1b2c36c0"));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersions["0.1.2.3"].Class, Is.EqualTo("DotNetFrameworkComServer.DotFrameworkComServer"));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersions["0.1.2.3"].RuntimeVersion, Is.EqualTo("v4.0.30319"));
            Assert.That(registrationInfo.DotNetFrameworkServer.AssemblyVersions["0.1.2.3"].CodeBase, Is.EqualTo("file://Mac/Home/repos/github/dwmkerr/dotnet-com-admin/src/ComAdmin.Tests/TestFiles/DotNetFrameworkComServer.DLL"));
            Assert.That(registrationInfo.DotNetCoreServer, Is.Null);
        }

        [Test]
        public void ExamineServer_Correctly_Identifies_Dot_Net_Core_Server()
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

            var registrationInfo = ComAdmin.ExamineServer(new Guid("{93DEE2FF-1446-4119-A78D-60858BD38E9D}"));
            Assert.That(registrationInfo.Clsid, Is.EqualTo(new Guid("{93DEE2FF-1446-4119-A78D-60858BD38E9D}")));
            Assert.That(registrationInfo.ClassName, Is.EqualTo(@"CoreCLR COMHost Server"));
            Assert.That(registrationInfo.ComServerPath,
                Is.EqualTo(
                    @"\\\\Mac\\Home\\repos\\github\\dwmkerr\\sharpshell\\SharpShell\\Samples\\ContextMenu\\CountLinesExtension\\bin\\Debug\\netcoreapp3.1\\CountLinesExtension.comhost.dll"));
            Assert.That(registrationInfo.ThreadingModel, Is.EqualTo(@"Both"));
            Assert.That(registrationInfo.DotNetFrameworkServer, Is.Null);
            Assert.That(registrationInfo.DotNetCoreServer.ProgId,
                Is.EqualTo(@"CountLinesExtension.CountLinesExtension"));
        }

        [Test]
        public void ExamineServer_Correctly_Identifies_Native_Server()
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

            var registrationInfo = ComAdmin.ExamineServer(new Guid("{93CB110F-9189-4349-BD9F-392D9A4D0096}"));
            Assert.That(registrationInfo.Clsid, Is.EqualTo(new Guid("{93CB110F-9189-4349-BD9F-392D9A4D0096}")));
            Assert.That(registrationInfo.ClassName, Is.EqualTo(@"Accessibility Control Panel"));
            Assert.That(registrationInfo.ComServerPath, Is.EqualTo(@"%SystemRoot%\System32\accessibilitycpl.dll"));
            Assert.That(registrationInfo.ThreadingModel, Is.EqualTo(@"Apartment"));
            Assert.That(registrationInfo.DotNetFrameworkServer, Is.Null);
            Assert.That(registrationInfo.DotNetCoreServer, Is.Null);
        }

    }
}
