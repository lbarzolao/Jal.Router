using Microsoft.ServiceBus.Messaging;

namespace Jal.Router.AzureServiceBus.Interface
{
    public interface IBrokeredMessageReplyToAdapter
    {
        string ReadPath(BrokeredMessage message);

        void WritePath(string path, BrokeredMessage message);

        string ReadConnectionString(BrokeredMessage message);

        void WriteConnectionString(string connectionstring, BrokeredMessage message);
    }
}