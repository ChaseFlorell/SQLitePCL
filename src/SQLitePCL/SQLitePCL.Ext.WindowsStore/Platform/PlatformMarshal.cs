namespace SQLitePCL
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Implements the <see cref="IPlatformMarshal"/> interface for Windows Store.
    /// </summary>
    internal class PlatformMarshal : IPlatformMarshal
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformMarshal"/>.
        /// </summary>
        private static IPlatformMarshal instance = new PlatformMarshal();

        private PlatformMarshal()
        {
        }

        /// <summary>
        /// A singleton instance of the <see cref="PlatformMarshal"/>.
        /// </summary>
        public static IPlatformMarshal Instance
        {
            get
            {
                return instance;
            }
        }

        void IPlatformMarshal.CleanUpStringNativeUTF8(System.IntPtr nativeString)
        {
            Marshal.FreeHGlobal(nativeString);
        }

        IntPtr IPlatformMarshal.MarshalStringManagedToNativeUTF8(string managedString)
        {
            int size;

            return ((IPlatformMarshal)this).MarshalStringManagedToNativeUTF8(managedString, out size);
        }

        IntPtr IPlatformMarshal.MarshalStringManagedToNativeUTF8(string managedString, out int size)
        {
            var result = IntPtr.Zero;
            size = 0;

            if (managedString != null)
            {
                var array = Encoding.UTF8.GetBytes(managedString);
                size = array.Length + 1;
                result = Marshal.AllocHGlobal(size);
                Marshal.Copy(array, 0, result, array.Length);
                Marshal.WriteByte(result, size - 1, 0);
            }

            return result;
        }

        string IPlatformMarshal.MarshalStringNativeUTF8ToManaged(System.IntPtr nativeString)
        {
            string result = null;

            if (nativeString != IntPtr.Zero)
            {
                int size = ((IPlatformMarshal)this).GetNativeUTF8Size(nativeString);
                var array = new byte[size - 1];
                Marshal.Copy(nativeString, array, 0, size - 1);
                result = Encoding.UTF8.GetString(array, 0, array.Length);
            }

            return result;
        }

        int IPlatformMarshal.GetNativeUTF8Size(System.IntPtr nativeString)
        {
            var offset = 0;

            if (nativeString != IntPtr.Zero)
            {
                while (Marshal.ReadByte(nativeString, offset) > 0)
                {
                    offset++;
                }

                offset++;
            }

            return offset;
        }

        void IPlatformMarshal.Copy(System.IntPtr source, byte[] destination, int startIndex, int length)
        {
            Marshal.Copy(source, destination, startIndex, length);
        }
    }
}
