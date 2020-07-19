using System;
using System.IO;
using System.Reflection;
using ComAdmin.ExamineFile;
using NUnit.Framework;

namespace ComAdmin.Tests.ExamineFile
{
    public class ExamineFileTests
    {
        [Test]
        public void ExamineFile_Can_Identify_A_Native_Dll()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "NativeDll.dll");
            var fileShellExtensions = ComAdmin.ExamineFile(path);
            Assert.That(fileShellExtensions.FileType, Is.EqualTo(FileType.NativeDll));
            Assert.That(fileShellExtensions.Version, Is.Null);
            Assert.That(fileShellExtensions.ProcessorArchitecture, Is.EqualTo(ProcessorArchitecture.None));
            Assert.That(fileShellExtensions.FrameworkName, Is.Null);
        }

        [Test]
        public void ExamineFile_Can_Identify_A_Dot_Net_Framework_Assembly()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "DotNetFrameworkComServer.dll");
            var fileShellExtensions = ComAdmin.ExamineFile(path);
            Assert.That(fileShellExtensions.FileType, Is.EqualTo(FileType.DotNetFrameworkAssembly));
            Assert.That(fileShellExtensions.Version, Is.EqualTo(new Version(1, 0,0, 0)));
            Assert.That(fileShellExtensions.ProcessorArchitecture, Is.EqualTo(ProcessorArchitecture.MSIL));
            Assert.That(fileShellExtensions.FrameworkName, Is.EqualTo(".NETFramework,Version=v4.7.2"));
        }

        [Test]
        public void ExamineFile_Can_Identify_A_Dot_Net_Core_Assembly()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "DotNetCoreComServer.dll");
            var fileShellExtensions = ComAdmin.ExamineFile(path);
            Assert.That(fileShellExtensions.FileType, Is.EqualTo(FileType.DotNetCoreAssembly));
            Assert.That(fileShellExtensions.Version, Is.EqualTo(new Version(1, 0, 0, 0)));
            Assert.That(fileShellExtensions.ProcessorArchitecture, Is.EqualTo(ProcessorArchitecture.MSIL));
            Assert.That(fileShellExtensions.FrameworkName, Is.EqualTo(".NETCoreApp,Version=v3.1"));
        }
    }
}
