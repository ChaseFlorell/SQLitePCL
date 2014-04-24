// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

namespace NGP.Test.Net45
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SQLitePCL;

    [TestClass]
    public class GeneralUnitTest
    {
        private Random rnd;
        private CultureInfo invClt;

        public GeneralUnitTest()
        {
            this.rnd = new Random(DateTime.Now.Millisecond);
            this.invClt = CultureInfo.InvariantCulture;
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
        public void TestFullPath()
        {
            var location = Environment.CurrentDirectory;
            var dbpath = Path.Combine(location, "test.db");

            using (var connection = new SQLiteConnection(dbpath))
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
                    var command = "INSERT INTO TestQuery(id, i, t, r) VALUES(" + record.Item1.ToString(this.invClt) + "," + record.Item2.ToString(this.invClt)
                        + ",'" + record.Item3 + "'," + record.Item4.ToString(this.invClt) + ");";

                    using (var statement = connection.Prepare(command))
                    {
                        statement.Step();
                    }
                }

                foreach (var record in insertedRecords)
                {
                    var command = "SELECT id, i, t, r FROM TestQuery WHERE id = " + record.Item1.ToString(this.invClt) + " AND i = " + record.Item2.ToString(this.invClt)
                        + " AND t = '" + record.Item3 + "' AND r = " + record.Item4.ToString(this.invClt) + ";";

                    using (var statement = connection.Prepare(command))
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
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) <= Math.Abs(insertedRecord.Item4 * 0.0000001));
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
        public void TestColumnDataCount()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestColumnDataCount;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestColumnDataCount(id INTEGER, desc TEXT);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestColumnDataCount(id, desc) VALUES(@id,@desc);"))
                {
                    statement.Bind(1, 1);
                    statement.Bind("@desc", "Desc 1");

                    statement.Step();

                    statement.Reset();
                    statement.ClearBindings();
                }

                using (var statement = connection.Prepare("SELECT id, desc AS desc FROM TestColumnDataCount ORDER BY id ASC;"))
                {
                    var columnCount = statement.ColumnCount;
                    var dataCount = statement.DataCount;

                    Assert.AreEqual(2, columnCount);
                    Assert.AreEqual(0, dataCount);

                    statement.Step();

                    columnCount = statement.ColumnCount;
                    dataCount = statement.DataCount;

                    Assert.AreEqual(2, columnCount);
                    Assert.AreEqual(2, dataCount);

                    statement.Step();

                    columnCount = statement.ColumnCount;
                    dataCount = statement.DataCount;

                    Assert.AreEqual(2, columnCount);
                    Assert.AreEqual(0, dataCount);
                }

                using (var statement = connection.Prepare("DROP TABLE TestColumnDataCount;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestFunction()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                connection.CreateFunction(
                    "CUSTOMFUNCSUM",
                    2,
                    new Function((arguments) =>
                    {
                        return (long)arguments[0] + (long)arguments[1];
                    }),
                    true);

                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestFunction;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestFunction(id INTEGER, a INTEGER, b INTEGER);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestFunction(id, a, b) VALUES(@id, @a, @b);"))
                {
                    for (var value = 0; value < 10; value++)
                    {
                        statement.Bind(1, value);
                        statement.Bind("@a", value - 1);
                        statement.Bind("@b", value + 1);

                        statement.Step();

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                using (var statement = connection.Prepare("SELECT id, CUSTOMFUNCSUM(a, b) / 2 AS CustomResult FROM TestFunction ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (long)statement[0];
                        var customResult = (long)statement[1];

                        Assert.AreEqual(id, customResult);
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestFunction;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestAggregate()
        {
            using (var connection = new SQLiteConnection("test.db"))
            {
                connection.CreateAggregate(
                    "CUSTOMAGGSUM",
                    1,
                    new AggregateStep((aggregateContextData, arguments) =>
                    {
                        aggregateContextData["Acum"] = aggregateContextData.ContainsKey("Acum") ? (long)arguments[0] + (long)aggregateContextData["Acum"] : (long)arguments[0];
                    }),
                    new AggregateFinal((aggregateContextData) =>
                    {
                        return aggregateContextData.ContainsKey("Acum") ? (long)aggregateContextData["Acum"] : 0L;
                    }));

                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestAggregate;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestAggregate(id INTEGER);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestAggregate(id) VALUES(@id);"))
                {
                    for (var value = 0; value < 10; value++)
                    {
                        statement.Bind(1, value);

                        statement.Step();

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                using (var statement = connection.Prepare("SELECT CUSTOMAGGSUM(id) AS CustomResult FROM TestAggregate;"))
                {
                    var rowTotal = 0;
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        rowTotal++;

                        var totalSum = (long)statement[0];

                        Assert.AreEqual(45, totalSum);
                    }

                    Assert.AreEqual(1, rowTotal);
                }

                using (var statement = connection.Prepare("DROP TABLE TestAggregate;"))
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
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) <= Math.Abs(insertedRecord.Item4 * 0.0000001));
                Assert.IsTrue(insertedRecord.Item5.SequenceEqual(queriedRecord.Item5));
            }
        }

        [TestMethod]
        public void TestBindParameterFilter()
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
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestBindParameterFilter;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestBindParameterFilter(id INTEGER, i INTEGER, t TEXT, r REAL, b BLOB);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestBindParameterFilter(id, i, t, r, b) VALUES(@id,@i,@t,@r,@b);"))
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

                using (var statement = connection.Prepare("SELECT id, i, t, r, b FROM TestBindParameterFilter WHERE id = @id AND i = @i AND t = @t AND r = @r AND b = @b;"))
                {
                    foreach (var record in insertedRecords)
                    {
                        statement.Bind(1, record.Item1);
                        statement.Bind("@i", record.Item2);
                        statement.Bind(3, record.Item3);
                        statement.Bind("@r", record.Item4);
                        statement.Bind(5, record.Item5);

                        while (statement.Step() == SQLiteResult.ROW)
                        {
                            var id = (long)statement[0];
                            var i = (long)statement[1];
                            var t = (string)statement[2];
                            var r = (double)statement[3];
                            var b = (byte[])statement[4];

                            queriedRecords.Add(new Tuple<int, long, string, double, byte[]>((int)id, i, t, r, b));
                        }

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestBindParameterFilter;"))
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
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) <= Math.Abs(insertedRecord.Item4 * 0.0000001));
                Assert.IsTrue(insertedRecord.Item5.SequenceEqual(queriedRecord.Item5));
            }
        }

        [TestMethod]
        public void TestParameterBoundInsertConstantQueryFilter()
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
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestParameterBoundInsertConstantQueryFilter;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestParameterBoundInsertConstantQueryFilter(id INTEGER, i INTEGER, t TEXT, r REAL);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestParameterBoundInsertConstantQueryFilter(id, i, t, r) VALUES(@id,@i,@t,@r);"))
                {
                    foreach (var record in insertedRecords)
                    {
                        statement.Bind(1, record.Item1);
                        statement.Bind("@i", record.Item2);
                        statement.Bind(3, record.Item3);
                        statement.Bind("@r", record.Item4);

                        statement.Step();

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                foreach (var record in insertedRecords)
                {
                    var command = "SELECT id, i, t, r FROM TestParameterBoundInsertConstantQueryFilter WHERE id = " + record.Item1.ToString(this.invClt) + " AND i = " + record.Item2.ToString(this.invClt)
                        + " AND t = '" + record.Item3 + "';";

                    using (var statement = connection.Prepare(command))
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
                }

                using (var statement = connection.Prepare("DROP TABLE TestParameterBoundInsertConstantQueryFilter;"))
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
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) <= Math.Abs(insertedRecord.Item4 * 0.0000001));
            }
        }

        [TestMethod]
        public void TestConstantInsertParameterBoundQueryFilter()
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
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestConstantInsertParameterBoundQueryFilter;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestConstantInsertParameterBoundQueryFilter(id INTEGER, i INTEGER, t TEXT, r REAL);"))
                {
                    statement.Step();
                }

                foreach (var record in insertedRecords)
                {
                    var command = "INSERT INTO TestConstantInsertParameterBoundQueryFilter(id, i, t, r) VALUES(" + record.Item1.ToString(this.invClt) + "," + record.Item2.ToString(this.invClt)
                        + ",'" + record.Item3 + "'," + record.Item4.ToString(this.invClt) + ");";

                    using (var statement = connection.Prepare(command))
                    {
                        statement.Step();
                    }
                }

                using (var statement = connection.Prepare("SELECT id, i, t, r FROM TestConstantInsertParameterBoundQueryFilter WHERE id = @id AND i = @i AND t = @t;"))
                {
                    foreach (var record in insertedRecords)
                    {
                        statement.Bind(1, record.Item1);
                        statement.Bind("@i", record.Item2);
                        statement.Bind(3, record.Item3);

                        while (statement.Step() == SQLiteResult.ROW)
                        {
                            var id = (long)statement[0];
                            var i = (long)statement[1];
                            var t = (string)statement[2];
                            var r = (double)statement[3];

                            queriedRecords.Add(new Tuple<int, long, string, double>((int)id, i, t, r));
                        }

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestConstantInsertParameterBoundQueryFilter;"))
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
                Assert.IsTrue(Math.Abs(insertedRecord.Item4 - queriedRecord.Item4) <= Math.Abs(insertedRecord.Item4 * 0.0000001));
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

                while (this.InvalidCharacter(codePoint))
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

        private bool InvalidCharacter(ushort codePoint)
        {
            return codePoint <= 0x001F || (codePoint >= 0x007F && codePoint <= 0x00A0) || codePoint == 0x0022 || codePoint == 0x0027;
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