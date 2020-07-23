using System;
using System.IO;
using System.Linq;
using System.Reflection;

//  We only reference the PE code if we are using .NET Core.
#if NETCOREAPP
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
#endif


namespace ComAdmin.ExamineFile
{
    /// <summary>
    /// This class provides the actual functionality required to examine a file. This works differently between the .NET
    /// Framework and .NET Core, so requires conditional compilation and more sophistiacted testing.
    /// </summary>
    public static class ExamineFileApi
    {
        /// <summary>
        /// The internal API to examine a file. Exposed via the <see cref="ComAdmin" /> static class.
        /// </summary>
        /// <param name="path">The path to the file to examine.</param>
        internal static ExamineFileResult ExamineFile(string path)
        {
#if NET40
            throw new PlatformNotSupportedException("The ExamineFile API is not available on the .NET Framework 4");
#elif NETSTANDARD
            throw new PlatformNotSupportedException("The ExamineFile API is not available on .NET Standard 2.0");
#else
            //  Get the assembly info.
            try
            {
                //  Get the assembly name - this is the first way to check whether it's actually a valid assembly, and also get the version number.
                var assemblyName = AssemblyName.GetAssemblyName(path);

                //  Loading metadata is a platform specific thing, feel like I'm back in C land :)
#if NETFRAMEWORK
                //  Now we need to actually load the assembly to get more metadata with reflection only loading.
                var assembly = Assembly.ReflectionOnlyLoadFrom(path);
                var customAttributes = assembly.GetCustomAttributesData();
                var framework = customAttributes
                    .Single(ca => ca.AttributeType.Name == "TargetFrameworkAttribute")
                    .ConstructorArguments[0]
                    .Value
                    .ToString();
#elif NETCOREAPP

                //  Load the metadata with the charming MetadataReader.
                //  Thanks so much https://gist.github.com/jbe2277/f91ef12df682f3bfb6293aabcb47be2a otherwise I would be lost.
                //  This is kludgy, but we have a big open issue to pull all of this into a dedicated library anyway.
                string framework = null;
                using (var stream = File.OpenRead(path))
                using (var reader = new PEReader(stream))
                {
                    var metadata = reader.GetMetadataReader();
                    var assembly = metadata.GetAssemblyDefinition();
                    foreach (var attribute in assembly.GetCustomAttributes().Select(metadata.GetCustomAttribute))
                    {
                        var ctor = metadata.GetMemberReference((MemberReferenceHandle) attribute.Constructor);
                        var attrType = metadata.GetTypeReference((TypeReferenceHandle) ctor.Parent);
                        var attrName = metadata.GetString(attrType.Name);
                        if (attrName != "TargetFrameworkAttribute") continue;
                        var attrValue = attribute.DecodeValue(new StringAttributeTypeProvider());
                        framework = attrValue.FixedArguments[0].Value.ToString();
                        break;
                    }
                }
#endif
                //  This is risky and brittle to my mind, but at the moment I'm not sure if we have a better way of knowing
                //  whether we are .NET Core or not.
                var fileType = framework != null && framework.StartsWith(".NETCore") // e.g. .NETCoreApp, Version 3.1
                    ? FileType.DotNetCoreAssembly
                    : FileType.DotNetFrameworkAssembly;

                return new ExamineFileResult(fileType, assemblyName.Version,
                    assemblyName.ProcessorArchitecture, framework);
            }
            catch (BadImageFormatException)
            {
                //  This exception is thrown if the file is a Win32 native dll.
                return new ExamineFileResult(FileType.NativeDll, null, ProcessorArchitecture.None, null);
            }
#endif
        }

#if NETCOREAPP
        /// <summary>
        /// This class is used internally only to decode a string valued custom attribute
        /// from assembly metadata. See the <see cref="ExamineFileApi"/> API for
        /// the usage.
        /// </summary>
        internal class StringAttributeTypeProvider : ICustomAttributeTypeProvider<string>
        {
            public string GetPrimitiveType(PrimitiveTypeCode typeCode)
            {
                //  We can only decode string values - anything else is invalid.
                if (typeCode != PrimitiveTypeCode.String)
                {
                    throw new InvalidOperationException($"Unable to decode type '{typeCode}'");
                }

                //  Return the full name of the string class, this is sufficient to allow the DecodeValue
                //  calls to work.
                return typeof(string).FullName;
            }

            public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            {
                throw new NotImplementedException();
            }

            public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            {
                throw new NotImplementedException();
            }

            public string GetSZArrayType(string elementType)
            {
                throw new NotImplementedException();
            }

            public string GetSystemType()
            {
                throw new NotImplementedException();
            }

            public string GetTypeFromSerializedName(string name)
            {
                throw new NotImplementedException();
            }

            public PrimitiveTypeCode GetUnderlyingEnumType(string type)
            {
                throw new NotImplementedException();
            }

            public bool IsSystemType(string type)
            {
                throw new NotImplementedException();
            }
        }
#endif
    }
}
