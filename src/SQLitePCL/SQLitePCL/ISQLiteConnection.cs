namespace SQLitePCL
{
    using System;

    public interface ISQLiteConnection : IDisposable
    {
        ISQLiteStatement Prepare(string sql);
    }
}
