using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using NFluent;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureStorageFiles
{
    [TestClass]
    public class Tests_File_30_SAS
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
        public async Task Test_30_SAS()
        {
            var share = _client.GetShareReference("photos");

            var rootDirectory = share.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference("Joins.png");

            // Get a Shared Access Signature for that file that is for read access for that file
            var sasToken = file.GetSharedAccessSignature(
                new SharedAccessFilePolicy
                {
                    Permissions = SharedAccessFilePermissions.Read,
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(5))
                }
            );

            var sasCreds = new StorageCredentials(sasToken);

            var sasCloudFile = new CloudFile(
                new Uri("https://omazurestoragecli.file.core.windows.net/photos/Joins.png"),
                sasCreds
            );

            bool fileExists = await sasCloudFile.ExistsAsync();

            Check.That(fileExists).IsTrue();

            using(var ms = new MemoryStream())
            {
                await sasCloudFile.DownloadToStreamAsync(ms);

                Check.That(ms.Length).IsStrictlyGreaterThan(0);
            }
        }
    }
}
