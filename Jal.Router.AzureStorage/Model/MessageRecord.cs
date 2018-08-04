using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Jal.Router.AzureStorage.Model
{
    public class MessageRecord : TableEntity
    {
        public MessageRecord(string partitionkey, string rowkey)
        {
            PartitionKey = partitionkey;
            RowKey = rowkey;
        }

        public MessageRecord()
        {

        }

        public string Tracks { get; set; }
        public string Data { get; set; }
        public string ContentId { get; set; }
        public DateTime DateTimeUtc { get; set; }

        public string Content { get; set; }

        public string Identity { get; set; }

        public string Version { get; set; }

        public int RetryCount { get; set; }

        public bool LastRetry { get; set; }

        public string Origin { get; set; }
        public string Saga { get; set; }
        public string Headers { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }
}