using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using NFluent;
using System.IO;
using System.Threading.Tasks;

namespace AzureStorageFiles
{
    [TestClass]
    public class Tests_File_20_Operations
    {
        private static TestContext _context = null;
        private static CloudFileClient _client = null;

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

            _client = storageAccountConfig.CreateCloudFileClient();
        }

        [TestMethod]
        public async Task Test_20_UploadFile()
        {
            var share = _client.GetShareReference("photos");

            await share.CreateIfNotExistsAsync();

            var rootDirectory = share.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference("Joins.png");

            var localFileName = @"C:\Users\v-ormeik\Desktop\Orestis\Joins.png";

            Check.That(File.Exists(localFileName)).IsTrue();

            await file.UploadFromFileAsync(localFileName);

            var fileExists = await file.ExistsAsync();

            Check.That(fileExists).IsTrue();
        }
    }
}
