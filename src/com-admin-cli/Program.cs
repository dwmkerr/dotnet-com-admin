using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Transactions;
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

        static int Main(string[] args)
        {
            //  Note that the 'object' type parameter is needed as we have two provide at least *two*
            //  verbs. If we only have one, we use 'object' as a dummy.
            return CommandLine.Parser.Default.ParseArguments<ExamineOptions, object>(args)
                .MapResult(
                    (ExamineOptions opts) => Examine(opts),
                    (object opts) => 1,
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

        static int ErrorHandler(IEnumerable<Error> errors)
        {
            return 1;
        }
    }
}
