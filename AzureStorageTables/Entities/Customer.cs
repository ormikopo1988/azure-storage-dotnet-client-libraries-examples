using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace AzureStorageTables.Entities
{
    public class Customer : TableEntity
    {
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        // Required Constructor
        public Customer()
        {

        }

        public Customer(string firstName, string lastName)
        {
            // more diversity => more scalability
            // PartionKey + RowKey together => Unique Identifier of the entity
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public Customer(string partitionKey, string rowKey, DateTimeOffset timestamp, string eTag, IDictionary<string, EntityProperty> props)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
            this.Timestamp = timestamp;
            this.ETag = eTag;

            if(props.ContainsKey("EmailAddress"))
            {
                this.EmailAddress = props["EmailAddress"].ToString();
            }
            
            if (props.ContainsKey("PhoneNumber"))
            {
                this.EmailAddress = props["PhoneNumber"].ToString();
            }
        }
    }
}
