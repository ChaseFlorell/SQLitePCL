namespace SQLitePCL
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal static class Platform
    {
        private static IPlatform current;

        private static string platformAssemblyName = "SQLitePCL.Ext";

        private static string platformTypeFullName = "SQLitePCL.CurrentPlatform";

        /// <summary>
        /// Gets the current platform. If none is loaded yet, accessing this property triggers platform resolution.
        /// </summary>
        public static IPlatform Instance
        {
            get
            {
                // create if not yet created
                if (current == null)
                {
                    // assume the platform assembly has the same key, same version and same culture
                    // as the assembly where the IPlatformProvider interface lives.
                    var provider = typeof(IPlatform);
                    var asm = new AssemblyName(provider.GetTypeInfo().Assembly.FullName);

                    // change name to the specified name
                    asm.Name = platformAssemblyName;
                    var name = platformTypeFullName + ", " + asm.FullName;

                    // look for the type information but do not throw if not found
                    var type = Type.GetType(name, false);

                    if (type != null)
                    {
                        // create type
                        // since we are the only one implementing this interface
                        // this cast is safe.
                        current = (IPlatform)Activator.CreateInstance(type);
                    }
                    else
                    {
                        // throw
                        ThrowForMissingPlatformAssembly();
                    }
                }

                return current;
            }

            // keep this public so we can set a Platform for unit testing.
            set
            {
                current = value;
            }
        }

        /// <summary>
        /// Method to throw an exception in case no Platform assembly could be found.
        /// </summary>
        private static void ThrowForMissingPlatformAssembly()
        {
            AssemblyName portable = new AssemblyName(typeof(Platform).GetTypeInfo().Assembly.FullName);

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.Platform_AssemblyNotFound, portable.Name, platformAssemblyName));
        }
    }
}
