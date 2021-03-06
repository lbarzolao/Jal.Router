using System;
using System.Collections.Generic;
using System.Linq;
using Jal.Router.Interface;
using Jal.Router.Interface.Management;
using Jal.Router.Interface.Outbound;
using Jal.Router.Model;

namespace Jal.Router.Impl.Outbound
{
    public class Bus : IBus
    {
        private readonly IEndPointProvider _provider;

        private readonly IComponentFactory _factory;

        private readonly IConfiguration _configuration;

        private readonly IPipeline _pipeline;

        public Bus(IEndPointProvider provider, IComponentFactory factory, IConfiguration configuration, IPipeline pipeline)
        {
            _provider = provider;
            _factory = factory;
            _configuration = configuration;
            _pipeline = pipeline;
        }

        private TResult Reply<TResult>(MessageContext message, Options options)
        {
            var interceptor = _factory.Create<IBusInterceptor>(_configuration.BusInterceptorType);

            interceptor.OnEntry(message, options);

            try
            {
                if (message.EndPoint.Channels.Any())
                {
                    var middlewares = new List<Type>
                    {
                        typeof(DistributionHandler)
                    };

                    middlewares.AddRange(_configuration.OutboundMiddlewareTypes);

                    middlewares.AddRange(message.EndPoint.MiddlewareTypes);

                    middlewares.Add(typeof(RequestReplyHandler));

                    var result = _pipeline.Execute(middlewares.ToArray(), message, options, "Reply", typeof(TResult));

                    interceptor.OnSuccess(message, options);

                    return (TResult)result;
                }
                else
                {
                    throw new ApplicationException($"Endpoint {message.EndPoint.Name}, missing channels");
                }
            }
            catch (Exception ex)
            {
                interceptor.OnError(message, options, ex);

                throw;
            }
            finally
            {
                interceptor.OnExit(message, options);
            }
        }
        public TResult Reply<TContent, TResult>(TContent content, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            return Reply<TContent, TResult>(content, endpoint, endpoint.Origin, options);
        }
        public TResult Reply<TContent, TResult>(TContent content, EndPoint endpoint, Origin origin, Options options)
        {
            var serializer = _factory.Create<IMessageSerializer>(_configuration.MessageSerializerType);

            var message = new MessageContext(endpoint, options)
            {
                Origin = origin,
                ContentType = content.GetType(),
                DateTimeUtc = DateTime.UtcNow,
                Content = serializer.Serialize(content),
                ResultType = typeof(TResult),
                Tracks = options.Tracks
            };

            return Reply<TResult>(message, options);
        }
        public TResult Reply<TContent, TResult>(TContent content, Origin origin, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            if (string.IsNullOrWhiteSpace(origin.From))
            {
                origin.From = endpoint.Origin.From;
            }

            if (string.IsNullOrWhiteSpace(origin.Key))
            {
                origin.Key = endpoint.Origin.Key;
            }

            return Reply<TContent, TResult>(content, endpoint, origin, options);
        }
        private void Send(MessageContext message, Options options)
        {
            var interceptor = _factory.Create<IBusInterceptor>(_configuration.BusInterceptorType);

            interceptor.OnEntry(message, options);

            try
            {
                if (message.EndPoint.Channels.Any())
                {
                    var middlewares = new List<Type>
                    {
                        typeof(DistributionHandler)
                    };

                    middlewares.AddRange(_configuration.OutboundMiddlewareTypes);

                    middlewares.AddRange(message.EndPoint.MiddlewareTypes);

                    middlewares.Add(typeof(PointToPointHandler));

                    _pipeline.Execute(middlewares.ToArray(), message, options, "Send");
                }
                else
                {
                    throw new ApplicationException($"Endpoint {message.EndPoint.Name}, missing channels");
                }

                interceptor.OnSuccess(message, options);
            }
            catch (Exception ex)
            {
                interceptor.OnError(message, options, ex);

                throw;
            }
            finally
            {
                interceptor.OnExit(message, options);
            }
        }

        public void Send<TContent>(TContent content, EndPoint endpoint, Origin origin, Options options)
        {
            var serializer = _factory.Create<IMessageSerializer>(_configuration.MessageSerializerType);

            var message = new MessageContext(endpoint, options)
            {
                Origin = origin,
                ContentType = content.GetType(),
                DateTimeUtc = DateTime.UtcNow,
                Content = serializer.Serialize(content),
                Tracks = options.Tracks
            };

            Send(message, options);
        }

        public void Send<TContent>(TContent content, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            var origin = endpoint.Origin;

            Send(content, endpoint, origin, options);
        }
        public void Send<TContent>(TContent content, Origin origin, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            if (string.IsNullOrWhiteSpace(origin.From))
            {
                origin.From = endpoint.Origin.From;
            }

            if (string.IsNullOrWhiteSpace(origin.Key))
            {
                origin.Key = endpoint.Origin.Key;
            }

            Send(content, endpoint, origin, options);
        } 
        public void Publish<TContent>(TContent content, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            var origin = endpoint.Origin;

            Publish(content, endpoint, origin, options);
        }
        public void Publish<TContent>(TContent content, Origin origin, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            if (string.IsNullOrWhiteSpace(origin.From))
            {
                origin.From = endpoint.Origin.From;
            }

            if (string.IsNullOrWhiteSpace(origin.Key))
            {
                origin.Key = endpoint.Origin.Key;
            }

            Publish(content, endpoint, origin, options);
        }
        public void Publish<TContent>(TContent content, EndPoint endpoint, Origin origin, Options options)
        {
            var serializer = _factory.Create<IMessageSerializer>(_configuration.MessageSerializerType);

            var message = new MessageContext(endpoint, options)
            {
                Origin = origin,
                ContentType = content.GetType(),
                DateTimeUtc = DateTime.UtcNow,
                Content = serializer.Serialize(content),
                Tracks = options.Tracks
            };

            Publish(message, options);
        }

        private void Publish(MessageContext message, Options options)
        {
            var interceptor = _factory.Create<IBusInterceptor>(_configuration.BusInterceptorType);

            interceptor.OnEntry(message, options);

            try
            {
                if (message.EndPoint.Channels.Any())
                {
                    var middlewares = new List<Type>
                    {
                        typeof(DistributionHandler)
                    };

                    middlewares.AddRange(_configuration.OutboundMiddlewareTypes);

                    middlewares.AddRange(message.EndPoint.MiddlewareTypes);

                    middlewares.Add(typeof(PublishSubscribeHandler));

                    _pipeline.Execute(middlewares.ToArray(), message, options, "Publish");
                }
                else
                {
                    throw new ApplicationException($"Endpoint {message.EndPoint.Name}, missing channels");
                }

                interceptor.OnSuccess(message, options);

            }
            catch (Exception ex)
            {
                interceptor.OnError(message, options, ex);

                throw;
            }
            finally
            {
                interceptor.OnExit(message, options);
            }
        }

        public void FireAndForget<TContent>(TContent content, EndPoint endpoint, Origin origin, Options options)
        {
            var serializer = _factory.Create<IMessageSerializer>(_configuration.MessageSerializerType);

            var message = new MessageContext(endpoint, options)
            {
                Origin = origin,
                ContentType = content.GetType(),
                DateTimeUtc = DateTime.UtcNow,
                Content = serializer.Serialize(content),
                Tracks = options.Tracks
            };

            message.Origin.Key = string.Empty;

            Send(message, options);
        }

        public void FireAndForget<TContent>(TContent content, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            FireAndForget(content, endpoint, new Origin() { Key = endpoint.Origin.Key, From = endpoint.Origin.From }, options);
        }

        public void FireAndForget<TContent>(TContent content, Origin origin, Options options)
        {
            var endpoint = _provider.Provide(options.EndPointName, content.GetType());

            if (string.IsNullOrWhiteSpace(origin.From))
            {
                origin.From = endpoint.Origin.From;
            }

            if (string.IsNullOrWhiteSpace(origin.Key))
            {
                origin.Key = endpoint.Origin.Key;
            }

            FireAndForget(content, endpoint, origin, options);
        }
    }
}