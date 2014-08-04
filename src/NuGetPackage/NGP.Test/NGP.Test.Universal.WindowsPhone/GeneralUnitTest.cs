// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

namespace NGP.Test.Universal.WindowsPhone
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using SQLitePCL;
    using Windows.Storage;

    [TestClass]
    public class GeneralUnitTest
    {
        private string databaseRelativePath = "test.db";
        private string databaseFullPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "test.db");
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
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
            }
        }

        [TestMethod]
        public void TestInvalidCharacterConnection()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                using (var connection = new SQLiteConnection(".#|-_$%\\/AN INVALID CONNECTION STRING"))
                {
                }
            });
        }

        [TestMethod]
        public void TestInvalidPathConnection()
        {
            Assert.ThrowsException<SQLiteException>(() =>
            {
                using (var connection = new SQLiteConnection("C:\\AN INVALID PATH\\test.db"))
                {
                }
            });
        }

        [TestMethod]
        public void TestPrepareValidStatement()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                using (var statement = connection.Prepare("CREATE TABLE IF NOT EXISTS t(x INTEGER, y TEXT);"))
                {
                }
            }
        }

        [TestMethod]
        public void TestPrepareInvalidStatement()
        {
            Assert.ThrowsException<SQLiteException>(() =>
            {
                using (var connection = new SQLiteConnection(this.databaseRelativePath))
                {
                    using (var statement = connection.Prepare("AN INVALID STATEMENT;"))
                    {
                    }
                }
            });
        }

        [TestMethod]
        public void TestStepStatement()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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
            using (var connection = new SQLiteConnection(this.databaseFullPath))
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

            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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
        public void TestInMemory()
        {
            var numRecords = this.rnd.Next(1, 11);

            var insertedRecords = new List<Tuple<int, long, string, double>>(numRecords);
            var queriedRecords = new List<Tuple<int, long, string, double>>(numRecords);

            for (var i = 0; i < numRecords; i++)
            {
                insertedRecords.Add(new Tuple<int, long, string, double>(i, this.GetRandomInteger(), this.GetRandomString(), this.GetRandomReal()));
            }

            using (var connection = new SQLiteConnection(":memory:"))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestInMemory;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestInMemory(id INTEGER, i INTEGER, t TEXT, r REAL);"))
                {
                    statement.Step();
                }

                foreach (var record in insertedRecords)
                {
                    var command = "INSERT INTO TestInMemory(id, i, t, r) VALUES(" + record.Item1.ToString(this.invClt) + "," + record.Item2.ToString(this.invClt)
                        + ",'" + record.Item3 + "'," + record.Item4.ToString(this.invClt) + ");";

                    using (var statement = connection.Prepare(command))
                    {
                        statement.Step();
                    }
                }

                foreach (var record in insertedRecords)
                {
                    var command = "SELECT id, i, t, r FROM TestInMemory WHERE id = " + record.Item1.ToString(this.invClt) + " AND i = " + record.Item2.ToString(this.invClt)
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

                using (var statement = connection.Prepare("DROP TABLE TestInMemory;"))
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
        public void TestTemporaryDB()
        {
            var numRecords = this.rnd.Next(1, 11);

            var insertedRecords = new List<Tuple<int, long, string, double>>(numRecords);
            var queriedRecords = new List<Tuple<int, long, string, double>>(numRecords);

            for (var i = 0; i < numRecords; i++)
            {
                insertedRecords.Add(new Tuple<int, long, string, double>(i, this.GetRandomInteger(), this.GetRandomString(), this.GetRandomReal()));
            }

            using (var connection = new SQLiteConnection(string.Empty))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestTemporaryDB;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestTemporaryDB(id INTEGER, i INTEGER, t TEXT, r REAL);"))
                {
                    statement.Step();
                }

                foreach (var record in insertedRecords)
                {
                    var command = "INSERT INTO TestTemporaryDB(id, i, t, r) VALUES(" + record.Item1.ToString(this.invClt) + "," + record.Item2.ToString(this.invClt)
                        + ",'" + record.Item3 + "'," + record.Item4.ToString(this.invClt) + ");";

                    using (var statement = connection.Prepare(command))
                    {
                        statement.Step();
                    }
                }

                foreach (var record in insertedRecords)
                {
                    var command = "SELECT id, i, t, r FROM TestTemporaryDB WHERE id = " + record.Item1.ToString(this.invClt) + " AND i = " + record.Item2.ToString(this.invClt)
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

                using (var statement = connection.Prepare("DROP TABLE TestTemporaryDB;"))
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
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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
        public void TestColumnSameName()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestColumnSameName;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestColumnSameName(id INTEGER, desc TEXT);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("SELECT id, desc AS descrip, desc, desc, SUM(id), desc \"some name\", desc \"sOmE NaMe\" FROM TestColumnSameName ORDER BY id ASC;"))
                {
                    var col0 = statement.ColumnName(0);
                    var col1 = statement.ColumnName(1);
                    var index2 = statement.ColumnIndex("desc");
                    var col3 = statement.ColumnName(3);
                    var col4 = statement.ColumnName(4);
                    var index5 = statement.ColumnIndex("some name");
                    var index6 = statement.ColumnIndex("sOmE NaMe");

                    Assert.AreEqual(col0, "id");
                    Assert.AreEqual(col1, "descrip");
                    Assert.AreEqual(index2, 2);
                    Assert.AreEqual(col3, "desc");
                    Assert.AreEqual(col4, "SUM(id)");
                    Assert.AreEqual(index5, 5);
                    Assert.AreEqual(index6, 6);
                }

                using (var statement = connection.Prepare("DROP TABLE TestColumnSameName;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestColumnDataCount()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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
        public void TestLastInsertRowId()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestLastInsertedRowId;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestLastInsertedRowId (id INTEGER PRIMARY KEY AUTOINCREMENT, desc TEXT);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestLastInsertedRowId (desc) VALUES (@desc);"))
                {
                    statement.Bind("@desc", "Desc 1");

                    statement.Step();

                    statement.Reset();
                    statement.ClearBindings();
                }

                var lastId = connection.LastInsertRowId();

                Assert.AreEqual(1, lastId);

                using (var statement = connection.Prepare("DROP TABLE TestLastInsertedRowId;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestDataType()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestDataType;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestDataType(id INTEGER, i INTEGER, t TEXT, r REAL, b BLOB, n);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestDataType(id, i, t, r, b) VALUES(@id,@i,@t,@r,@b);"))
                {
                    statement.Bind(1, 0);
                    statement.Bind("@i", this.GetRandomInteger());
                    statement.Bind(3, this.GetRandomString());
                    statement.Bind("@r", this.GetRandomReal());
                    statement.Bind(5, this.GetRandomBlob());

                    statement.Step();

                    statement.Reset();
                    statement.ClearBindings();
                }

                using (var statement = connection.Prepare("SELECT id, i, t, r, b, n FROM TestDataType ORDER BY id ASC;"))
                {
                    statement.Step();

                    var integerType = statement.DataType(1);
                    var textType = statement.DataType(2);
                    var floatType = statement.DataType(3);
                    var blobType = statement.DataType(4);
                    var nullType = statement.DataType(5);

                    Assert.AreEqual(SQLiteType.INTEGER, integerType);
                    Assert.AreEqual(SQLiteType.TEXT, textType);
                    Assert.AreEqual(SQLiteType.FLOAT, floatType);
                    Assert.AreEqual(SQLiteType.BLOB, blobType);
                    Assert.AreEqual(SQLiteType.NULL, nullType);
                }

                using (var statement = connection.Prepare("DROP TABLE TestDataType;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestFunctionLambda()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                connection.CreateFunction(
                    "CUSTOMFUNCSUMLAMBDA",
                    2,
                    (arguments) =>
                    {
                        return (long)arguments[0] + (long)arguments[1];
                    },
                    true);

                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestFunctionLambda;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestFunctionLambda(id INTEGER, a INTEGER, b INTEGER);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestFunctionLambda(id, a, b) VALUES(@id, @a, @b);"))
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

                using (var statement = connection.Prepare("SELECT id, CUSTOMFUNCSUMLAMBDA(a, b) / 2 AS CustomResult FROM TestFunctionLambda ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (long)statement[0];
                        var customResult = (long)statement[1];

                        Assert.AreEqual(id, customResult);
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestFunctionLambda;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestFunctionInstance()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                connection.CreateFunction(
                    "CUSTOMFUNCSUMINSTANCE",
                    2,
                    this.InstanceFunction,
                    true);

                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestFunctionInstance;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestFunctionInstance(id INTEGER, a INTEGER, b INTEGER);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestFunctionInstance(id, a, b) VALUES(@id, @a, @b);"))
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

                using (var statement = connection.Prepare("SELECT id, CUSTOMFUNCSUMINSTANCE(a, b) / 2 AS CustomResult FROM TestFunctionInstance ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (long)statement[0];
                        var customResult = (long)statement[1];

                        Assert.AreEqual(id, customResult);
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestFunctionInstance;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestFunctionStatic()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                connection.CreateFunction(
                    "CUSTOMFUNCSUMSTATIC",
                    2,
                    GeneralUnitTest.StaticFunction,
                    true);

                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestFunctionStatic;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestFunctionStatic(id INTEGER, a INTEGER, b INTEGER);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestFunctionStatic(id, a, b) VALUES(@id, @a, @b);"))
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

                using (var statement = connection.Prepare("SELECT id, CUSTOMFUNCSUMSTATIC(a, b) / 2 AS CustomResult FROM TestFunctionStatic ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (long)statement[0];
                        var customResult = (long)statement[1];

                        Assert.AreEqual(id, customResult);
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestFunctionStatic;"))
                {
                    statement.Step();
                }
            }
        }

        [TestMethod]
        public void TestAggregate()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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

            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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
        public void TestBindPrimitiveTypes()
        {
            var numRecords = this.rnd.Next(1, 11);

            var insertedRecords = new List<Tuple<int, byte, sbyte, short, ushort, int, uint, Tuple<long, ulong, char, string, decimal, float, double>>>(numRecords);
            var queriedRecords = new List<Tuple<int, byte, sbyte, short, ushort, int, uint, Tuple<long, ulong, char, string, decimal, float, double>>>(numRecords);

            for (var i = 0; i < numRecords; i++)
            {
                insertedRecords.Add(new Tuple<int, byte, sbyte, short, ushort, int, uint, Tuple<long, ulong, char, string, decimal, float, double>>(
                    i,
                    (byte)this.GetRandomInteger(),
                    (sbyte)this.GetRandomInteger(),
                    (short)this.GetRandomInteger(),
                    (ushort)this.GetRandomInteger(),
                    (int)this.GetRandomInteger(),
                    (uint)this.GetRandomInteger(),
                    new Tuple<long, ulong, char, string, decimal, float, double>(
                        this.GetRandomInteger(),
                        (ulong)Math.Abs(this.GetRandomInteger()),
                        this.GetRandomString()[0],
                        this.GetRandomString(),
                        (decimal)this.GetRandomReal(),
                        (float)this.GetRandomReal(),
                        this.GetRandomReal())));
            }

            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestBindPrimitiveTypes;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestBindPrimitiveTypes(id INTEGER, b INTEGER, sb INTEGER, s INTEGER, us INTEGER, i INTEGER, ui INTEGER, l INTEGER, ul INTEGER, c TEXT, st TEXT, m REAL, f REAL, d REAL);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestBindPrimitiveTypes(id, b, sb, s, us, i, ui, l, ul, c, st, m, f, d) VALUES(@id,@b,@sb,@s,@us,@i,@ui,@l,@ul,@c,@st,@m,@f,@d);"))
                {
                    foreach (var record in insertedRecords)
                    {
                        statement.Bind("@id", record.Item1);
                        statement.Bind("@b", record.Item2);
                        statement.Bind("@sb", record.Item3);
                        statement.Bind("@s", record.Item4);
                        statement.Bind("@us", record.Item5);
                        statement.Bind("@i", record.Item6);
                        statement.Bind("@ui", record.Item7);
                        statement.Bind("@l", record.Rest.Item1);
                        statement.Bind("@ul", record.Rest.Item2);
                        statement.Bind("@c", record.Rest.Item3);
                        statement.Bind("@st", record.Rest.Item4);
                        statement.Bind("@m", record.Rest.Item5);
                        statement.Bind("@f", record.Rest.Item6);
                        statement.Bind("@d", record.Rest.Item7);

                        statement.Step();

                        statement.Reset();
                        statement.ClearBindings();
                    }
                }

                using (var statement = connection.Prepare("SELECT id, b, sb, s, us, i, ui, l, ul, c, st, m, f, d FROM TestBindPrimitiveTypes ORDER BY id ASC;"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var id = (int)statement.GetInteger("id");
                        var b = (byte)statement.GetInteger("b");
                        var sb = (sbyte)statement.GetInteger("sb");
                        var s = (short)statement.GetInteger("s");
                        var us = (ushort)statement.GetInteger("us");
                        var i = (int)statement.GetInteger("i");
                        var ui = (uint)statement.GetInteger("ui");
                        var l = statement.GetInteger("l");
                        var ul = (ulong)statement.GetInteger("ul");
                        var c = statement.GetText("c")[0];
                        var st = statement.GetText("st");
                        var m = (decimal)statement.GetFloat("m");
                        var f = (float)statement.GetFloat("f");
                        var d = statement.GetFloat("d");

                        queriedRecords.Add(new Tuple<int, byte, sbyte, short, ushort, int, uint, Tuple<long, ulong, char, string, decimal, float, double>>(
                    id,
                    b,
                    sb,
                    s,
                    us,
                    i,
                    ui,
                    new Tuple<long, ulong, char, string, decimal, float, double>(
                        l,
                        ul,
                        c,
                        st,
                        m,
                        f,
                        d)));
                    }
                }

                using (var statement = connection.Prepare("DROP TABLE TestBindPrimitiveTypes;"))
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
                Assert.AreEqual(insertedRecord.Item4, queriedRecord.Item4);
                Assert.AreEqual(insertedRecord.Item5, queriedRecord.Item5);
                Assert.AreEqual(insertedRecord.Item6, queriedRecord.Item6);
                Assert.AreEqual(insertedRecord.Item7, queriedRecord.Item7);
                Assert.AreEqual(insertedRecord.Rest.Item1, queriedRecord.Rest.Item1);
                Assert.AreEqual(insertedRecord.Rest.Item2, queriedRecord.Rest.Item2);
                Assert.IsTrue(
                    insertedRecord.Rest.Item3 == queriedRecord.Rest.Item3,
                    "Expected: {0}. Actual {1}.",
                    insertedRecord.Rest.Item3,
                    queriedRecord.Rest.Item3);
                Assert.IsTrue(
                    insertedRecord.Rest.Item4 == queriedRecord.Rest.Item4,
                    "Expected: {0}. Actual {1}.",
                    insertedRecord.Rest.Item4,
                    queriedRecord.Rest.Item4);
                Assert.IsTrue(
                    Math.Abs(insertedRecord.Rest.Item5 - queriedRecord.Rest.Item5) <= Math.Max(Math.Abs(insertedRecord.Rest.Item5), Math.Abs(queriedRecord.Rest.Item5)) * 0.0000001m,
                    "Expected: {0}. Actual: {1}. Delta: {2}.",
                    insertedRecord.Rest.Item5,
                    queriedRecord.Rest.Item5,
                    Math.Max(Math.Abs(insertedRecord.Rest.Item5), Math.Abs(queriedRecord.Rest.Item5)) * 0.0000001m);
                Assert.AreEqual(insertedRecord.Rest.Item6, queriedRecord.Rest.Item6, Math.Max(Math.Abs(insertedRecord.Rest.Item6), Math.Abs(queriedRecord.Rest.Item6)) * 0.0000001f);
                Assert.AreEqual(insertedRecord.Rest.Item7, queriedRecord.Rest.Item7, Math.Max(Math.Abs(insertedRecord.Rest.Item7), Math.Abs(queriedRecord.Rest.Item7)) * 0.0000001d);
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

            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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

            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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

            using (var connection = new SQLiteConnection(this.databaseRelativePath))
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

        [TestMethod]
        public void TestEmptyBlob()
        {
            using (var connection = new SQLiteConnection(this.databaseRelativePath))
            {
                using (var statement = connection.Prepare("DROP TABLE IF EXISTS TestEmptyBlob;"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("CREATE TABLE TestEmptyBlob(b BLOB);"))
                {
                    statement.Step();
                }

                using (var statement = connection.Prepare("INSERT INTO TestEmptyBlob(b) VALUES(@b);"))
                {
                    statement.Bind("@b", this.GetRandomBlob(0));

                    statement.Step();

                    statement.Reset();
                    statement.ClearBindings();
                }

                using (var statement = connection.Prepare("SELECT b FROM TestEmptyBlob;"))
                {
                    statement.Step();

                    var emptyBlobValue = statement.GetBlob(0);

                    Assert.IsNotNull(emptyBlobValue);
                    Assert.AreEqual(0, emptyBlobValue.Length);
                }

                using (var statement = connection.Prepare("DROP TABLE TestEmptyBlob;"))
                {
                    statement.Step();
                }
            }
        }

        private static object StaticFunction(object[] arguments)
        {
            return (long)arguments[0] + (long)arguments[1];
        }

        private object InstanceFunction(object[] arguments)
        {
            var instanceVar = this.databaseFullPath ?? string.Empty;
            var zero = (long)instanceVar.Length - (long)instanceVar.Length;
            return (long)arguments[0] + (long)arguments[1] + zero;
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
