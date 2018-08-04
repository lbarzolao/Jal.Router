using System;
using Jal.Router.Model;
using Jal.Router.Model.Inbound;

namespace Jal.Router.Impl.Inbound.Sagas
{
    public class AbstractMiddlewareMessageHandler
    {
        protected SagaEntity CreateSagaEntity(MessageContext context, MiddlewareParameter parameter)
        {
            return new SagaEntity
            {
                Created = context.DateTimeUtc,
                Updated = context.DateTimeUtc,
                Name = parameter.Saga.Name,
                DataType = parameter.Saga.DataType.FullName,
                Timeout = parameter.Saga.Timeout,
                Status = context.SagaContext.Status,
                Key = Guid.NewGuid().ToString()
            };
        }

        protected static MessageEntity CreateMessageEntity(MessageContext context, MiddlewareParameter parameter, SagaEntity sagaentity)
        {
            return new MessageEntity
            {
                Content = context.Content,
                ContentType = parameter.Route.ContentType.FullName,
                Identity = context.Identity,
                Version = context.Version,
                RetryCount = context.RetryCount,
                LastRetry = context.LastRetry,
                Origin = context.Origin,
                Saga = context.SagaContext,
                Headers = context.Headers,
                DateTimeUtc = context.DateTimeUtc,
                Data = sagaentity.Data,
                Name = parameter.Route.Name,
                Tracks = context.Tracks,
                ContentId = context.ContentId
            };
        }
    }
}