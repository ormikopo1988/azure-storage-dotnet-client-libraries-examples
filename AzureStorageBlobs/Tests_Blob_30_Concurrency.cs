using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using NFluent;
using System;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blob_30_Concurrency
    {
        private static TestContext _context = null;
        private static CloudBlobClient _client = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            _context = context;

            string storageAccountName = _context.Properties["AzureOnlyBlobStorageName"] as string;

            Check.That(storageAccountName).IsNotEmpty();

            string storageAccountKey = _context.Properties["AzureOnlyBlobStorageKey"] as string;

            Check.That(storageAccountKey).IsNotEmpty();

            var storageAccountCredentials = new StorageCredentials(storageAccountName, storageAccountKey);

            var storageAccountConfig = new CloudStorageAccount(storageAccountCredentials, true);

            _client = storageAccountConfig.CreateCloudBlobClient();
        }

        [TestMethod]
        public async Task Test_30_Blob_InfiniteLease()
        {
            // Containers are like folders
            CloudBlobContainer container = _client.GetContainerReference("test-append-blob-container");

            await container.CreateIfNotExistsAsync();

            // Blobs are like files
            CloudAppendBlob blob = container.GetAppendBlobReference("test-append-blob-with-lease");

            var blobExists = await blob.ExistsAsync();

            if(!blobExists)
            {
                await blob.CreateOrReplaceAsync();
            }

            string data = string.Empty.PadLeft(64 * 1024, '*');

            string leaseId = Guid.NewGuid().ToString();

            var operationContext = new OperationContext();

            var accessCondition = new AccessCondition();

            // For pessimistic access control
            // Acquire lease
            var leaseID = await blob.AcquireLeaseAsync(null, leaseId, accessCondition, null, operationContext);

            accessCondition.LeaseId = leaseID;

            try
            {
                // Make modifications based on that lease
                for (int i = 0; i < 1000; i++)
                {
                    await blob.AppendTextAsync(data, null, accessCondition, null, operationContext);
                }
            }
            finally
            {
                // And finally release that lease
                await blob.ReleaseLeaseAsync(accessCondition, null, operationContext);
            }
        }
    }
}
