using System;
using Jal.Router.Impl;
using Jal.Router.Interface;
using Jal.Router.Model.Management;

namespace Jal.Router.AzureServiceBus.Extensions
{
    public static class AbstractRouterConfigurationSourceExtensions
    {
        public static void RegisterQueue<TExtractorConectionString>(this AbstractRouterConfigurationSource configuration, string name, Func<IValueSettingFinder, string> connectionstringextractor)
            where TExtractorConectionString : IValueSettingFinder
        {
            configuration.RegisterPointToPointChannel<TExtractorConectionString>(name, connectionstringextractor);
        }

        public static void RegisterQueue(this AbstractRouterConfigurationSource configuration, string name, string connectionstring)
        {
            configuration.RegisterPointToPointChannel(name, connectionstring);
        }

        public static void RegisterTopic<TExtractorConectionString>(this AbstractRouterConfigurationSource configuration, string name,
            Func<IValueSettingFinder, string> connectionstringextractor)
            where TExtractorConectionString : IValueSettingFinder
        {
            configuration.RegisterPublishSubscriberChannel<TExtractorConectionString>(name, connectionstringextractor);
        }

        public static void RegisterTopic(this AbstractRouterConfigurationSource configuration, string name, string connectionstring)
        {
            configuration.RegisterPublishSubscriberChannel(name, connectionstring);
        }

        public static void RegisterSubscriptionToTopic<TExtractorConectionString>(this AbstractRouterConfigurationSource configuration, string name, string path,
            Func<IValueSettingFinder, string> connectionstring, SubscriptionToPublishSubscribeChannelRule rule = null)
            where TExtractorConectionString : IValueSettingFinder
        {
            configuration.RegisterSubscriptionToPublishSubscriberChannel<TExtractorConectionString>(name, path, connectionstring, rule);
        }

        public static void RegisterSubscriptionToTopic(this AbstractRouterConfigurationSource configuration, string name, string path,
            string connectionstring, SubscriptionToPublishSubscribeChannelRule rule = null)
        {
            configuration.RegisterSubscriptionToPublishSubscriberChannel(name, path, connectionstring, rule);
        }
    }
}