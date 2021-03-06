﻿// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

namespace SQLitePCL
{
    using System.IO;
    using Windows.Storage;

    /// <summary>
    /// Implements the <see cref="IPlatformStorage"/> interface for Windows Store.
    /// </summary>
    internal sealed class PlatformStorage : IPlatformStorage
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformStorage"/>.
        /// </summary>
        private static IPlatformStorage instance = new PlatformStorage();

        private PlatformStorage()
        {
        }

        /// <summary>
        /// A singleton instance of the <see cref="PlatformStorage"/>.
        /// </summary>
        internal static IPlatformStorage Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Returns a platform-specific local file path.
        /// </summary>
        string IPlatformStorage.GetLocalFilePath(string filename)
        {
            var result = filename;

            if (!Path.IsPathRooted(filename))
            {
                result = Path.Combine(ApplicationData.Current.LocalFolder.Path, filename);
            }

            return result;
        }

        /// <summary>
        /// Returns a platform-specific temporary directory path.
        /// </summary>
        string IPlatformStorage.GetTemporaryDirectoryPath()
        {
            return ApplicationData.Current.TemporaryFolder.Path;
        }
    }
}
