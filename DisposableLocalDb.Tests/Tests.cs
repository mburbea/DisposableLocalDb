using Disposable;
using Microsoft.Data.SqlClient;
using NUnit.Framework;


    [SetUpFixture]
    internal static class Shared
    {
        internal static DisposableLocalDb MyDb;

        [OneTimeSetUp]
        public static void InitTestEnv()
        {
            MyDb = new DisposableLocalDb("mydb", "alter database mydb collate latin1_general_bin2");
            using var conn = new SqlConnection(MyDb.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand("create table dbo.test(timestamp); insert into dbo.test default values;", conn);
            cmd.ExecuteNonQuery();
        }

        [OneTimeTearDown]
        public static void Cleanup()
        {
            MyDb.Dispose();
        }
    }

namespace Disposable.Tests
{
    internal static class SomeTest
    {
        [Test]
        public static void DidItWork()
        {
            using var conn = new SqlConnection(Shared.MyDb.ConnectionString);
            Assert.DoesNotThrow(() => conn.Open());
            using var cmd = new SqlCommand("select count(*) from dbo.test", conn);
            int value = 0;
            Assert.DoesNotThrow(() => value = (int)cmd.ExecuteScalar());
            Assert.That(value, Is.EqualTo(1));

        }
    }
}