using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using NFluent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queue_20_Metadata
    {
        private static TestContext _context = null;
        private static CloudQueueClient _client = null;
        private static string _testQueueName = "test-q-meta";

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

            theQueue.Metadata.Add(new KeyValuePair<string, string>(
                "PollingGap", "10"
            ));

            theQueue.SetMetadataAsync().Wait();
        }

        [TestMethod]
        public async Task Test_20_Meta()
        {
            var theQueue = _client.GetQueueReference(_testQueueName);

            await theQueue.FetchAttributesAsync();

            Check.That(theQueue.Metadata).IsNotNull();
            Check.That(theQueue.Metadata.Count).IsNotZero();

            Check.That(theQueue.Metadata["PollingGap"]).IsNotEmpty();

            var pollingGap = int.Parse(theQueue.Metadata["PollingGap"]);

            bool cancel = false;

            do
            {
                var msgReceived = await theQueue.GetMessageAsync();

                if(msgReceived != null)
                {
                    // do work

                    await theQueue.DeleteMessageAsync(msgReceived); 
                }
                else
                {
                    Thread.Sleep(pollingGap);

                    cancel = true;
                }
            }
            while (!cancel);
        }
    }
}
