﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ComAdmin.RegisterServer
{
    public class ComServerRegistrationInfo
    {
        private ComServerRegistrationInfo(ComServerRegistrationType comServerRegistrationType, Guid clsid, string className, string comServerPath, string threadingModel, 
            DotNetFrameworkServerInfo dotNetFrameworkServerInfo, DotNetCoreServerInfo dotNetCoreServerInfo)
        {
            ComServerRegistrationType = comServerRegistrationType;
            Clsid = clsid;
            ClassName = className;
            ComServerPath = comServerPath;
            ThreadingModel = threadingModel;
            DotNetFrameworkServer = dotNetFrameworkServerInfo;
            DotNetCoreServer = dotNetCoreServerInfo;
        }

        public static ComServerRegistrationInfo NewUnknownComServerRegistrationInfo(Guid clsid, string className)
        {
            return new ComServerRegistrationInfo(ComServerRegistrationType.Unknown, clsid, className, null, null, null, null);
        }

        public static ComServerRegistrationInfo NewDotNetFrameworkServerRegistrationInfo(Guid clsid, string className, string comServerPath, string threadingModel, string assembly, string assemblyVersion, string @class, string runtimeVersion, string codeBase)
        {
            return new ComServerRegistrationInfo(
                ComServerRegistrationType.DotNetFrameworkAssembly,
                clsid,
                className,
                comServerPath,
                threadingModel,
                new DotNetFrameworkServerInfo(assembly, assemblyVersion, @class, runtimeVersion, codeBase),
                null);
        }
        public static ComServerRegistrationInfo NewDotNetCoreServerRegistrationInfo(Guid clsid, string className, string comServerPath, string threadingModel, string progId)
        {
            return new ComServerRegistrationInfo(
                ComServerRegistrationType.DotNetCoreAssembly,
                clsid,
                className,
                comServerPath,
                threadingModel,
                null,
                new DotNetCoreServerInfo(progId));
        }

        public static ComServerRegistrationInfo NewNativeServerRegistrationInfo(Guid clsid, string className, string comServerPath, string threadingModel)
        {
            return new ComServerRegistrationInfo(ComServerRegistrationType.NativeDll, clsid, className, comServerPath, threadingModel, null, null);
        }
        
        /// <summary>
        /// The type of the COM server. This is a best-effort assumption based on the keys in the registry.
        /// </summary>
        public ComServerRegistrationType ComServerRegistrationType { get; }

        /// <summary>
        /// The CLSID (Class ID) of the server.
        /// </summary>
        public Guid Clsid { get;  }

        /// <summary>
        /// The Class Name of the server, which is normally the descriptive 'default value' of the CLSID key.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// The path to the actual COM server (which might be a native server, mscoree.dll, a proxy, etc).
        /// </summary>
        public string ComServerPath { get; }

        /// <summary>
        /// Gets the threading model.
        /// </summary>
        public string ThreadingModel { get; }

        /// <summary>
        /// The .NET Framework specific server info. Only set if <see cref="ComServerRegistrationType"/> is <see cref="ComServerRegistrationType.DotNetFrameworkAssembly"/>.
        /// </summary>
        public DotNetFrameworkServerInfo DotNetFrameworkServer { get; }

        /// <summary>
        /// The .NET Framework specific server info. Only set if <see cref="ComServerRegistrationType"/> is <see cref="ComServerRegistrationType.DotNetCoreAssembly"/>.
        /// </summary>
        public DotNetCoreServerInfo DotNetCoreServer { get; }

        public class DotNetFrameworkServerInfo
        {
            public DotNetFrameworkServerInfo(string assembly, string assemblyVersion, string @class, string runtimeVersion, string codeBase)
            {
                Assembly = assembly;
                AssemblyVersion = assemblyVersion;
                Class = @class;
                RuntimeVersion = runtimeVersion;
                CodeBase = codeBase;
            }

            /// <summary>
            /// Gets the assembly version.
            /// </summary>
            public string AssemblyVersion { get; }

            /// <summary>
            /// Gets the assembly.
            /// </summary>
            public string Assembly { get; }

            /// <summary>
            /// Gets the class.
            /// </summary>
            public string Class { get; }

            /// <summary>
            /// Gets the runtime version.
            /// </summary>
            public string RuntimeVersion { get; }

            /// <summary>
            /// Gets the codebase path.
            /// </summary>
            public string CodeBase { get; }
        }

        public class DotNetCoreServerInfo
        {
            public DotNetCoreServerInfo(string progId)
            {
                ProgId = progId;
            }

            /// <summary>
            /// Gets the progam id.
            /// </summary>
            public string ProgId { get; }
        }
    }
}
