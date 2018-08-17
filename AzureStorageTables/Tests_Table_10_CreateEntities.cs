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
    public class Tests_Table_10_CreateEntities
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
        public async Task Test_10_CreateEntity()
        {
            var table = _client.GetTableReference("testnewtable");

            await table.CreateIfNotExistsAsync();

            ITableEntity entity = new Customer("Orestis", "Meikopoulos")
            {
                EmailAddress = "ormeik@test.com",
                PhoneNumber = "6999999999"
            };

            var op = TableOperation.Insert(entity);

            await table.ExecuteAsync(op);
        }

        [TestMethod]
        public async Task Test_11_Batch()
        {
            var table = _client.GetTableReference("testnewtable");

            await table.CreateIfNotExistsAsync();

            var customers = LoadCustomers();

            TableBatchOperation batchOperation = new TableBatchOperation();

            foreach (var c in customers)
            {
                var op = TableOperation.Insert(c);

                batchOperation.Add(op);
            }
            
            await table.ExecuteBatchAsync(batchOperation);
        }

        private List<Customer> LoadCustomers()
        {
            var random = new Random();

            var list = new List<Customer>();

            for(int i=0; i<100; i++) // 100 is indeed the max batch size
            {
                string ln = "meik"; // all batch inserts must have same 

                string fn = "fn" + random.Next(1000, 5000).ToString(); // likely to generate duplicates

                if(list.Count<Customer>((ce) => ce.RowKey == fn) > 0)
                {
                    continue; // no duplicates
                }

                string pn = random.Next(10000000, 99999999).ToString();
                string email = $"{fn}@{ln}.com";

                list.Add(
                    new Customer(fn, ln)
                    {
                        EmailAddress = email,
                        PhoneNumber = pn
                    }
                );
            }

            return list;
        }
    }
}
