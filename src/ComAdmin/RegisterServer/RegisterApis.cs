using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using ComAdmin.Extensions;
using ComAdmin.Registry;
using Microsoft.Win32;

namespace ComAdmin.RegisterServer
{
    public class RegisterServerApis
    {
        public static void RegisterDotNetCoreComServer(IRegistry registry, RegistryView registryView, Guid clsid, string programId, string comHostPath)
        {
            throw new NotImplementedException();
        }
    }
}
