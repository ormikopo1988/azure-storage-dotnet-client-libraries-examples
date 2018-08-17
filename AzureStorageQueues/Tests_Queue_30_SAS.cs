using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using NFluent;
using System;
using System.Threading.Tasks;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queue_30_SAS
    {
        private static TestContext _context = null;
        private static CloudQueueClient _client = null;
        private static string _sasTokenProcessingMessages = null;
        private static string _sasTokenAddMessages = null;
        private static string _testQueueName = "test-q-sas";

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

            var theQueue = _client.GetQueueReference(_testQueueName);

            theQueue.CreateIfNotExistsAsync().Wait();
            theQueue.ClearAsync().Wait();

            _sasTokenProcessingMessages = theQueue.GetSharedAccessSignature(new SharedAccessQueuePolicy
            {
                Permissions = SharedAccessQueuePermissions.ProcessMessages,
                SharedAccessExpiryTime = new DateTimeOffset(DateTime.Now.AddSeconds(90))
            });

            _sasTokenAddMessages = theQueue.GetSharedAccessSignature(new SharedAccessQueuePolicy
            {
                Permissions = SharedAccessQueuePermissions.Add,
                SharedAccessExpiryTime = new DateTimeOffset(DateTime.Now.AddSeconds(90))
            });
        }

        [TestMethod]
        public async Task Test_30_AddMessage()
        {
            var qClient = new CloudQueueClient(
                new Uri("https://omazurestoragecli.queue.core.windows.net/"),
                new StorageCredentials(_sasTokenAddMessages)
            );

            var myQueue = qClient.GetQueueReference(_testQueueName);

            //bool queueExists = await myQueue.ExistsAsync(); // no permission for this

            await myQueue.AddMessageAsync(new CloudQueueMessage("data")); // do have permission for this based on the sas token
        }

        [TestMethod]
        public async Task Test_31_ProcessMessage()
        {
            var qClient = new CloudQueueClient(
                new Uri("https://omazurestoragecli.queue.core.windows.net/"),
                new StorageCredentials(_sasTokenProcessingMessages)
            );

            var myQueue = qClient.GetQueueReference(_testQueueName);

            //bool queueExists = await myQueue.ExistsAsync(); // no permission for this

            var msg = await myQueue.GetMessageAsync();

            if(msg != null)
            {
                // do work 

                await myQueue.DeleteMessageAsync(msg);
            }
        }
    }
}
