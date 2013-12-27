namespace SQLitePCL
{
    using System;

    /// <summary>
    /// An interface for platform-specific assemblies to implement to support 
    /// Marshaling operations.
    /// </summary>
    public interface IPlatformMarshal
    {
        void CleanUpStringNativeUTF8(IntPtr nativeString);

        IntPtr MarshalStringManagedToNativeUTF8(string managedString);

        IntPtr MarshalStringManagedToNativeUTF8(string managedString, out int size);

        string MarshalStringNativeUTF8ToManaged(IntPtr nativeString);

        int GetNativeUTF8Size(IntPtr nativeString);

        void Copy(IntPtr source, byte[] destination, int startIndex, int length);
    }
}
