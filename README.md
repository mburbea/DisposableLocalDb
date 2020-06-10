# DisposableLocalDb
This simple library will attach a single file local database instance to your localdb and on dispose detach and delete the relevant data.

### Why?
I find myself using this pattern in tests where I'm testing code that uses internal databases and EF in memory doesn't cut it.
Also while you can use something like sqlite or sqlce, sometimes query use T-Sql constructs that aren't supported. 
Or doesn't make sense to model in EF. While you could mock everything sometimes internal libraries will use sql directly. 
This let's you populate spin up, manipulate the database as you would and throw it away when you're done.

### Example

Here is some test code I'd use from NUnit

```cs
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
```
