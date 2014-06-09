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

    public class SQLiteStatement : ISQLiteStatement
    {
        private IPlatformMarshal platformMarshal;

        private ISQLite3Provider sqlite3Provider;

        private SQLiteConnection connection;

        private IntPtr stm;

        private bool disposed = false;

        internal SQLiteStatement(SQLiteConnection connection, IntPtr stm)
        {
            this.platformMarshal = Platform.Instance.PlatformMarshal;
            this.sqlite3Provider = Platform.Instance.SQLite3Provider;

            this.connection = connection;
            this.stm = stm;
        }

        ~SQLiteStatement()
        {
            this.Dispose(false);
        }

        public ISQLiteConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public int ColumnCount
        {
            get
            {
                return this.sqlite3Provider.Sqlite3ColumnCount(this.stm);
            }
        }

        public int DataCount
        {
            get
            {
                return this.sqlite3Provider.Sqlite3DataCount(this.stm);
            }
        }

        public object this[int index]
        {
            get
            {
                object result = null;

                var type = (SQLiteType)this.sqlite3Provider.Sqlite3ColumnType(this.stm, index);

                switch (type)
                {
                    case SQLiteType.INTEGER:
                        result = this.sqlite3Provider.Sqlite3ColumnInt64(this.stm, index);
                        break;
                    case SQLiteType.FLOAT:
                        result = this.sqlite3Provider.Sqlite3ColumnDouble(this.stm, index);
                        break;
                    case SQLiteType.TEXT:
                        var textPointer = this.sqlite3Provider.Sqlite3ColumnText(this.stm, index);
                        result = this.platformMarshal.MarshalStringNativeUTF8ToManaged(textPointer);
                        break;
                    case SQLiteType.BLOB:
                        var blobPointer = this.sqlite3Provider.Sqlite3ColumnBlob(this.stm, index);
                        var length = this.sqlite3Provider.Sqlite3ColumnBytes(this.stm, index);
                        result = new byte[length];
                        this.platformMarshal.Copy(blobPointer, (byte[])result, 0, length);
                        break;
                    case SQLiteType.NULL:
                        break;
                }

                return result;
            }
        }

        public SQLiteResult Step()
        {
            return (SQLiteResult)this.sqlite3Provider.Sqlite3Step(this.stm);
        }

        public string ColumnName(int index)
        {
            return this.platformMarshal.MarshalStringNativeUTF8ToManaged(this.sqlite3Provider.Sqlite3ColumnName(this.stm, index));
        }

        public void Reset()
        {
            if (this.sqlite3Provider.Sqlite3Reset(this.stm) != (int)SQLiteResult.OK)
            {
                var errmsg = this.connection.ErrorMessage();
                throw new SQLiteException(errmsg);
            }
        }

        public void Bind(int index, object value)
        {
            var invokeResult = 0;

            if (value == null)
            {
                invokeResult = this.sqlite3Provider.Sqlite3BindNull(this.stm, index);
            }
            else
            {
                if (value is int)
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindInt(this.stm, index, (int)value);
                }
                else if (value is long)
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindInt64(this.stm, index, (long)value);
                }
                else if (value is double)
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindDouble(this.stm, index, (double)value);
                }
                else if (value is string)
                {
                    int valueLength;
                    var valuePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8((string)value, out valueLength);

                    try
                    {
                        invokeResult = this.sqlite3Provider.Sqlite3BindText(this.stm, index, valuePtr, valueLength - 1, (IntPtr)(-1));
                    }
                    finally
                    {
                        if (valuePtr != IntPtr.Zero)
                        {
                            this.platformMarshal.CleanUpStringNativeUTF8(valuePtr);
                        }
                    }
                }
                else if (value is byte[])
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindBlob(this.stm, index, (byte[])value, ((byte[])value).Length, (IntPtr)(-1));
                }
            }

            if (invokeResult != (int)SQLiteResult.OK)
            {
                var errmsg = this.connection.ErrorMessage();
                throw new SQLiteException(errmsg);
            }
        }

        public void Bind(string paramName, object value)
        {
            var paramNamePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(paramName);

            try
            {
                var index = this.sqlite3Provider.Sqlite3BindParameterIndex(this.stm, paramNamePtr);
                this.Bind(index, value);
            }
            finally
            {
                if (paramNamePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(paramNamePtr);
                }
            }
        }

        public void ClearBindings()
        {
            if (this.sqlite3Provider.Sqlite3ClearBindings(this.stm) != (int)SQLiteResult.OK)
            {
                var errmsg = this.connection.ErrorMessage();
                throw new SQLiteException(errmsg);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                //// if (disposing)
                //// {
                ////     // Managed
                //// }

                // Unmanaged
                this.sqlite3Provider.Sqlite3Finalize(this.stm);

                this.stm = IntPtr.Zero;

                this.disposed = true;
            }
        }
    }
}
