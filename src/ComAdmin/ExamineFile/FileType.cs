namespace ComAdmin.ExamineFile
{
    /// <summary>
    /// This enumeration represents the type of file identified by the <see cref="ComAdmin.ExamineFile" /> API.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// The file is not an assembly or native dll.
        /// </summary>
        Unknown,

        /// <summary>
        /// The file is a native Windows DLL.
        /// </summary>
        NativeDll,

        /// <summary>
        /// The file is an assembly which targets the .NET Framework.
        /// </summary>
        DotNetFrameworkAssembly,

        /// <summary>
        /// The file is an assembly which targets .NET Core.
        /// </summary>
        DotNetCoreAssembly
    }
}
