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

        /// <summary>
        /// This function attempts to identify whether a server with a given CLSID is registered, and then based on the registration
        /// details attempts to determine whether it is a Native DLL, a .NET Framework Assembly or a .NET Core Assembly.
        /// </summary>
        /// <param name="registry">The registry implementation.</param>
        /// <param name="registryView">The registry view.</param>
        /// <param name="clsid">The CLSID of the server to check for.</param>
        /// <returns>A <see cref="ComServerRegistrationInfo" /> object if we know a server is registered, OR null if we know the
        /// server is not registered.</returns>
        public static ComServerRegistrationInfo GetComServerRegistrationInfo(IRegistry registry, RegistryView registryView, Guid clsid)
        {
            //  Open the classes key then search for a key for the server.
            using (var classesKey = OpenClassesKey(registry, registryView, RegistryKeyPermissionCheck.ReadSubTree))
            using (var serverClassKey = classesKey.OpenSubKey(clsid.ToRegistryString()))
            {
                //  If there's no subkey, the server isn't registered.
                if (serverClassKey == null)
                    return null;

                //  Every server *should* have a name for the class.
                var className = serverClassKey.GetValue(null, null)?.ToString();

                //  Try to load in InProc32Server key...
                using (var inproc32ServerKey = serverClassKey.OpenSubKey(KeyNameInProc32))
                {
                    //  ...if it's not there, then *something* is registered, but we don't have a clue what it is.
                    //  It could be something which has been badly installed or something corrupted.
                    if (inproc32ServerKey == null) return ComServerRegistrationInfo.NewUnknownComServerRegistrationInfo(clsid, className);

                    //  Get the default value, this will be the actual DLL which will host the COM server. It might
                    //  be a server itself, it might be a proxy, we will see...
                    var inprocServer32 = inproc32ServerKey.GetValue(null, null)?.ToString();

                    //  Again, if we have no InProc32Server value, we've got something that we cannot activate and
                    //  is most likely broken.
                    if (string.IsNullOrEmpty(inprocServer32)) return ComServerRegistrationInfo.NewUnknownComServerRegistrationInfo(clsid, className);

                    //  Get the threading model, which should be present for *all* servers.
                    var threadingModel = inproc32ServerKey.GetValue(KeyNameThreadingModel, null)?.ToString();

                    //  If the class name is 'CoreCLR COMHost Server' this is a .NET Core server. This is a fragile way
                    //  to identify the server type, but seems to be the only way.
                    if (className == KeyValueCoreClrComHostServer)
                    {
                        using (var progIdSubKey = inproc32ServerKey.OpenSubKey(KeyNameProgId))
                        {
                            return ComServerRegistrationInfo.NewDotNetCoreServerRegistrationInfo(clsid, className, inprocServer32, threadingModel, progIdSubKey?.GetValue(null, null)?.ToString());
                        }
                    }

                    //  OK, if the inproc server is 'mscoree.dll' then that's the .NET Framework entrypoint and we have a .NET Framework
                    //  COM server. Use 'EndsWith' as it might be a path we have.
                    if (inprocServer32.EndsWith(KeyValueNetFrameworkServer, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //  We've got a .NET server. We should have one subkey, with the assembly version.
                        var assemblyVersion = inproc32ServerKey.GetSubKeyNames().FirstOrDefault();

                        //  If we have no subkey name, we've got a partially registered or most likely corrupted server.
                        if (assemblyVersion == null) return ComServerRegistrationInfo.NewUnknownComServerRegistrationInfo(clsid, className);

                        //  Open the assembly subkey.
                        using (var assemblySubkey = inproc32ServerKey.OpenSubKey(assemblyVersion))
                        {
                            //  If we can't open the key, we've got a problem.
                            if (assemblySubkey == null) return ComServerRegistrationInfo.NewUnknownComServerRegistrationInfo(clsid, className);

                            //  Get the assembly details.
                            var assembly = assemblySubkey.GetValue(KeyNameAssembly)?.ToString();
                            var @class = assemblySubkey.GetValue(KeyNameClass)?.ToString();
                            var runtimeVersion = assemblySubkey.GetValue(KeyNameRuntimeVersion)?.ToString();
                            var codeBase = assemblySubkey.GetValue(KeyNameCodeBase, null)?.ToString();

                            //  Return the server info.
                            return ComServerRegistrationInfo.NewDotNetFrameworkServerRegistrationInfo(
                                clsid,
                                className,
                                inprocServer32,
                                threadingModel,
                                assembly,
                                assemblyVersion,
                                @class,
                                runtimeVersion,
                                codeBase);
                        }
                    }

                    //  We've got a native COM server.
                    return ComServerRegistrationInfo.NewNativeServerRegistrationInfo(clsid, className, inprocServer32, threadingModel);
                }
            }
        }

        /// <summary>
        /// Opens the classes key for the given registry view with the given permissions.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="registryView">The view of the registry (i.e. 32 or 64 bit).</param>
        /// <param name="permissions">The required permissions.</param>
        /// <returns>The classes registry key.</returns>
        private static IRegistryKey OpenClassesKey(IRegistry registry, RegistryView registryView, RegistryKeyPermissionCheck permissions)
        {
            //  Open classes the classes key based on the given view.
            var classesKey = registry
                .OpenBaseKey(RegistryHive.ClassesRoot, registryView)
                .OpenSubKey(KeyNameClasses, permissions, RegistryRights.QueryValues | RegistryRights.ReadPermissions | RegistryRights.EnumerateSubKeys);
            
            //  If we got back null, something has gone seriously wrong. Otherwise just give back the key.
            if (classesKey == null)
                throw new InvalidOperationException($"Unable to open the classes key for the registry view '{registryView}' with permissions '{permissions}'.");
            return classesKey;
        }

        //  Well-defined key names and values commonly used for .NET COM servers.
        private const string KeyNameClasses = @"CLSID";
        private const string KeyNameInProc32 = @"InprocServer32";

        //  Well-defined key names and values used specifically for .NET Framework COM servers.
        private const string KeyValueNetFrameworkServer = @"mscoree.dll";
        private const string KeyNameThreadingModel = @"ThreadingModel";
        private const string KeyNameAssembly = @"Assembly";
        private const string KeyNameClass = @"Class";
        private const string KeyNameRuntimeVersion = @"RuntimeVersion";
        private const string KeyNameCodeBase = @"CodeBase";

        //  Well-defined key names and values used specifically for .NET Core COM servers.
        private const string KeyValueCoreClrComHostServer = @"CoreCLR COMHost Server";
        private const string KeyNameProgId = @"ProgId";
    }
}
