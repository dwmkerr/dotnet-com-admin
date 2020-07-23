﻿using System;
using ComAdmin.ExamineFile;
using ComAdmin.RegisterServer;
using ComAdmin.Registry;
using Microsoft.Win32;

namespace ComAdmin
{
    /// <summary>
    /// The ComAdmin class contains the main APIs which are provided by this library to support COM administration.
    /// </summary>
    public class ComAdmin
    {
        /// <summary>
        /// Examine a given file to determine whether it is an assembly or native dll, and whether
        /// it contains shell extensions.
        /// </summary>
        /// <param name="filePath">The path to the file to examine.</param>
        /// <returns>The details of the file to examine.</returns>
        public static ExamineFileResult ExamineFile(string filePath)
        {
            return ExamineFileApi.ExamineFile(filePath);
        }
        public static void RegisterDotNetCoreComServer(RegistryView registryView, Guid clsid, string programId, string comHostPath)
        {
            RegisterServerApis.RegisterDotNetCoreComServer(_registry, registryView, clsid, programId, comHostPath);
        }

        public static ComServerRegistrationInfo GetComServerRegistrationInfo(Guid clsid)
        {
            return RegisterServerApis.GetComServerRegistrationInfo(_registry, RegistryView.Registry64, clsid);
        }

        /// <summary>
            /// Set the underlying registry implementation used by the <see cref="ComAdmin" /> APIs.
            /// The default implementation is the standard <see cref="WindowsRegistry" /> class, which is
            /// just a wrapper around the standard Windows Registry.
            /// </summary>
            /// <param name="registry">The registry implementation to use for <see cref="ComAdmin" /> APIs.</param>
            public static void SetRegistryImplementation(IRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// By default, we use the Windows Registry for all operations. You should not need to change
        /// this unless you are trying to run specific test cases.
        /// </summary>
        private static IRegistry _registry = new WindowsRegistry();
    }
}
