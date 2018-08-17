using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using NFluent;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blob_40_SAS
    {
        private static TestContext _context = null;
        private static CloudBlobClient _client = null;
        private static string _sasTokenRead = null;

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

            var container = _client.GetContainerReference("photos");

            var blob = container.GetBlobReference("Joins.png");

            _sasTokenRead = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = new DateTimeOffset(DateTime.Now.AddMinutes(2))
            });
        }

        [TestMethod]
        public async Task Test_40_SAS()
        {
            var blob = new CloudBlob(
                new Uri("https://omazureblobstorage.blob.core.windows.net/photos/Joins.png"),
                new StorageCredentials(_sasTokenRead)
            );

            using(var ms = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(ms);
            }
        }
    }
}
