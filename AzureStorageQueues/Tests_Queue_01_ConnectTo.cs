using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using NFluent;
using System.Threading.Tasks;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queue_01_ConnectTo
    {
        private static TestContext _context = null;
        private static CloudQueueClient _client = null;

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

            _client = storageAccountConfig.CreateCloudQueueClient();
        }

        [TestMethod]
        public async Task Test_01_ConnectToLocalEmulator()
        {
            var devSA = CloudStorageAccount.DevelopmentStorageAccount;

            var devClient = devSA.CreateCloudQueueClient();

            var myFirstQueueName = "test-q-create";

            var devQueue = devClient.GetQueueReference(myFirstQueueName);

            await devQueue.CreateIfNotExistsAsync();
        }

        [TestMethod]
        public async Task Test_02_ConnectToAzure()
        {
            var myFirstQueueName = "test-q-create";

            var azQueue = _client.GetQueueReference(myFirstQueueName);

            await azQueue.CreateIfNotExistsAsync();
        }
    }
}
