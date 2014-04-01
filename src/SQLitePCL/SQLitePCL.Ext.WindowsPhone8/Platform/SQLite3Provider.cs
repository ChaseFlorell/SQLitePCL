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
    using SQLitePCL.Ext.WindowsPhone8.RuntimeProxy;
    using System;

    /// <summary>
    /// Implements the <see cref="ISQLite3Provider"/> interface for Windows Store.
    /// </summary>
    internal class SQLite3Provider : ISQLite3Provider
    {
        /// <summary>
        /// A singleton instance of the <see cref="SQLite3Provider"/>.
        /// </summary>
        private static ISQLite3Provider instance = new SQLite3Provider();

        private SQLite3Provider()
        {
        }

        /// <summary>
        /// A singleton instance of the <see cref="SQLite3Provider"/>.
        /// </summary>
        public static ISQLite3Provider Instance
        {
            get
            {
                return instance;
            }
        }

        public int Sqlite3Open(IntPtr filename, out IntPtr db)
        {
            long databasePtr;

            var result = SQLite3RuntimeProvider.sqlite3_open(filename.ToInt64(), out databasePtr);

            db = new IntPtr(databasePtr);

            return result;
        }

        int ISQLite3Provider.Sqlite3CloseV2(IntPtr db)
        {
            return SQLite3RuntimeProvider.sqlite3_close_v2(db.ToInt64());
        }

        int ISQLite3Provider.Sqlite3PrepareV2(IntPtr db, IntPtr sql, int length, out IntPtr stm, IntPtr tail)
        {
            long stmPtr;

            var result = SQLite3RuntimeProvider.sqlite3_prepare_v2(db.ToInt64(), sql.ToInt64(), length, out stmPtr, tail.ToInt64());

            stm = new IntPtr(stmPtr);

            return result;
        }

        IntPtr ISQLite3Provider.Sqlite3Errmsg(IntPtr db)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_errmsg(db.ToInt64()));
        }

        int ISQLite3Provider.Sqlite3BindInt(IntPtr stm, int paramIndex, int value)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_int(stm.ToInt64(), paramIndex, value);
        }

        int ISQLite3Provider.Sqlite3BindInt64(IntPtr stm, int paramIndex, long value)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_int64(stm.ToInt64(), paramIndex, value);
        }

        int ISQLite3Provider.Sqlite3BindText(IntPtr stm, int paramIndex, IntPtr value, int length, IntPtr destructor)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_text(stm.ToInt64(), paramIndex, value.ToInt64(), length, destructor.ToInt64());
        }

        int ISQLite3Provider.Sqlite3BindDouble(IntPtr stm, int paramIndex, double value)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_double(stm.ToInt64(), paramIndex, value);
        }

        int ISQLite3Provider.Sqlite3BindBlob(IntPtr stm, int paramIndex, byte[] value, int length, IntPtr destructor)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_blob(stm.ToInt64(), paramIndex, value, length, destructor.ToInt64());
        }

        int ISQLite3Provider.Sqlite3BindNull(IntPtr stm, int paramIndex)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_null(stm.ToInt64(), paramIndex);
        }

        int ISQLite3Provider.Sqlite3BindParameterCount(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_parameter_count(stm.ToInt64());
        }

        IntPtr ISQLite3Provider.Sqlite3BindParameterName(IntPtr stm, int paramIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_bind_parameter_name(stm.ToInt64(), paramIndex));
        }

        int ISQLite3Provider.Sqlite3BindParameterIndex(IntPtr stm, IntPtr paramName)
        {
            return SQLite3RuntimeProvider.sqlite3_bind_parameter_index(stm.ToInt64(), paramName.ToInt64());
        }

        int ISQLite3Provider.Sqlite3Step(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_step(stm.ToInt64());
        }

        int ISQLite3Provider.Sqlite3ColumnInt(IntPtr stm, int columnIndex)
        {
            return SQLite3RuntimeProvider.sqlite3_column_int(stm.ToInt64(), columnIndex);
        }

        long ISQLite3Provider.Sqlite3ColumnInt64(IntPtr stm, int columnIndex)
        {
            return SQLite3RuntimeProvider.sqlite3_column_int64(stm.ToInt64(), columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnText(IntPtr stm, int columnIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_column_text(stm.ToInt64(), columnIndex));
        }

        double ISQLite3Provider.Sqlite3ColumnDouble(IntPtr stm, int columnIndex)
        {
            return SQLite3RuntimeProvider.sqlite3_column_double(stm.ToInt64(), columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnBlob(IntPtr stm, int columnIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_column_blob(stm.ToInt64(), columnIndex));
        }

        int ISQLite3Provider.Sqlite3ColumnType(IntPtr stm, int columnIndex)
        {
            return SQLite3RuntimeProvider.sqlite3_column_type(stm.ToInt64(), columnIndex);
        }

        int ISQLite3Provider.Sqlite3ColumnBytes(IntPtr stm, int columnIndex)
        {
            return SQLite3RuntimeProvider.sqlite3_column_bytes(stm.ToInt64(), columnIndex);
        }

        int ISQLite3Provider.Sqlite3ColumnCount(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_column_count(stm.ToInt64());
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnName(IntPtr stm, int columnIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_column_name(stm.ToInt64(), columnIndex));
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnOriginName(IntPtr stm, int columnIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_column_origin_name(stm.ToInt64(), columnIndex));
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnTableName(IntPtr stm, int columnIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_column_table_name(stm.ToInt64(), columnIndex));
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnDatabaseName(IntPtr stm, int columnIndex)
        {
            return new IntPtr(SQLite3RuntimeProvider.sqlite3_column_database_name(stm.ToInt64(), columnIndex));
        }

        int ISQLite3Provider.Sqlite3DataCount(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_data_count(stm.ToInt64());
        }

        int ISQLite3Provider.Sqlite3Reset(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_reset(stm.ToInt64());
        }

        int ISQLite3Provider.Sqlite3ClearBindings(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_clear_bindings(stm.ToInt64());
        }

        int ISQLite3Provider.Sqlite3Finalize(IntPtr stm)
        {
            return SQLite3RuntimeProvider.sqlite3_finalize(stm.ToInt64());
        }
    }
}
