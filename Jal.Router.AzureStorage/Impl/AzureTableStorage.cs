﻿using System;
using System.Linq;
using System.Net;
using System.Text;
using Jal.Router.AzureStorage.Extensions;
using Jal.Router.AzureStorage.Model;
using Jal.Router.Impl.Inbound.Sagas;
using Jal.Router.Interface.Management;
using Jal.Router.Model;
using Jal.Router.Model.Inbound;
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

        private readonly string _currenttablenamesufix;

        public AzureTableStorage(string connectionstring, string sagastoragename = "sagas", string messagestorgename = "messages", string tablenamesufix = "")
        {
            _connectionstring = connectionstring;

            _sagastoragename = sagastoragename;

            _messagestorgename = messagestorgename;

            _currenttablenamesufix = tablenamesufix;
        }

        private static CloudTable GetCloudTable(string connectionstring, string tablename)
        {
            var account = CloudStorageAccount.Parse(connectionstring);

            var tableClient = account.CreateCloudTableClient();

            var table = tableClient.GetTableReference(tablename);

            return table;
        }

        private void CreateSaga<TData>(Saga saga, MessageContext context, TData data)
        {
            try
            {
                var partition = $"{context.DateTimeUtc.ToString("yyyyMMdd")}_{saga.Name}";

                var row = Guid.NewGuid().ToString();

                context.SagaInfo.SetId(partition, row, _currenttablenamesufix);

                var table = GetCloudTable(_connectionstring, $"{_sagastoragename}{_currenttablenamesufix}");

                var record = new SagaRecord(partition, row)
                {
                    Data = JsonConvert.SerializeObject(data),
                    Created = context.DateTimeUtc,
                    Updated = context.DateTimeUtc,
                    Name = saga.Name,
                    DataType = saga.DataType.FullName,
                    Timeout = saga.Timeout,
                    Status = "STARTED"
                };

                table.Execute(TableOperation.Insert(record));
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the saga record creation saga {saga.Name}", ex);
            }
        }

        private void CreateMessage(SagaRecord saga, MessageContext context, Route route, string tablenamesufix)
        {
            try
            {
                var table = GetCloudTable(_connectionstring, $"{_messagestorgename}{tablenamesufix}");

                var record = new MessageRecord(saga.RowKey, $"{route.BodyType.Name}_{Guid.NewGuid()}")
                {
                    Content = context.Body,
                    ContentType = route.BodyType.FullName,
                    Id = context.Id,
                    Version = context.Version,
                    RetryCount = context.RetryCount,
                    LastRetry = context.LastRetry,
                    Origin = JsonConvert.SerializeObject(context.Origin),
                    Saga = JsonConvert.SerializeObject(context.SagaInfo),
                    Headers = JsonConvert.SerializeObject(context.Headers),
                    DateTimeUtc = context.DateTimeUtc,
                    Name = route.Name,
                    Data = saga.Data
                };

                var size = Encoding.UTF8.GetByteCount(record.Content);

                if (size >= 64000)
                {
                    record.Content = LimitByteLength(record.Content, 63000)+ "...";
                }

                table.Execute(TableOperation.Insert(record));
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the message record creation saga {saga.Name} and route {route.Name}", ex);
            }
        }

        public static string LimitByteLength(string input, int maxLength)
        {
            for (int i = input.Length - 1; i >= 0; i--)
            {
                if (Encoding.UTF8.GetByteCount(input.Substring(0, i + 1)) <= maxLength)
                {
                    return input.Substring(0, i + 1);
                }
            }

            return string.Empty;
        }

        public SagaRecord GetSaga(MessageContext context, Saga saga, string tablenamesufix)
        {
            try
            {
                var partitionkey = context.SagaInfo.GetPartitionKey();

                var rowkey = context.SagaInfo.GetRowKey();

                if (!string.IsNullOrWhiteSpace(partitionkey) && !string.IsNullOrWhiteSpace(rowkey))
                {

                        var table = GetCloudTable(_connectionstring,$"{_sagastoragename}{tablenamesufix}");

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

        public override void Create<TData>(MessageContext context, TData data)
        {
            CreateSaga(context.Saga, context, data);
        }


        public override void Update<TData>(MessageContext context, TData data)
        {
            var tablenamesufix = context.SagaInfo.GetTableNameSufix();

            var record = GetSaga(context, context.Saga, tablenamesufix);

            if (record != null)
            {
                UpdateSaga(record, data, tablenamesufix, context.DateTimeUtc);

                CreateMessage(record, context, context.Route, tablenamesufix);
            }
        }

        public override void Create(MessageContext context)
        {
            try
            {
                var partition = $"{context.DateTimeUtc.ToString("yyyyMMdd")}_{context.Route.BodyType.Name}";

                var table = GetCloudTable(_connectionstring, $"{_messagestorgename}{_currenttablenamesufix}");

                var record = new MessageRecord(partition, $"{Guid.NewGuid()}")
                {
                    Content =context.Body,
                    ContentType = context.Route.BodyType.FullName,
                    Id = context.Id,
                    Version = context.Version,
                    RetryCount = context.RetryCount,
                    LastRetry = context.LastRetry,
                    Origin = JsonConvert.SerializeObject(context.Origin),
                    Headers = JsonConvert.SerializeObject(context.Headers),
                    DateTimeUtc = context.DateTimeUtc,
                    Name = context.Route.Name,
                };

                table.Execute(TableOperation.Insert(record));
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error during the message record creation route {context.Route.Name}", ex);
            }
        }

        private void UpdateSaga<TData>(SagaRecord record, TData data, string tablenamesufix, DateTime datetime)
        {
            try
            {
                record.Data = JsonConvert.SerializeObject(data);

                record.Updated = datetime;

                record.ETag = "*";

                var table = GetCloudTable(_connectionstring, $"{_sagastoragename}{tablenamesufix}");

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

        public override TData Find<TData>(MessageContext context)
        {
            var tablenamesufix = context.SagaInfo.GetTableNameSufix();

            var record = GetSaga(context, context.Saga, tablenamesufix);

            if (record!=null)
            {
                return JsonConvert.DeserializeObject<TData>(record.Data);
            }

            return null;
        }
    }
}
