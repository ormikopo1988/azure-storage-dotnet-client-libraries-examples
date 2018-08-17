using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using NFluent;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blob_01_ConnectTo
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
        public async Task Test_01_ConnectTo_DevelopmentStorage()
        {
            // Connect to the local emulator storage account
            var dsaConfig = CloudStorageAccount.DevelopmentStorageAccount;

            var client = dsaConfig.CreateCloudBlobClient();

            var myContainerName = "test-container-one";

            var container = client.GetContainerReference(myContainerName);

            await container.CreateIfNotExistsAsync();
        }

        [TestMethod]
        public async Task Test_02_ConnectTo_AzureBlobStorage()
        {
            var myContainerName = "test-container-one";

            var container = _client.GetContainerReference(myContainerName);

            await container.CreateIfNotExistsAsync();
        }
    }
}
