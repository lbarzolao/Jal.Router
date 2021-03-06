using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Jal.Router.AzureStorage.Model
{
    public class SagaRecord : TableEntity
    {
        public SagaRecord(string partitionkey, string rowkey)
        {
            PartitionKey = partitionkey;
            RowKey = rowkey;
        }

        public SagaRecord()
        {
            
        }

        public string Data { get; set; }

        public string DataType { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public DateTime? Ended { get; set; }

        public int? Timeout { get; set; }

        public string Status { get; set; }

        public double Duration { get; set; }
    }
}