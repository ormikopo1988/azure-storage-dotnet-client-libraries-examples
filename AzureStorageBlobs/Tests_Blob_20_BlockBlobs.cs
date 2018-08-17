using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using NFluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageBlobs
{
    [TestClass]
    public class Tests_Blob_20_BlockBlobs
    {
        private static TestContext _context = null;
        private static CloudBlobClient _client = null;
        private string _eTag = null;

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
        public async Task Test_20_UploadBlob()
        {
            // Containers are like folders
            CloudBlobContainer container = _client.GetContainerReference("photos");

            await container.CreateIfNotExistsAsync();

            // Blobs are like files
            CloudBlockBlob blob = container.GetBlockBlobReference("Joins.png");

            await blob.DeleteIfExistsAsync();

            var localFileName = @"C:\Users\v-ormeik\Desktop\Orestis\Joins.png";

            Check.That(File.Exists(localFileName)).IsTrue();

            await blob.UploadFromFileAsync(localFileName);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageException))]
        public async Task Test_21_UploadBlobIfNotExists()
        {
            // Containers are like folders
            CloudBlobContainer container = _client.GetContainerReference("photos");

            await container.CreateIfNotExistsAsync();

            // Blobs are like files
            CloudBlockBlob blob = container.GetBlockBlobReference("Joins.png");

            // A special condition that means that when we try to upload this specific file, nothing similar must exist in the container at all
            var accessCondition = AccessCondition.GenerateIfNoneMatchCondition("*");

            var localFileName = @"C:\Users\v-ormeik\Desktop\Orestis\Joins.png";

            Check.That(File.Exists(localFileName)).IsTrue();

            await blob.UploadFromFileAsync(localFileName, accessCondition, null, null);
        }

        [TestMethod]
        public async Task Test_22_BlockBlobUpload()
        {
            await BlockBlobUpload();
        }

        [TestMethod]
        public async Task Test_23_ModifyBlockBlob()
        {
            await ModifyBlockBlobWithoutChecks();
        }

        [TestMethod]
        public async Task Test_24_ModifyBlockBlobIfNotModified()
        {
            await BlockBlobUpload();

            // uncomment below line in order for the test to fail - eTag has changed
            //await ModifyBlockBlobWithoutChecks();

            var container = _client.GetContainerReference("test-blockblob-blockuploads");

            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("myblockblob");

            bool blobExists = await blob.ExistsAsync();

            Check.That(blobExists).IsTrue();

            // Ask the blob for the list of blocks
            var blockItems = await blob.DownloadBlockListAsync();

            var blockIds = blockItems.Select(b => b.Name).ToList();

            var myNewBlock = string.Empty.PadRight(50, '+') + Environment.NewLine;

            var nBlockId = 3;

            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(nBlockId.ToString("d6")));

            Check.That(blockIds).Contains(blockId);

            var accessCondition = AccessCondition.GenerateIfMatchCondition(_eTag);

            using (var blockData = new MemoryStream(Encoding.UTF8.GetBytes(myNewBlock)))
            {
                await blob.PutBlockAsync(blockId, blockData, null, accessCondition, null, null);
            }

            await blob.PutBlockListAsync(blockIds, accessCondition, null, null);
        }

        private async Task BlockBlobUpload()
        {
            var container = _client.GetContainerReference("test-blockblob-blockuploads");

            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("myblockblob");

            await blob.DeleteIfExistsAsync();

            var myDataBlockList = new List<string>
            {
                string.Empty.PadRight(50, '*') + Environment.NewLine,
                string.Empty.PadRight(50, '-') + Environment.NewLine,
                string.Empty.PadRight(50, '=') + Environment.NewLine,
                string.Empty.PadRight(50, '-') + Environment.NewLine,
                string.Empty.PadRight(50, '*') + Environment.NewLine
            };

            var blockIds = new List<string>();

            int id = 0;

            foreach (var myDataBlock in myDataBlockList)
            {
                id++;

                // Generate an id that is acceptable to the Azure Storage account for each block.
                // It has to be Base64 encoded.
                var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(id.ToString("d6")));

                using (var blockData = new MemoryStream(Encoding.UTF8.GetBytes(myDataBlock)))
                {
                    await blob.PutBlockAsync(blockId, blockData, null);
                }

                blockIds.Add(blockId);
            }

            // Once all those uploads have completed, we ask the blob that once we have uploaded everything,
            // please put the list of blocks that we recently uploaded in order to commit the changes
            await blob.PutBlockListAsync(blockIds);

            await blob.FetchAttributesAsync();

            _eTag = blob.Properties.ETag;

            Check.That(_eTag).IsNotNull().And.IsNotEmpty();
        }

        private static async Task ModifyBlockBlobWithoutChecks()
        {
            var container = _client.GetContainerReference("test-blockblob-blockuploads");

            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference("myblockblob");

            bool blobExists = await blob.ExistsAsync();

            Check.That(blobExists).IsTrue();

            // Ask the blob for the list of blocks
            var blockItems = await blob.DownloadBlockListAsync();

            var blockIds = blockItems.Select(b => b.Name).ToList();

            var myNewBlock = string.Empty.PadRight(50, '+') + Environment.NewLine;

            var nBlockId = 3;

            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(nBlockId.ToString("d6")));

            Check.That(blockIds).Contains(blockId);

            using (var blockData = new MemoryStream(Encoding.UTF8.GetBytes(myNewBlock)))
            {
                await blob.PutBlockAsync(blockId, blockData, null);
            }

            await blob.PutBlockListAsync(blockIds);
        }
    }
}
