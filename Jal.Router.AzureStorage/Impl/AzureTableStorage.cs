﻿using System;
using System.Net;
using Jal.Router.AzureStorage.Extensions;
using Jal.Router.AzureStorage.Model;
using Jal.Router.Impl;
using Jal.Router.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Jal.Router.AzureStorage.Impl
{
    public class AzureTableStorage : AbstractStorage
    {
        private readonly string _connectionstring;

        private readonly string _sagastoragename;

        private readonly string _messagestorgename;

        public AzureTableStorage(string connectionstring, string sagastoragename = "sagas", string messagestorgename = "messages")
        {
            _connectionstring = connectionstring;

            _sagastoragename = sagastoragename;

            _messagestorgename = messagestorgename;
        }

        private static CloudTable GetCloudTable(string connectionstring, string tablename)
        {
            var account = CloudStorageAccount.Parse(connectionstring);

            var tableClient = account.CreateCloudTableClient();

            var table = tableClient.GetTableReference(tablename);

            return table;
        }

        private SagaRecord CreateSaga<TContent, TData>(Saga<TData> saga, InboundMessageContext<TContent> context, TData data)
        {
            try
            {
                var table = GetCloudTable(_connectionstring, _sagastoragename);

                var record = new SagaRecord($"{context.DateTimeUtc.ToString("yyyyMMdd")}_{saga.Name}", Guid.NewGuid().ToString())
                {
                    Data = JsonConvert.SerializeObject(data),
                    Created = context.DateTimeUtc,
                    Name = saga.Name,
                    DataType = saga.DataType.FullName
                };

                table.Execute(TableOperation.Insert(record));

                return record;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the saga record creation saga {saga.Name}", ex);
            }
        }

        private void CreateMessage<TContent>(SagaRecord saga, InboundMessageContext<TContent> context, Route route)
        {
            try
            {
                var table = GetCloudTable(_connectionstring, _messagestorgename);

                var record = new MessageRecord(saga.RowKey, $"{route.BodyType.Name}_{Guid.NewGuid()}")
                {
                    Content = JsonConvert.SerializeObject(context.Content),
                    ContentType = route.BodyType.FullName,
                    Id = context.Id,
                    Version = context.Version,
                    RetryCount = context.RetryCount,
                    LastRetry = context.LastRetry,
                    Origin = JsonConvert.SerializeObject(context.Origin),
                    Saga = JsonConvert.SerializeObject(context.Saga),
                    Headers = JsonConvert.SerializeObject(context.Headers),
                    DateTimeUtc = context.DateTimeUtc,
                    Name = route.Name,
                    Data = saga.Data
                };

                table.Execute(TableOperation.Insert(record));
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the message record creation saga {saga.Name} and route {route.Name}", ex);
            }
        }

        public SagaRecord GetSaga<TContent>(InboundMessageContext<TContent> context, Saga saga)
        {
            try
            {
                var partitionkey = context.Saga.GetPartitionKey();

                var rowkey = context.Saga.GetRowKey();

                if (!string.IsNullOrWhiteSpace(partitionkey) && !string.IsNullOrWhiteSpace(rowkey))
                {

                        var table = GetCloudTable(_connectionstring, _sagastoragename);

                        var result = table.Execute(TableOperation.Retrieve<SagaRecord>(partitionkey, rowkey));

                        return result.Result as SagaRecord;
                    
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the saga record lookup saga {saga.Name}", ex);
            }

            return null;
        }

        public override void Create<TContent, TData>(Saga<TData> saga, InboundMessageContext<TContent> context, Route route, TData data)
        {
            var record = CreateSaga(saga, context, data);

            context.Saga.SetId(record.PartitionKey,record.RowKey);
        }


        public override void Update<TContent, TData>(Saga<TData> saga, InboundMessageContext<TContent> context, Route route, TData data)
        {
            var record = GetSaga(context, saga);

            if (record != null)
            {
                UpdateSaga(record, data);

                CreateMessage(record, context, route);
            }
        }

        private void UpdateSaga<TData>(SagaRecord record, TData data)
        {
            try
            {
                record.Data = JsonConvert.SerializeObject(data);

                var table = GetCloudTable(_connectionstring, _sagastoragename);

                table.Execute(TableOperation.Replace(record));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    throw new ApplicationException($"Error during the saga update (Optimistic concurrency violation – entity has changed since it was retrieved) {record.Name}", ex);
                }
                else
                {
                    throw new ApplicationException($"Error during the saga update ({ex.RequestInformation.HttpStatusCode}) {record.Name}", ex);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the saga update {record.Name}", ex);
            }
        }

        public override TData Find<TContent, TData>(Saga<TData> saga, InboundMessageContext<TContent> context, Route route)
        {
            var record = GetSaga(context, saga);

            if (record!=null)
            {
                return JsonConvert.DeserializeObject<TData>(record.Data);
            }

            return null;
        }
    }
}