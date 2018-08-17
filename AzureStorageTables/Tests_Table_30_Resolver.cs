using AzureStorageTables.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using NFluent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AzureStorageTables
{
    [TestClass]
    public class Tests_Table_30_Resolver
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
        public async Task Test_30_Resolver()
        {
            var table = _client.GetTableReference("mixtable");

            await table.CreateIfNotExistsAsync();

            // Generate mixed data
            var customer = new Customer("Orestis-Customer", "Meikopoulos")
            {
                EmailAddress = "ormeik@test.com",
                PhoneNumber = "6999999999"
            };

            var operationInsertCustomer = TableOperation.InsertOrReplace(customer);

            var staff = new Staff("Orestis-Staff", "Meikopoulos")
            {
                EmailAddress = "ormeik@test.com",
                PhoneNumber = "6999999999",
                StaffId = "007"
            };

            var operationInsertStaff = TableOperation.InsertOrReplace(staff);

            // Insert customer and staff
            await table.ExecuteAsync(operationInsertCustomer);
            await table.ExecuteAsync(operationInsertStaff);

            // Now lets query

            var query = new TableQuery<TableEntity>()
                                .Where(
                                    TableQuery.GenerateFilterCondition(
                                        "PartitionKey",
                                        QueryComparisons.Equal,
                                        "Meikopoulos"
                                    )
                                );

            var results = new List<TableEntity>();

            TableContinuationToken tct = null;

            EntityResolver<TableEntity> resolver = 
                (partitionKey, rowKey, timestamp, properties, eTag) => 
                {
                    if(properties.ContainsKey("StaffId"))
                    {
                        return new Staff(partitionKey, rowKey, timestamp, eTag, properties);
                    }
                    else
                    {
                        return new Customer(partitionKey, rowKey, timestamp, eTag, properties);
                    }
                };

            do
            {
                // We can provide an entity resolver in order to get the correct type of the entity when we have a mix
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, resolver, tct);

                tct = queryResult.ContinuationToken;

                results.AddRange(queryResult.Results);
            }
            while (tct != null);

            Check.That(results.Count).IsEqualTo(2);

            var noOfCustomers = results.Count<TableEntity>(te => te is Customer);
            var noOfStaff = results.Count<TableEntity>(te => te is Staff);

            Check.That(noOfCustomers).IsEqualTo(1);
            Check.That(noOfStaff).IsEqualTo(1);
        }
    }
}
