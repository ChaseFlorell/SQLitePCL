namespace SQLitePCL
{
    using System;

    public interface ISQLiteStatement : IDisposable
    {
        ISQLiteConnection Connection { get; }

        object this[int index] { get; }

        SQLiteResult Step();

        string ColumnName(int index);

        void Reset();

        void Bind(int index, object value);

        void Bind(string paramName, object value);

        void ClearBindings();
    }
}
