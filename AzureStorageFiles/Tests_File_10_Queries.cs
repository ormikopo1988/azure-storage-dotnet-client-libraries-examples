using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using NFluent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureStorageFiles
{
    [TestClass]
    public class Tests_File_10_Queries
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
        public async Task Test_10_QueryForShares()
        {
            var shares = new List<CloudFileShare>();

            FileContinuationToken fct = null;

            do
            {
                var srs = await _client.ListSharesSegmentedAsync(fct);

                fct = srs.ContinuationToken;

                shares.AddRange(srs.Results);
            }
            while (fct != null);

            Check.That(shares.Count).IsEqualTo(1);
        }

        [TestMethod]
        public async Task Test_11_QueryForFilesAndDirectories()
        {
            var share = _client.GetShareReference("demo01");

            var shareExists = await share.ExistsAsync();

            Check.That(shareExists).IsTrue();

            var root = share.GetRootDirectoryReference();

            var rootExists = await root.ExistsAsync();

            Check.That(rootExists).IsTrue();

            var queryResults = new List<IListFileItem>();

            FileContinuationToken fct = null;

            do
            {
                var queryResult = await root.ListFilesAndDirectoriesSegmentedAsync(fct);

                queryResults.AddRange(queryResult.Results);

                fct = queryResult.ContinuationToken;
            }
            while (fct != null);

            Check.That(queryResults.Count).IsStrictlyGreaterThan(0);
        }
    }
}
