using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using NFluent;
using System.Net;
using System.Threading.Tasks;

namespace AzureStorageFiles
{
    [TestClass]
    public class Tests_File_01_ConnectTo
    {
        private static TestContext _context = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            _context = context;
        }

        [TestMethod]
        public async Task Test_01_Connect()
        {
            string storageAccountName = _context.Properties["AzureStorageName"] as string;

            Check.That(storageAccountName).IsNotEmpty();

            string storageAccountKey = _context.Properties["AzureStorageKey"] as string;

            Check.That(storageAccountKey).IsNotEmpty();

            var storageAccountCredentials = new StorageCredentials(storageAccountName, storageAccountKey);

            var storageAccountConfig = new CloudStorageAccount(storageAccountCredentials, true);

            var client = storageAccountConfig.CreateCloudFileClient();

            var share = client.GetShareReference("demo01");

            bool shareExists = await share.ExistsAsync();

            Check.That(shareExists).IsTrue();
        }

        [TestMethod]
        public void Test_02_ClientConnectionLimit()
        {
            Check.That(ServicePointManager.DefaultConnectionLimit).IsEqualTo(2);
        }
    }
}
