using AzureStorageTables.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using NFluent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AzureStorageTables
{
    [TestClass]
    public class Tests_Table_20_Queries
    {
        private static TestContext _context = null;
        private static CloudTableClient _client = null;

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

            _client = storageAccountConfig.CreateCloudTableClient();
        }

        [TestMethod]
        public async Task Test_20_Query()
        {
            var table = _client.GetTableReference("testnewtable");

            bool tableExists = await table.ExistsAsync();

            Check.That(tableExists).IsTrue();

            var query = new TableQuery<Customer>()
                                .Where(
                                    TableQuery.GenerateFilterCondition(
                                        "PartitionKey",
                                        QueryComparisons.Equal,
                                        "meik"
                                    )
                                );

            var customerEntities = new List<Customer>();

            TableContinuationToken tct = null;

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, tct);

                tct = queryResult.ContinuationToken;

                customerEntities.AddRange(queryResult.Results);
            }
            while (tct != null);

            Check.That(customerEntities.Count).IsStrictlyGreaterThan(0);
        }

        [TestMethod]
        public async Task Test_21_QueryWithCompinedFilters()
        {
            var table = _client.GetTableReference("testnewtable");

            bool tableExists = await table.ExistsAsync();

            Check.That(tableExists).IsTrue();

            var query = new TableQuery<Customer>()
                                .Where(
                                    TableQuery.CombineFilters(
                                        TableQuery.GenerateFilterCondition(
                                            "PartitionKey",
                                            QueryComparisons.Equal,
                                            "Meikopoulos"
                                        ),
                                        TableOperators.And,
                                        TableQuery.GenerateFilterCondition(
                                            "RowKey",
                                            QueryComparisons.Equal,
                                            "Orestis"
                                        )
                                    )
                                );

            var customerEntities = new List<Customer>();

            TableContinuationToken tct = null;

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, tct);

                tct = queryResult.ContinuationToken;

                customerEntities.AddRange(queryResult.Results);
            }
            while (tct != null);

            Check.That(customerEntities.Count).IsEqualTo(1);
        }

        [TestMethod]
        public async Task Test_22_QueryWithOnlySelectedColumns()
        {
            var table = _client.GetTableReference("testnewtable");

            bool tableExists = await table.ExistsAsync();

            Check.That(tableExists).IsTrue();

            var query = new TableQuery().Select(new string[] { "EmailAddress" });

            var dynamicTableEntities = new List<DynamicTableEntity>();

            TableContinuationToken tct = null;

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, tct);

                tct = queryResult.ContinuationToken;

                dynamicTableEntities.AddRange(queryResult.Results);
            }
            while (tct != null);

            Check.That(dynamicTableEntities.Count).IsStrictlyGreaterThan(0);

            foreach (DynamicTableEntity e in dynamicTableEntities)
            {
                Check.That(e.Properties.TryGetValue("EmailAddress", out EntityProperty value)).IsTrue();
            }
        }
    }
}
