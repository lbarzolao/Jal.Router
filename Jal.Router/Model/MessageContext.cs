using System;
using System.Collections.Generic;
using System.Linq;

namespace Jal.Router.Model
{
    public class MessageContext
    {
        public string EndPointName { get; set; }
        public string ToConnectionString { get; set; }
        public string ToSubscription { get; set; }
        public string ToPath { get; set; }
        public string Id { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public string Version { get; set; }
        public int RetryCount { get; set; }
        public bool LastRetry { get; set; }
        public Route Route { get; set; }     
        public Origin Origin { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public SagaInfo SagaInfo { get; set; }
        public Saga Saga { get; set; }
        public DateTime? ScheduledEnqueueDateTimeUtc { get; set; }
        public Type ContentType { get; set; }
        public Type ResultType { get; set; }
        public string ContentAsString { get; set; }
        public string ToReplyConnectionString { get; set; }
        public string ToReplyPath { get; set; }
        public int ToReplyTimeOut { get; set; }
        public string ToReplySubscription { get; set; }
        public string ReplyToRequestId { get; set; }
        public string RequestId { get; set; }
        public object Content { get; set; }
        public MessageContext()
        {
            Headers = new Dictionary<string, string>();
            Version = "1";
            LastRetry = true;
            Origin = new Origin();
            SagaInfo = new SagaInfo();
        }

        public Dictionary<string, string> CopyHeaders()
        {
            return Headers.ToDictionary(header => header.Key, header => header.Value);
        }
    }
}