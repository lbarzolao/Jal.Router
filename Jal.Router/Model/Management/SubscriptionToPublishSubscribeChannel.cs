using System;

namespace Jal.Router.Model.Management
{
    public class SubscriptionToPublishSubscribeChannel
    {
        public SubscriptionToPublishSubscribeChannel(string name)
        {
            Name = name;
        }
        public Origin Origin { get; set; }

        public string Name { get; set; }

        public Type PathExtractorType { get; set; }

        public Type ConnectionStringExtractorType { get; set; }

        public object ToConnectionStringExtractor { get; set; }

        public string TopicPath { get; set; }
    }
}