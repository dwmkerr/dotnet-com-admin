namespace ComAdmin.ExamineServer
{
    /// <summary>
    /// This enumeration represents the type of file identified by the <see cref="ExamineServerApis.ExamineServer" /> API.
    /// </summary>
    public enum ComServerRegistrationType
    {
        /// <summary>
        /// A server is registered, but we cannot identify its type. This should only be the case if there is no 'InProcServer32' key.
        /// </summary>
        Unknown,

        /// <summary>
        /// The COM server appears to be a Native DLL.
        /// </summary>
        NativeDll,

        /// <summary>
        /// The COM server is in an assembly which targets the .NET Framework and is hosted my mscoree.dll.
        /// </summary>
        DotNetFrameworkAssembly,

        /// <summary>
        /// The COM server is in an assembly which targets .NET Core and is hosted by a proxy dll.
        /// </summary>
        DotNetCoreAssembly
    }
}
