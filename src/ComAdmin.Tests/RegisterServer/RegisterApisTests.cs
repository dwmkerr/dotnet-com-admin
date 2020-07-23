using System;
using System.IO;
using System.Reflection;
using ComAdmin.Registry;
using Microsoft.Win32;
using NUnit.Framework;

namespace ComAdmin.Tests.RegisterServer
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
