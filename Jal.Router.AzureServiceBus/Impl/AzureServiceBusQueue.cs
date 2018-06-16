using System;
using Jal.Router.Impl;
using Jal.Router.Interface;
using Jal.Router.Interface.Management;
using Jal.Router.Model;
using Microsoft.ServiceBus.Messaging;

namespace Jal.Router.AzureServiceBus.Impl
{
    public class AzureServiceBusQueue : AbstractPointToPointChannel
    {
        public override string Send(MessageContext context, object message)
        {
            var queueclient = QueueClient.CreateFromConnectionString(context.ToConnectionString, context.ToPath);

            var bm = message as BrokeredMessage;

            if (bm != null)
            {
                queueclient.Send(bm);

                queueclient.Close();

                return bm.MessageId;
            }

            return string.Empty;
        }

        public override void Listen(Channel channel, Action<object>[] routeactions, string channelpath)
        {
            var client = QueueClient.CreateFromConnectionString(channel.ToConnectionString, channel.ToPath);

            var receiver = client.MessagingFactory.CreateMessageReceiver(channel.ToPath);

            var options = CreateOptions();

            Action<BrokeredMessage> routeaction = message =>
            {
                foreach (var action in routeactions)
                {
                    var clone = message.Clone();
                    action(clone);
                }
            };


            receiver.OnMessage(bm => OnMessage(channelpath, bm.MessageId,()=> routeaction(bm), () => client.Complete(bm.LockToken)), options);

            channel.ShutdownAction = () => { receiver.Close(); client.Close(); };
        }

        private OnMessageOptions CreateOptions()
        {
            var options = new OnMessageOptions() {AutoComplete = false};

            if (_maxconcurrentcalls > 0)
            {
                options.MaxConcurrentCalls = _maxconcurrentcalls;
            }
            if (_autorenewtimeout != null)
            {
                options.AutoRenewTimeout = _autorenewtimeout.Value;
            }
            return options;
        }

        private readonly int _maxconcurrentcalls;

        private readonly TimeSpan? _autorenewtimeout;

        public AzureServiceBusQueue(IComponentFactory factory, IConfiguration configuration, IChannelPathBuilder builder, int maxconcurrentcalls=0, TimeSpan? autorenewtimeout=null) 
            : base(factory, configuration, builder)
        {
            _maxconcurrentcalls = maxconcurrentcalls;
            _autorenewtimeout = autorenewtimeout;
        }
    }
}