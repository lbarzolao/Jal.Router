using Jal.Router.AzureServiceBus.Model;
using Microsoft.ServiceBus.Messaging;

namespace Jal.Router.AzureServiceBus.Interface
{
    public interface IBrokeredMessageRouter
    {
        void Route<TContent>(BrokeredMessage brokeredMessage, string name = "");

        void Reply<TContent>(TContent content, BrokeredMessageContext context);
    }
}