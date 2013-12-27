namespace SQLitePCL
{
    using System;

    public class SQLiteConnection : ISQLiteConnection
    {
        private IPlatformMarshal platformMarshal;

        private IPlatformStorage platformStorage;

        private ISQLite3Provider sqlite3Provider;

        private IntPtr db;

        private bool disposed = false;

        public SQLiteConnection(string fileName)
        {
            this.platformMarshal = Platform.Instance.PlatformMarshal;
            this.platformStorage = Platform.Instance.PlatformStorage;
            this.sqlite3Provider = Platform.Instance.SQLite3Provider;

            var localFilePath = this.platformStorage.GetLocalFilePath(fileName);

            var fileNamePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(localFilePath);

            try
            {
                if (this.sqlite3Provider.Sqlite3Open(fileNamePtr, out this.db) != (int)SQLiteResult.OK)
                {
                    if (this.db != null && this.db != IntPtr.Zero)
                    {
                        var errmsgPtr = this.sqlite3Provider.Sqlite3Errmsg(this.db);

                        var errmsg = this.platformMarshal.MarshalStringNativeUTF8ToManaged(errmsgPtr);

                        this.sqlite3Provider.Sqlite3CloseV2(this.db);

                        throw new SQLiteException("Unable to open the database file: " + fileName + " Details: " + errmsg);
                    }

                    throw new SQLiteException("Unable to open the database file: " + fileName);
                }
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to open the database file: " + fileName, ex);
            }
            finally
            {
                if (fileNamePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(fileNamePtr);
                }
            }
        }

        ~SQLiteConnection()
        {
            this.Dispose(false);
        }

        public ISQLiteStatement Prepare(string sql)
        {
            IntPtr stm;

            int sqlLength;

            var sqlPtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(sql, out sqlLength);

            try
            {
                if (this.sqlite3Provider.Sqlite3PrepareV2(this.db, sqlPtr, sqlLength, out stm, IntPtr.Zero) != (int)SQLiteResult.OK)
                {
                    var errmsgPtr = this.sqlite3Provider.Sqlite3Errmsg(this.db);

                    var errmsg = this.platformMarshal.MarshalStringNativeUTF8ToManaged(errmsgPtr);

                    throw new SQLiteException("Unable to prepare the sql statement: " + sql + " Details: " + errmsg);
                }

                return new SQLiteStatement(this, stm);
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to prepare the sql statement: " + sql, ex);
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(sqlPtr);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        internal string ErrorMessage()
        {
            var errmsgPtr = this.sqlite3Provider.Sqlite3Errmsg(this.db);

            return this.platformMarshal.MarshalStringNativeUTF8ToManaged(errmsgPtr);
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
                this.sqlite3Provider.Sqlite3CloseV2(this.db);

                this.db = IntPtr.Zero;

                this.disposed = true;
            }
        }
    }
}
