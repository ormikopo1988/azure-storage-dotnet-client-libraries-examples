using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using NFluent;
using System.Threading.Tasks;

namespace AzureStorageTables
{
    [TestClass]
    public class Tests_Table_01_ConnectTo
    {
        private static TestContext _context = null;
        private static CloudTableClient _client = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            _context = context;

            string storageAccountName = _context.Properties["AzureStorageName"] as string;

            Check.That(storageAccountName).IsNotEmpty();

            string storageAccountKey = _context.Properties["AzureStorageKey"] as string;

            Check.That(storageAccountKey).IsNotEmpty();

            var storageAccountCredentials = new StorageCredentials(storageAccountName, storageAccountKey);

            var storageAccountConfig = new CloudStorageAccount(storageAccountCredentials, true);

            _client = storageAccountConfig.CreateCloudTableClient();
        }

        [TestMethod]
        public async Task Test_01_ConnectToLocalEmulator()
        {
            var devSA = CloudStorageAccount.DevelopmentStorageAccount;

            var devClient = devSA.CreateCloudTableClient();

            var myFirstTableName = "testtablecreate";

            var devTable = devClient.GetTableReference(myFirstTableName);

            //bool tableExists = await devTable.ExistsAsync();

            //Check.That(tableExists).IsFalse();

            await devTable.CreateIfNotExistsAsync();
        }

        [TestMethod]
        public async Task Test_02_ConnectToAzure()
        {
            var myFirstTableName = "testtablecreate";

            var azTable = _client.GetTableReference(myFirstTableName);

            await azTable.CreateIfNotExistsAsync();
        }
    }
}
