namespace NGP.Test.Net45
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SQLitePCL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class GeneralUnitTest
    {
        private Random rnd;

        public GeneralUnitTest()
        {
            this.rnd = new Random(DateTime.Now.Millisecond);
        }

        [TestMethod]
        public void TestValidConnection()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidCharacterConnection()
        {
            using (var connection = new SQLiteConnection(".#|-_$%\\/AN INVALID CONNECTION STRING"))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SQLiteException))]
        public void TestInvalidPathConnection()
        {
            using (var connection = new SQLiteConnection("C:\\AN INVALID PATH\\test.db"))
            {
            }
        }

        [TestMethod]
        public void TestPrepareValidStatement()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("CREATE TABLE IF NOT EXISTS t(x INTEGER, y TEXT);"))
                {
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SQLiteException))]
        public void TestPrepareInvalidStatement()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("AN INVALID STATEMENT;"))
                {
                }
            }
        }

        [TestMethod]
        public void TestStepStatement()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("CREATE TABLE IF NOT EXISTS t(x INTEGER, y TEXT);"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestQueryStatement()
        {
            var numRecords = this.rnd.Next(1, 11);

            var insertedRecords = new List<Tuple<int, long, string, double>>(numRecords);
            var queriedRecords = new List<Tuple<int, long, string, double>>(numRecords);

            for (var i = 0; i < numRecords; i++)
            {
                insertedRecords.Add(new Tuple<int, long, string, double>(i, this.GetRandomInteger(), this.GetRandomString(), this.GetRandomReal()));
            }

            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestQuery;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestQuery(id INTEGER, i INTEGER, t TEXT, r REAL);"))
                {
                    statement.Step();
                }

                foreach (var record in insertedRecords)
                {
                    var command = "INSERT INTO TestQuery(id, i, t, r) VALUES(" + record.Item1 + "," + record.Item2 + ",'" + record.Item3 + "'," + record.Item4 + ");";

                    using (var statement = connection.Prepare(command))
                    {
                        statement.Step();
                    }
                }

                using (var statement = connection.Prepare("SELECT id, i, t, r FROM TestQuery ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (long)statement[0];
                        var i = (long)statement[1];
                        var t = (string)statement[2];
                        var r = (double)statement[3];

                        queriedRecords.Add(new Tuple<int, long, string, double>((int)id, i, t, r));
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestQuery;"))
                {
                    statement.Step();
                }
            }

            Assert.AreEqual(insertedRecords.Count, queriedRecords.Count);

            insertedRecords.Sort((x, y) => { return x.Item1 - y.Item1; });
            queriedRecords.Sort((x, y) => { return x.Item1 - y.Item1; });

            for (var i = 0; i < insertedRecords.Count; i++)
            {
                var insertedRecord = insertedRecords[i];
                var queriedRecord = queriedRecords[i];

                Assert.AreEqual(insertedRecord.Item1, queriedRecord.Item1);
                Assert.AreEqual(insertedRecord.Item2, queriedRecord.Item2);
                Assert.AreEqual(insertedRecord.Item3, queriedRecord.Item3);
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) < Math.Abs(insertedRecord.Item4 * 0.00000001));
            }
        }

        [TestMethod]
        public void TestColumnName()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestColumnName;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestColumnName(id INTEGER, desc TEXT);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("SELECT id, desc AS descrip FROM TestColumnName ORDER BY id ASC;"))
                {
                    var col0 = statement.ColumnName(0);
                    var col1 = statement.ColumnName(1);

                    Assert.AreEqual(col0, "id");
                    Assert.AreEqual(col1, "descrip");
                }

                using (var statement = connection.Prepare("DROP TABLE TestColumnName;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestBindParameter()
        {
            var numRecords = this.rnd.Next(1, 11);

            var insertedRecords = new List<Tuple<int, long, string, double, byte[]>>(numRecords);
            var queriedRecords = new List<Tuple<int, long, string, double, byte[]>>(numRecords);

            for (var i = 0; i < numRecords; i++)
            {
                insertedRecords.Add(new Tuple<int, long, string, double, byte[]>(i, this.GetRandomInteger(), this.GetRandomString(), this.GetRandomReal(), this.GetRandomBlob()));
            }

            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestBindParameter;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestBindParameter(id INTEGER, i INTEGER, t TEXT, r REAL, b BLOB);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestBindParameter(id, i, t, r, b) VALUES(@id,@i,@t,@r,@b);"))
                {
                    foreach (var record in insertedRecords)
                    {
                        statement.Bind(1, record.Item1);
                        statement.Bind("@i", record.Item2);
                        statement.Bind(3, record.Item3);
                        statement.Bind("@r", record.Item4);
                        statement.Bind(5, record.Item5);

                        statement.Step();

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                using (var statement = connection.Prepare("SELECT id, i, t, r, b FROM TestBindParameter ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (long)statement[0];
                        var i = (long)statement[1];
                        var t = (string)statement[2];
                        var r = (double)statement[3];
                        var b = (byte[])statement[4];

                        queriedRecords.Add(new Tuple<int, long, string, double, byte[]>((int)id, i, t, r, b));
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestBindParameter;"))
                {
                    statement.Step();
                }
            }

            Assert.AreEqual(insertedRecords.Count, queriedRecords.Count);

            insertedRecords.Sort((x, y) => { return x.Item1 - y.Item1; });
            queriedRecords.Sort((x, y) => { return x.Item1 - y.Item1; });

            for (var i = 0; i < insertedRecords.Count; i++)
            {
                var insertedRecord = insertedRecords[i];
                var queriedRecord = queriedRecords[i];

                Assert.AreEqual(insertedRecord.Item1, queriedRecord.Item1);
                Assert.AreEqual(insertedRecord.Item2, queriedRecord.Item2);
                Assert.AreEqual(insertedRecord.Item3, queriedRecord.Item3);
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) < Math.Abs(insertedRecord.Item4 * 0.00000001));
                Assert.IsTrue(insertedRecord.Item5.SequenceEqual(queriedRecord.Item5));
            }
        }

        private long GetRandomInteger()
        {
            return this.rnd.Next(-100, 101);
        }

        private double GetRandomReal()
        {
            return this.rnd.NextDouble() * this.rnd.Next(-100, 101);
        }

        private byte[] GetRandomBlob()
        {
            var length = this.rnd.Next(1, 101);

            return this.GetRandomBlob(length);
        }

        private byte[] GetRandomBlob(int length)
        {
            var result = new byte[length];

            this.rnd.NextBytes(result);

            return result;
        }

        private string GetRandomString()
        {
            var length = this.rnd.Next(1, 101);

            return this.GetRandomString(length);
        }

        private string GetRandomString(int length)
        {
            var builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                var codePoint = (ushort)this.rnd.Next(0x10000);

                while (codePoint == 0x0022 || codePoint == 0x0027)
                {
                    codePoint = (ushort)this.rnd.Next(0x10000);
                }

                if (codePoint < 0xD800 || codePoint > 0xDFFF)
                {
                    this.AppendCodePoint(builder, codePoint);
                }
                else if (codePoint < 0xDC00)
                {
                    this.AppendCodePoint(builder, codePoint);

                    var trailCodePoint = (ushort)this.rnd.Next(0xDC00, 0xE000);
                    this.AppendCodePoint(builder, trailCodePoint);
                }
                else
                {
                    var leadCodePoint = (ushort)this.rnd.Next(0xD800, 0xDC00);
                    this.AppendCodePoint(builder, leadCodePoint);

                    this.AppendCodePoint(builder, codePoint);
                }
            }

            return builder.ToString();
        }

        private void AppendCodePoint(StringBuilder builder, ushort codePoint)
        {
            var buffer = new byte[2];
            buffer[0] = (byte)(codePoint & 0xFF);
            buffer[1] = (byte)(codePoint >> 8);

            builder.Append(Encoding.Unicode.GetChars(buffer));
        }
    }
}