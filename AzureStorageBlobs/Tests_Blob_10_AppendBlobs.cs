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
    public class Tests_Blob_10_AppendBlobs
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
        public async Task Test_10_AppendBlob()
        {
            // Containers are like folders
            CloudBlobContainer container = _client.GetContainerReference("test-append-blob-container");

            await container.CreateIfNotExistsAsync();

            // Blobs are like files
            CloudAppendBlob blob = container.GetAppendBlobReference("test-append-blob");

            await blob.CreateOrReplaceAsync();

            for(int i=0; i<100; i++)
            {
                var txt = $"Line #{(i + 1).ToString("d6")}, written at {DateTime.UtcNow.ToLongTimeString()}: {string.Empty.PadRight(20, '*')}" + Environment.NewLine;

                await blob.AppendTextAsync(txt);
            }
        }
    }
}
