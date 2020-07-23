using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Transactions;
using ComAdmin.RegisterServer;
using CommandLine;

namespace ComAdminCli
{
    public class Program
    {
        [Verb("examine", HelpText = "Examine a file to identify whether it hosts COM servers")]
        class ExamineOptions
        {
            [Value(0, Required = true, HelpText = "Path of the file to examine")]
            public string Path { get; set; }
        }

        [Verb("get-server-info", HelpText = "Get info on a server from the registry")]
        class GetServerInfoOptions
        {
            [Value(0, Required = true, HelpText = "The server Class ID (CLSID)")]
            public string Clsid { get; set; }
        }

        static int Main(string[] args)
        {
            //  Note that the 'object' type parameter is needed as we have two provide at least *two*
            //  verbs. If we only have one, we use 'object' as a dummy.
            return CommandLine.Parser.Default.ParseArguments<ExamineOptions, GetServerInfoOptions>(args)
                .MapResult(
                    (ExamineOptions opts) => Examine(opts),
                    (GetServerInfoOptions opts) => GetServerInfo(opts),
                    errs => ErrorHandler(errs));
        }

        static void WriteTitle()
        {
            //  Get the metadata.
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"COM Admin CLI v{fileVersionInfo.ProductVersion}, {fileVersionInfo.LegalCopyright}");
            Console.WriteLine($"https://github/dwmkerr/dotnet-com-admin");
            Console.WriteLine($"");
        }

        static int Examine(ExamineOptions opts)
        {
            //  Let the user know what we're doing.
            WriteTitle();
            Console.WriteLine($"  Examining '{Path.GetFileName(opts.Path)}'...");

            //  Examine the file.
            var examineFileResult = ComAdmin.ComAdmin.ExamineFile(opts.Path);

            //  Write out the file details.
            Console.WriteLine($"    Source File            : {opts.Path}");
            Console.WriteLine($"    File Type              : {examineFileResult.FileType}");
            Console.WriteLine($"    Version Type           : {examineFileResult.Version}");
            Console.WriteLine($"    Processor Architecture : {examineFileResult.ProcessorArchitecture}");
            Console.WriteLine($"    Framework              : {examineFileResult.FrameworkName}");

            return 0;
        }

        static int GetServerInfo(GetServerInfoOptions opts)
        {
            //  Let the user know what we're doing.
            WriteTitle();
            Console.WriteLine($"  Getting info for server with class '{Path.GetFileName(opts.Clsid)}'...");

            //  Try and parse the Guid.
            Guid clsid;
            if (!Guid.TryParse(opts.Clsid, out clsid))
            {
                Console.WriteLine($"Error: Unable to convert '{opts.Clsid}' into a valid CLSID");
                return 1;
            }

            //  Get the server info.
            var registrationInfo = ComAdmin.ComAdmin.GetComServerRegistrationInfo(clsid);

            //  First check for the case that the server is not registered.
            if (registrationInfo == null)
            {
                Console.WriteLine($"No COM server with CLSID {clsid} is registered.");
                return 0;
            }

            //  Bail out for unknown servers.
            if (registrationInfo.ComServerRegistrationType == ComServerRegistrationType.Unknown)
            {
                Console.WriteLine($"COM server with CLSID {clsid} has an entry but appears to be incomplete - it may be corrupted.");
                return 0;
            }


            //  Write out the file details.
            Console.WriteLine($"    Server Type      : {registrationInfo.ComServerRegistrationType}");
            Console.WriteLine($"    Class ID         : {registrationInfo.Clsid}");
            Console.WriteLine($"    Class Name       : {registrationInfo.ClassName}");
            Console.WriteLine($"    Threading Model  : {registrationInfo.ThreadingModel}");

            switch (registrationInfo.ComServerRegistrationType)
            {
                case ComServerRegistrationType.NativeDll:
                    return 0;
                case ComServerRegistrationType.DotNetFrameworkAssembly:
                    Console.WriteLine($"    Assembly         : {registrationInfo.DotNetFrameworkServer.Assembly}");
                    Console.WriteLine($"    Assembly Version : {registrationInfo.DotNetFrameworkServer.AssemblyVersion}");
                    Console.WriteLine($"    Class            : {registrationInfo.DotNetFrameworkServer.Class}");
                    Console.WriteLine($"    Runtime Version  : {registrationInfo.DotNetFrameworkServer.RuntimeVersion}");
                    return 0;
                case ComServerRegistrationType.DotNetCoreAssembly:
                    Console.WriteLine($"    Prog Id Model    : {registrationInfo.DotNetCoreServer.ProgId}");
                    return 0;
                case ComServerRegistrationType.Unknown:
                default:
                    Console.WriteLine($"Error: unknown server type '{registrationInfo}");
                    return 1;
            }
        }

        static int ErrorHandler(IEnumerable<Error> errors)
        {
            return 1;
        }
    }
}
