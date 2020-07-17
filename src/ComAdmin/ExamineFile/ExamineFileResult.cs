using System;
using System.Reflection;

namespace ComAdmin.ExamineFile
{
    /// <summary>
    /// Provides details on a file and the shell extensions it contains.
    /// </summary>
    public class ExamineFileResult
    {
        public ExamineFileResult(FileType fileType, Version version, ProcessorArchitecture processorArchitecture, string frameworkName)
        {
            FileType = fileType;
            Version = version;
            ProcessorArchitecture = processorArchitecture;
            FrameworkName = frameworkName;
        }

        /// <summary>
        /// The file type.
        /// </summary>
        public FileType FileType { get; }

        /// <summary>
        /// The assembly version. Only set if the <see cref="FileType"/> is an assembly.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The processor architecture. If the file is not an assembly, this is set to <see cref="ProcessorArchitecture.None"/>. 
        /// </summary>
        public ProcessorArchitecture ProcessorArchitecture { get; }

        /// <summary>
        /// The frameowork name. Only set if the <see cref="FileType"/> is an assembly.
        /// </summary>
        public string FrameworkName { get; }
    }
}
