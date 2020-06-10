using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace Disposable.Tests
{
    [SetUpFixture]
    internal static class Shared
    {
        private static DisposableLocalDb MyDb;
        private static DisposableLocalDb ThirdPartyDb;
        internal static IConfiguration Config;

        [OneTimeSetUp]
        public static void InitTestEnv()
        {
            MyDb = new DisposableLocalDb("mydb");
            ThirdPartyDb = new DisposableLocalDb("thirdparty");
            var mock = new Mock<IConfiguration>();
            mock.Setup(x => x["ConnectionStrings:mydb"]).Returns(MyDb.ConnectionString);
            mock.Setup(x => x["ConnectionStrings:thirdparty"]).Returns(ThirdPartyDb.ConnectionString);
            Config = mock.Object;
            // other one time setup code here.
        }

        [OneTimeTearDown]
        public static void Cleanup()
        {
            using (MyDb) { }
            using (ThirdPartyDb) { }
        }
    }
}