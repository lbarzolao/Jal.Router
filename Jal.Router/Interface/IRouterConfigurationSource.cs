﻿using Jal.Router.Model;
using Jal.Router.Model.Management;

namespace Jal.Router.Interface
{
    public interface IRouterConfigurationSource
    {
        Route[] GetRoutes();

        Saga[] GetSagas();

        EndPoint[] GetEndPoints();

        SubscriptionToPublishSubscribeChannel[] GetSubscriptionsToPublishSubscribeChannel();

        PublishSubscribeChannel[] GetPublishSubscribeChannels();

        PointToPointChannel[] GetPointToPointChannels();
    }
}