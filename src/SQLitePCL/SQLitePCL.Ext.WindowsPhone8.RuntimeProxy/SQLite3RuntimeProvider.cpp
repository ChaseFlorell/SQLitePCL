// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

#include "pch.h"
#include "SQLite3RuntimeProvider.h"

using namespace SQLitePCL::Ext::WindowsPhone8::RuntimeProxy;
using namespace Platform;
using namespace std;

int32 SQLite3RuntimeProvider::sqlite3_open(int64 filename, int64* db)
{
	sqlite3* sqlite3 = nullptr;

	int32 result = ::sqlite3_open((const char*)filename, &sqlite3);

	if (db)
	{
		*db = (int64)sqlite3;
	}

	return result;
}

int32 SQLite3RuntimeProvider::sqlite3_close_v2(int64 db)
{
	return ::sqlite3_close_v2((sqlite3*)db);
}

int32 SQLite3RuntimeProvider::sqlite3_prepare_v2(int64 db, int64 zSql, int32 nByte, int64* ppStmpt, int64 pzTail)
{
	sqlite3_stmt* sqlite3_stmt = nullptr;

	int32 result = ::sqlite3_prepare_v2((sqlite3*)db, (const char*)zSql, nByte, &sqlite3_stmt, (const char**)pzTail);

	if (ppStmpt)
	{
		*ppStmpt = (int64)sqlite3_stmt;
	}

	return result;
}

int32 SQLite3RuntimeProvider::sqlite3_create_function(int64 db, int64 zFunctionName, int32 nArg, int32 eTextRep, int64 pApp, int64 xFunc, int64 xStep, int64 xFinal)
{
	return ::sqlite3_create_function((sqlite3*)db, (const char*)zFunctionName, nArg, eTextRep, (void*)pApp, (void(*)(sqlite3_context*, int, sqlite3_value**))xFunc, (void(*)(sqlite3_context*, int, sqlite3_value**))xStep, (void(*)(sqlite3_context*))xFinal);
}

int64 SQLite3RuntimeProvider::sqlite3_last_insert_rowid(int64 db)
{
	return (int64)::sqlite3_last_insert_rowid((sqlite3*)db);
}

int64 SQLite3RuntimeProvider::sqlite3_errmsg(int64 db)
{
	return (int64)::sqlite3_errmsg((sqlite3*)db);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_int(int64 stmHandle, int32 iParam, int32 value)
{
	return ::sqlite3_bind_int((sqlite3_stmt*)stmHandle, iParam, value);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_int64(int64 stmHandle, int32 iParam, int64 value)
{
	return ::sqlite3_bind_int64((sqlite3_stmt*)stmHandle, iParam, (sqlite3_int64)value);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_text(int64 stmHandle, int32 iParam, int64 value, int32 length, int64 destructor)
{
	return ::sqlite3_bind_text((sqlite3_stmt*)stmHandle, iParam, (const char*)value, length, (void(*)(void*))destructor);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_double(int64 stmHandle, int32 iParam, float64 value)
{
	return ::sqlite3_bind_double((sqlite3_stmt*)stmHandle, iParam, value);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_blob(int64 stmHandle, int32 iParam, const Array<uint8>^ value, int32 length, int64 destructor)
{
	return ::sqlite3_bind_blob((sqlite3_stmt*)stmHandle, iParam, value ? value->Data : nullptr, length, (void(*)(void*))destructor);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_null(int64 stmHandle, int32 iParam)
{
	return ::sqlite3_bind_null((sqlite3_stmt*)stmHandle, iParam);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_parameter_count(int64 stmHandle)
{
	return ::sqlite3_bind_parameter_count((sqlite3_stmt*)stmHandle);
}

int64 SQLite3RuntimeProvider::sqlite3_bind_parameter_name(int64 stmHandle, int32 iParam)
{
	return (int64)::sqlite3_bind_parameter_name((sqlite3_stmt*)stmHandle, iParam);
}

int32 SQLite3RuntimeProvider::sqlite3_bind_parameter_index(int64 stmHandle, int64 paramName)
{
	return ::sqlite3_bind_parameter_index((sqlite3_stmt*)stmHandle, (const char*)paramName);
}

int32 SQLite3RuntimeProvider::sqlite3_step(int64 stmHandle)
{
	return ::sqlite3_step((sqlite3_stmt*)stmHandle);
}

int32 SQLite3RuntimeProvider::sqlite3_column_int(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_int((sqlite3_stmt*)stmHandle, iCol);
}

int64 SQLite3RuntimeProvider::sqlite3_column_int64(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_int64((sqlite3_stmt*)stmHandle, iCol);
}

int64 SQLite3RuntimeProvider::sqlite3_column_text(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_text((sqlite3_stmt*)stmHandle, iCol);
}

float64 SQLite3RuntimeProvider::sqlite3_column_double(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_double((sqlite3_stmt*)stmHandle, iCol);
}

int64 SQLite3RuntimeProvider::sqlite3_column_blob(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_blob((sqlite3_stmt*)stmHandle, iCol);
}

int32 SQLite3RuntimeProvider::sqlite3_column_type(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_type((sqlite3_stmt*)stmHandle, iCol);
}

int32 SQLite3RuntimeProvider::sqlite3_column_bytes(int64 stmHandle, int32 iCol)
{
	return ::sqlite3_column_bytes((sqlite3_stmt*)stmHandle, iCol);
}

int32 SQLite3RuntimeProvider::sqlite3_column_count(int64 stmHandle)
{
	return ::sqlite3_column_count((sqlite3_stmt*)stmHandle);
}

int64 SQLite3RuntimeProvider::sqlite3_column_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 SQLite3RuntimeProvider::sqlite3_column_origin_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_origin_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 SQLite3RuntimeProvider::sqlite3_column_table_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_table_name((sqlite3_stmt*)stmHandle, iCol);
}

int64 SQLite3RuntimeProvider::sqlite3_column_database_name(int64 stmHandle, int32 iCol)
{
	return (int64)::sqlite3_column_database_name((sqlite3_stmt*)stmHandle, iCol);
}

int32 SQLite3RuntimeProvider::sqlite3_data_count(int64 stmHandle)
{
	return ::sqlite3_data_count((sqlite3_stmt*)stmHandle);
}

int32 SQLite3RuntimeProvider::sqlite3_reset(int64 stmHandle)
{
	return ::sqlite3_reset((sqlite3_stmt*)stmHandle);
}

int32 SQLite3RuntimeProvider::sqlite3_clear_bindings(int64 stmHandle)
{
	return ::sqlite3_clear_bindings((sqlite3_stmt*)stmHandle);
}

int32 SQLite3RuntimeProvider::sqlite3_finalize(int64 stmHandle)
{
	return ::sqlite3_finalize((sqlite3_stmt*)stmHandle);
}

int32 SQLite3RuntimeProvider::sqlite3_value_int(int64 value)
{
	return ::sqlite3_value_int((sqlite3_value*)value);
}

int64 SQLite3RuntimeProvider::sqlite3_value_int64(int64 value)
{
	return (int64)::sqlite3_value_int64((sqlite3_value*)value);
}

int64 SQLite3RuntimeProvider::sqlite3_value_text(int64 value)
{
	return (int64)::sqlite3_value_text((sqlite3_value*)value);
}

float64 SQLite3RuntimeProvider::sqlite3_value_double(int64 value)
{
	return ::sqlite3_value_double((sqlite3_value*)value);
}

int64 SQLite3RuntimeProvider::sqlite3_value_blob(int64 value)
{
	return (int64)::sqlite3_value_blob((sqlite3_value*)value);
}

int32 SQLite3RuntimeProvider::sqlite3_value_type(int64 value)
{
	return ::sqlite3_value_type((sqlite3_value*)value);
}

int32 SQLite3RuntimeProvider::sqlite3_value_bytes(int64 value)
{
	return ::sqlite3_value_bytes((sqlite3_value*)value);
}

void SQLite3RuntimeProvider::sqlite3_result_int(int64 context, int32 result)
{
	::sqlite3_result_int((sqlite3_context*)context, result);
}

void SQLite3RuntimeProvider::sqlite3_result_int64(int64 context, int64 result)
{
	::sqlite3_result_int64((sqlite3_context*)context, (sqlite3_int64)result);
}

void SQLite3RuntimeProvider::sqlite3_result_text(int64 context, int64 result, int32 length, int64 destructor)
{
	::sqlite3_result_text((sqlite3_context*)context, (const char*)result, length, (void(*)(void*))destructor);
}

void SQLite3RuntimeProvider::sqlite3_result_double(int64 context, float64 result)
{
	::sqlite3_result_double((sqlite3_context*)context, result);
}

void SQLite3RuntimeProvider::sqlite3_result_blob(int64 context, const Platform::Array<uint8>^ result, int32 length, int64 destructor)
{
	::sqlite3_result_blob((sqlite3_context*)context, result ? result->Data : nullptr, length, (void(*)(void*))destructor);
}

void SQLite3RuntimeProvider::sqlite3_result_null(int64 context)
{
	::sqlite3_result_null((sqlite3_context*)context);
}

void SQLite3RuntimeProvider::sqlite3_result_error(int64 context, int64 result, int32 length)
{
	::sqlite3_result_error((sqlite3_context*)context, (const char*)result, length);
}

int64 SQLite3RuntimeProvider::sqlite3_aggregate_context(int64 context, int32 length)
{
	return (int64)::sqlite3_aggregate_context((sqlite3_context*)context, length);
}