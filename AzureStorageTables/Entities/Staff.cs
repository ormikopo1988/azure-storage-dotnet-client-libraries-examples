using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace AzureStorageTables.Entities
{
    public class Staff : TableEntity
    {
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string StaffId { get; set; }

        // Required Constructor
        public Staff()
        {

        }

        public Staff(string firstName, string lastName)
        {
            // more diversity => more scalability
            // PartionKey + RowKey together => Unique Identifier of the entity
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public Staff(string partitionKey, string rowKey, DateTimeOffset timestamp, string eTag, IDictionary<string, EntityProperty> props)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
            this.Timestamp = timestamp;
            this.ETag = eTag;

            if (props.ContainsKey("EmailAddress"))
            {
                this.EmailAddress = props["EmailAddress"].ToString();
            }

            if (props.ContainsKey("PhoneNumber"))
            {
                this.EmailAddress = props["PhoneNumber"].ToString();
            }

            if (props.ContainsKey("StaffId"))
            {
                this.StaffId = props["StaffId"].ToString();
            }
        }
    }
}
