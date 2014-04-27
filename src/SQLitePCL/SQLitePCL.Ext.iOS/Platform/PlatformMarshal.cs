// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

namespace SQLitePCL
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FunctionNativeCdecl(IntPtr context, int numberOfArguments, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] arguments);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void AggregateStepNativeCdecl(IntPtr context, int numberOfArguments, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] arguments);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void AggregateFinalNativeCdecl(IntPtr context);

	// unwrap the result of MarshalDelegateToNativeFunctionPointer as we are
	// not going to be using this pointer, but rather the pointer to this 
	// object, which will contain the references to the function that the 
	// current pointer points to.
	internal struct Sqlite3FunctionMarshallingProxy
	{
		// functions
		public readonly FunctionNativeCdecl Invoke;

		// aggregates
		public readonly AggregateStepNativeCdecl Step;
		public readonly AggregateFinalNativeCdecl Final;

		public Sqlite3FunctionMarshallingProxy(IntPtr invoke)
		{
			var invokeFunction = GCHandle.FromIntPtr(invoke);
			Invoke = invokeFunction.Target as FunctionNativeCdecl;
			invokeFunction.Free();

			Step = null;
			Final = null;
		}

		public Sqlite3FunctionMarshallingProxy(IntPtr step, IntPtr final)
		{
			var stepFunction = GCHandle.FromIntPtr(step);
			Step = stepFunction.Target as AggregateStepNativeCdecl;
			stepFunction.Free();

			var finalFunction = GCHandle.FromIntPtr(final);
			Final = finalFunction.Target as AggregateFinalNativeCdecl;
			finalFunction.Free();

			Invoke = null;
		}

		public IntPtr ToIntPtr()
		{
			// TODO: this allocates a new GC handle that needs to be freed somewhere...
			return GCHandle.ToIntPtr(GCHandle.Alloc(this));
		}

		public static Sqlite3FunctionMarshallingProxy FromIntPtr(IntPtr gcHandleIntPtr)
		{
			GCHandle gcHandle = GCHandle.FromIntPtr(gcHandleIntPtr);
			return (Sqlite3FunctionMarshallingProxy)gcHandle.Target;
		}
	}

    /// <summary>
    /// Implements the <see cref="IPlatformMarshal"/> interface for Xamarin iOS.
    /// </summary>
    internal sealed class PlatformMarshal : IPlatformMarshal
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
        internal static IPlatformMarshal Instance
        {
            get
            {
                return instance;
            }
        }

        void IPlatformMarshal.CleanUpStringNativeUTF8(IntPtr nativeString)
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

        string IPlatformMarshal.MarshalStringNativeUTF8ToManaged(IntPtr nativeString)
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

        int IPlatformMarshal.GetNativeUTF8Size(IntPtr nativeString)
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

        Delegate IPlatformMarshal.ApplyNativeCallingConventionToFunction(FunctionNative function)
        {
            return new FunctionNativeCdecl((context, numberOfArguments, arguments) => { function.Invoke(context, numberOfArguments, arguments); });
        }

        Delegate IPlatformMarshal.ApplyNativeCallingConventionToAggregateStep(AggregateStepNative step)
        {
            return new AggregateStepNativeCdecl((context, numberOfArguments, arguments) => { step.Invoke(context, numberOfArguments, arguments); });
        }

        Delegate IPlatformMarshal.ApplyNativeCallingConventionToAggregateFinal(AggregateFinalNative final)
        {
            return new AggregateFinalNativeCdecl((context) => { final.Invoke(context); });
        }

        IntPtr IPlatformMarshal.MarshalDelegateToNativeFunctionPointer(Delegate del)
        {
            // don't actually get a pointer to the function, just a GC handle,
            // that is then used and freed by the proxy class.
            return GCHandle.ToIntPtr(GCHandle.Alloc(del));
        }

        void IPlatformMarshal.Copy(IntPtr source, byte[] destination, int startIndex, int length)
        {
            Marshal.Copy(source, destination, startIndex, length);
        }

        void IPlatformMarshal.Copy(byte[] source, IntPtr destination, int startIndex, int length)
        {
            Marshal.Copy(source, startIndex, destination, length);
        }
    }
}
