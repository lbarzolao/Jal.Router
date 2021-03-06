using System;
using Jal.Router.Interface;
using Jal.Router.Model.Management;
using Microsoft.ApplicationInsights;

namespace Jal.Router.ApplicationInsights.Impl
{
    public class ApplicationInsightsSubscriptionToPublishSubscribeChannelInfoLogger : ILogger<SubscriptionToPublishSubscribeChannelInfo>
    {
        private readonly TelemetryClient _client;
        public ApplicationInsightsSubscriptionToPublishSubscribeChannelInfoLogger(TelemetryClient client)
        {
            _client = client;
        }

        public void Log(SubscriptionToPublishSubscribeChannelInfo info, DateTime datetime)
        {
            _client.TrackMetric($"{info.Path}/{info.Subscription} - MessageCount", info.MessageCount);
            _client.TrackMetric($"{info.Path}/{info.Subscription} - DeadLetterMessageCount", info.DeadLetterMessageCount);
            _client.TrackMetric($"{info.Path}/{info.Subscription} - ScheduledMessageCount", info.ScheduledMessageCount);
        }
    }
}