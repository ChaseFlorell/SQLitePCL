namespace SQLitePCL
{
    using System;

    public class SQLiteException : Exception
    {
        public SQLiteException(string message)
            : base(message)
        {
        }

        public SQLiteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
