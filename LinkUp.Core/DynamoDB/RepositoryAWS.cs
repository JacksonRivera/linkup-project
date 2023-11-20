using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using LinkUp.Core.AWS.Business; 
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using System.Reflection;
using Newtonsoft.Json;
using System.Drawing;

namespace LinkUp.Core.AWS.DynamoDB
{
    public class RepositoryAWS<T> where T : DynamoDBTableEntity, new()
    {
        public AmazonDynamoDBClient Client;
        public DynamoDBContext Context;
        private IServiceProvider _service;

        public RepositoryAWS()
        {
            var awsAccessKeyId = Environment.GetEnvironmentVariable("awsAccessKeyId", EnvironmentVariableTarget.Process);
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("awsSecretAccessKey", EnvironmentVariableTarget.Process);
            var awsRegionName = Environment.GetEnvironmentVariable("awsRegionName", EnvironmentVariableTarget.Process);

            Client = new AmazonDynamoDBClient(awsAccessKeyId: awsAccessKeyId, awsSecretAccessKey: awsSecretAccessKey, GetRegion(awsRegionName));
            Context = new DynamoDBContext(Client);
        }

        public RepositoryAWS(IServiceProvider service)
        {
            _service = service;

            var options = _service.GetService<IOptions<AWSSettings>>();
            var awsAccessKeyId = options.Value.AWSAccessKeyId;
            var awsSecretAccessKey = options.Value.AWSSecretAccessKey;
            var regionName = options.Value.RegionName;

            Client = new AmazonDynamoDBClient(awsAccessKeyId: awsAccessKeyId, awsSecretAccessKey: awsSecretAccessKey, GetRegion(regionName));
            Context = new DynamoDBContext(Client);
        }

        public RepositoryAWS(string awsAccessKeyIdName, string awsSecretAccessKeyName)
        {
            var awsAccessKeyId = Environment.GetEnvironmentVariable(awsAccessKeyIdName, EnvironmentVariableTarget.Process);
            var awsSecretAccessKey = Environment.GetEnvironmentVariable(awsSecretAccessKeyName, EnvironmentVariableTarget.Process);

            Client = new AmazonDynamoDBClient(awsAccessKeyId: awsAccessKeyId, awsSecretAccessKey: awsSecretAccessKey, GetRegion());
            Context = new DynamoDBContext(Client);
        }

        public RepositoryAWS(string awsAccessKeyId, string awsSecretAccessKey, string awsRegionName)
        {
            Client = new AmazonDynamoDBClient(awsAccessKeyId: awsAccessKeyId, awsSecretAccessKey: awsSecretAccessKey, GetRegion(awsRegionName));
            Context = new DynamoDBContext(Client);
        }

        public object Get(object defaultAppName, string shopId)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> FilterScan(List<ScanCondition> conditions)
        {
            try
            {
                var result = Context.ScanAsync<T>(conditions).GetRemainingAsync();

                return result;
            }
            catch (Exception)
            {
            }

            return Task.FromResult(new List<T>());
        }

        public Task<List<T>> FilterWithPartitionKey(string partitionKey, List<ScanCondition> filter)
        {
            try
            {
                var config = new DynamoDBOperationConfig()
                {
                    QueryFilter = filter
                };
                var result = Context.QueryAsync<T>(partitionKey, config).GetRemainingAsync();

                return result;
            }
            catch (Exception e)
            {
            }

            return Task.FromResult(new List<T>());
        }

        public string GetNextPaginationToken(AsyncSearch<T> asyncSearch)
        {
            string paginationToken = null;

            if (!asyncSearch.IsDone)
            {
                var searchProperty = asyncSearch.GetType()
                    .GetProperty("DocumentSearch", BindingFlags.NonPublic
                    | BindingFlags.Instance);
                var searchGetter = searchProperty.GetGetMethod(nonPublic: true);
                var search = (Search)searchGetter.Invoke(asyncSearch, null);

                paginationToken = search.PaginationToken;
            }

            return paginationToken;
        }

        public Task<List<T>> GetAll()
        {
            try
            {
                var conditions = new List<ScanCondition>();
                var result = Context.ScanAsync<T>(conditions).GetRemainingAsync();

                return result;
            }
            catch (Exception)
            {
            }

            return Task.FromResult(new List<T>());
        }

        public AsyncSearch<T> GetWithPagination(string paginationToken, int limit, QueryFilter filter, DynamoDBOperationConfig operationConfig = null, string indexName = null, List<string> attributesToGet = null)
        {

            var config = new QueryOperationConfig()
            {
                IndexName = indexName,
                BackwardSearch = true,
                Filter = filter,
                Limit = limit,
                PaginationToken = paginationToken,
                AttributesToGet = attributesToGet,
                Select = attributesToGet != null && attributesToGet.Count > 0 ? SelectValues.SpecificAttributes : string.IsNullOrEmpty(indexName) ? SelectValues.AllAttributes : SelectValues.AllProjectedAttributes
            };

            var result = FromQueryAsync(config, operationConfig);

            return result;
        }

        public Task<List<T>> FilterAsync(string partitionKey, QueryFilter rowKeyFilter = null, QueryOperationConfig queryConfig = null, string partitionKeyName = "PartitionKey", DynamoDBOperationConfig config = null)
        {
            try
            {
                var partitionKeys = GetPartitions(partitionKey);

                queryConfig = queryConfig != null ? queryConfig : new QueryOperationConfig();
                queryConfig.Select = queryConfig.AttributesToGet != null && queryConfig.AttributesToGet.Count > 0 ? SelectValues.SpecificAttributes : string.IsNullOrEmpty(queryConfig.IndexName) ? SelectValues.AllAttributes : SelectValues.AllProjectedAttributes;

                if (partitionKeys.Count == 1)
                {
                    var result = ExecuteFilterAsync(partitionKeys.FirstOrDefault(), partitionKeyName, queryConfig, rowKeyFilter, config);
                    return result;
                }
                else if (partitionKeys.Count > 1)
                {
                    var result = GetFilterInParallelInWithBatches(partitionKeys, partitionKeyName, 100, queryConfig, rowKeyFilter, config);
                    return result;
                }
                else
                {
                    var result = ExecuteFilterAsync(partitionKey, partitionKeyName, queryConfig, rowKeyFilter, config);
                    return result;
                }
            }
            catch (Exception e)
            {
            }

            return Task.FromResult(new List<T>());
        }

        public AsyncSearch<T> FromQueryAsync(QueryOperationConfig QueryConfig, DynamoDBOperationConfig config = null)
        {

            var result = Context.FromQueryAsync<T>(QueryConfig, config);

            return result;
        }

        public Task<List<T>> Get(string partitionKey, DynamoDBOperationConfig config = null)
        {
            try
            {
                var partitionKeys = GetPartitions(partitionKey);

                if (partitionKeys.Count == 1)
                {
                    var result = Context.QueryAsync<T>(partitionKeys.FirstOrDefault(), config).GetRemainingAsync();
                    return result;
                }
                else if (partitionKeys.Count > 1)
                {
                    var result = GetInParallelInWithBatches(partitionKeys, 100, config);
                    return result;
                }
                else
                {
                    var result = Context.QueryAsync<T>(partitionKey, config).GetRemainingAsync();
                    return result;
                }

            }
            catch (Exception e)
            {
            }

            return Task.FromResult(new List<T>());
        }

        private async Task<List<T>> GetInParallelInWithBatches(List<string> partitionKeys, int batchSize, DynamoDBOperationConfig config)
        {
            var tasks = new List<Task<List<T>>>();
            int numberOfBatches = (int)Math.Ceiling((double)partitionKeys.Count() / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentIds = partitionKeys.Skip(i * batchSize).Take(batchSize);
                var t = partitionKeys.Skip(i * batchSize);
                tasks.Add(GetInParallel(currentIds.ToList(), config));
            }

            return (await Task.WhenAll(tasks)).SelectMany(u => u).ToList();
        }

        private async Task<List<T>> GetInParallel(List<string> partitionKeys, DynamoDBOperationConfig config)
        {
            var tasks = new List<Task<List<T>>>();

            foreach (var pk in partitionKeys)
            {

                tasks.Add(GeEntitiesAsync(pk, config));
            }

            return (await Task.WhenAll(tasks)).SelectMany(u => u).ToList();
        }

        private async Task<List<T>> GeEntitiesAsync(string partitionKey, DynamoDBOperationConfig config)
        {
            var task = await Context.QueryAsync<T>(partitionKey, config).GetRemainingAsync();

            return task;
        }

        private async Task<List<T>> GetFilterInParallelInWithBatches(List<string> partitionKeys, string partitionKeyName, int batchSize, QueryOperationConfig queryConfig, QueryFilter rowKeyFilter, DynamoDBOperationConfig config)
        {
            var tasks = new List<Task<List<T>>>();
            int numberOfBatches = (int)Math.Ceiling((double)partitionKeys.Count() / batchSize);
            var map = new Dictionary<int, List<string>>();

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentIds = partitionKeys.Skip(i * batchSize).Take(batchSize);

                map.Add(i, currentIds.ToList());
            }

            foreach (var item in map)
            {
                tasks.Add(Task.Run(() => GetFilterParallel(item.Value, partitionKeyName, queryConfig, rowKeyFilter, config)));
            }

            return (await Task.WhenAll(tasks)).SelectMany(u => u).ToList();
        }

        private async Task<List<T>> GetFilterParallel(List<string> partitionKeys, string partitionKeyName, QueryOperationConfig queryConfig, QueryFilter rowKeyFilter, DynamoDBOperationConfig config)
        {
            var tasks = new List<Task<List<T>>>();

            foreach (var pk in partitionKeys)
            {
                tasks.Add(Task.Run(() => ExecuteFilterAsync(pk, partitionKeyName, queryConfig, rowKeyFilter, config)));
            }

            return (await Task.WhenAll(tasks)).SelectMany(u => u).ToList();
        }

        private async Task<List<T>> ExecuteFilterAsync(string partitionKey, string partitionKeyName, QueryOperationConfig queryConfig, QueryFilter rowKeyFilter, DynamoDBOperationConfig config = null)
        {
            var currentQueryConfig = JsonConvert.DeserializeObject<QueryOperationConfig>(JsonConvert.SerializeObject(queryConfig));


            if (rowKeyFilter != null)
            {
                currentQueryConfig.Filter = rowKeyFilter;
            }
            else
            {
                currentQueryConfig.Filter = new QueryFilter();
            }

            if (queryConfig.Filter != null)
            {
                foreach (var f in queryConfig.Filter.ToConditions())
                {
                    var condition = f.Value;
                    QueryOperator queryOperator = QueryOperator.Equal;
                    switch (condition.ComparisonOperator.Value)
                    {
                        case "EQ":
                            queryOperator = QueryOperator.Equal;
                            break;
                        case "BETWEEN":
                            queryOperator = QueryOperator.Between;
                            break;
                        case "CONTAINS":
                            //queryOperator = QueryOperator.CON;
                            break;
                        case "GE":
                            queryOperator = QueryOperator.GreaterThanOrEqual;
                            break;
                        case "GT":
                            queryOperator = QueryOperator.GreaterThan;
                            break;
                        case "IN":
                            queryOperator = QueryOperator.Equal;
                            break;
                        case "LE":
                            queryOperator = QueryOperator.LessThanOrEqual;
                            break;
                        case "NE":
                            //queryOperator = QueryOperator.L;
                            break;
                        case "LT":
                            queryOperator = QueryOperator.LessThan;
                            break;
                        case "NOT_CONTAINS":
                            //queryOperator = QueryOperator.Between;
                            break;
                        case "BEGINS_WITH":
                            queryOperator = QueryOperator.BeginsWith;
                            break;
                        case "NOT_NULL":
                            //queryOperator = QueryOperator.Between;
                            break;
                        case "NULL":
                            //queryOperator = QueryOperator.Between;
                            break;
                    };


                    currentQueryConfig.Filter.AddCondition(f.Key, queryOperator, condition.AttributeValueList);
                }
            }


            currentQueryConfig.Filter.AddCondition(partitionKeyName, QueryOperator.Equal, new List<AttributeValue>() { new AttributeValue(partitionKey) });

            var task = await Context.FromQueryAsync<T>(currentQueryConfig, config).GetRemainingAsync();

            return task;
        }

        public T Get(string partitionKey, string rowKey, DynamoDBOperationConfig config = null)
        {
            try
            {
                var result = string.IsNullOrEmpty(rowKey) ? default(T) : Context.LoadAsync<T>(GetPKWithAttributes(partitionKey, rowKey), rowKey, config).Result;

                return result;
            }
            catch (Exception e)
            {
            }

            return default(T);
        }

        public List<T> GetInBatch(List<string> partionKeys, DynamoDBOperationConfig config = null)
        {
            var result = new List<T>();

            if (partionKeys.Count > 0)
            {
                var batchGet = Context.CreateBatchGet<T>(config);

                foreach (var pk in partionKeys)
                {
                    batchGet.AddKey(pk);
                }

                batchGet.ExecuteAsync();

                result = batchGet.Results;
            }

            return result;
        }

        public Task GetInBatchAsync(List<string> partionKeys, List<string> sortKeys, DynamoDBOperationConfig config = null)
        {
            if (partionKeys.Count > 0 && sortKeys.Count > 0)
            {
                var batchGet = Context.CreateBatchGet<T>(config);

                batchGet.AddKey(partionKeys, sortKeys);

                return batchGet.ExecuteAsync();
            }

            return GetDefaultTask("OK");
        }

        private string ConvertPKWithAttributes(T entity)
        {
            var newPk = entity.PartitionKey;
            try
            {
                var type = typeof(T);
                var uniformPartitioning = type.GetCustomAttributes(typeof(UniformPartitioning), false).ToList();
                var timePKPeriodicity = type.GetCustomAttributes(typeof(TimePKPeriodicity), false).ToList();

                foreach (TimePKPeriodicity property in timePKPeriodicity)
                {
                    newPk = property.GetTimePartionKey(entity.PartitionKey);
                }
                foreach (UniformPartitioning property in uniformPartitioning)
                {
                    newPk = property.GetNewPartitionKey(entity.PartitionKey, entity.RowKey);
                }
            }
            catch (Exception)
            {
                newPk = entity.PartitionKey;
            }

            return newPk;
        }

        private string GetPKWithAttributes(string partitionKey, string rowKey)
        {
            var newPk = partitionKey;

            try
            {
                var type = typeof(T);
                var uniformPartitioning = type.GetCustomAttributes(typeof(UniformPartitioning), false).ToList();
                var timePKPeriodicity = type.GetCustomAttributes(typeof(TimePKPeriodicity), false).ToList();

                foreach (TimePKPeriodicity property in timePKPeriodicity)
                {
                    newPk = property.GetTimePartionKey(partitionKey);
                }
                foreach (UniformPartitioning property in uniformPartitioning)
                {
                    newPk = property.GetNewPartitionKey(partitionKey, rowKey);
                }
            }
            catch (Exception)
            {
                newPk = partitionKey;
            }

            return newPk;
        }

        private List<string> GetPartitions(string partitionKey)
        {
            var partitionKeys = new List<string>();

            try
            {
                var type = typeof(T);
                var uniformPartitioning = type.GetCustomAttributes(typeof(UniformPartitioning), false).ToList();

                foreach (UniformPartitioning property in uniformPartitioning)
                {
                    partitionKeys = property.GetPartitions(partitionKey);
                }
            }
            catch (Exception)
            {
                partitionKeys = new List<string>();
                partitionKeys.Add(partitionKey);
            }

            return partitionKeys;
        }

        private string GetTableName()
        {
            var tableName = string.Empty;

            try
            {
                var type = typeof(T);
                var dynamoDBTable = type.GetCustomAttributes(typeof(Amazon.DynamoDBv2.DataModel.DynamoDBTableAttribute), false).ToList();

                foreach (DynamoDBTableAttribute property in dynamoDBTable)
                {
                    tableName = property.TableName;
                }
            }
            catch (Exception)
            {
            }

            return tableName;
        }

        public Task SaveAsync(T entity)
        {
            var result = Context.SaveAsync(entity);

            return result;
        }

        public Task Save(T entity, List<string> duplicatePK = null)
        {
            try
            {
                if (duplicatePK == null)
                {
                    entity.PartitionKey = ConvertPKWithAttributes(entity);

                    var result = SaveAsync(entity);
                    result.Wait();

                    return result;
                }
                else
                {
                    var entities = new List<T>();

                    entities.Add(entity);

                    var resultBatch = SaveInBatch(entities, duplicatePK);

                    return resultBatch;
                }
            }
            catch (Exception e)
            {
            }

            return GetDefaultTask("BAD");
        }

        public Task SaveInBatchAsync(List<T> entities, List<string> duplicatePK = null)
        {
            if (entities.Count > 0)
            {
                if (duplicatePK != null)
                {
                    var newEntities = new List<T>();

                    foreach (var newPk in duplicatePK)
                    {
                        foreach (var entity in entities.ToList())
                        {
                            var newEntity = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(entity));

                            newEntity.PartitionKey = newPk;
                            newEntities.Add(newEntity);
                        }
                    }

                    entities.AddRange(newEntities);
                }

                foreach (var entity in entities)
                {
                    entity.PartitionKey = ConvertPKWithAttributes(entity);
                }

                var batchWriter = Context.CreateBatchWrite<T>();

                batchWriter.AddPutItems(entities);
                return batchWriter.ExecuteAsync();
            }

            return GetDefaultTask("OK");
        }

        public Task SaveInBatch(List<T> entities, List<string> duplicatePK = null)
        {
            try
            {
                var result = SaveInBatchAsync(entities, duplicatePK);

                result.Wait();

                return result;
            }
            catch (Exception e)
            {
            }

            return GetDefaultTask("BAD");
        }

        public Task Replace(string newPartitionKey, string newRowKey, T entity, List<string> oldDuplicatePK = null, List<string> duplicatePK = null)
        {
            try
            {
                List<TransactWriteItem> actions = new List<TransactWriteItem>();
                var keys = new Dictionary<string, AttributeValue>();

                keys.Add("PartitionKey", new AttributeValue(entity.PartitionKey));
                keys.Add("RowKey", new AttributeValue(entity.RowKey));
                actions.Add(new TransactWriteItem()
                {
                    Delete = new Delete()
                    {
                        Key = keys,
                        TableName = GetTableName()
                    }

                });

                if (oldDuplicatePK != null)
                {
                    foreach (var newPk in oldDuplicatePK)
                    {
                        var keysDuplicates = new Dictionary<string, AttributeValue>();

                        keysDuplicates.Add("PartitionKey", new AttributeValue(GetPKWithAttributes(newPk, entity.RowKey)));
                        keysDuplicates.Add("RowKey", new AttributeValue(entity.RowKey));
                        actions.Add(new TransactWriteItem()
                        {
                            Delete = new Delete()
                            {
                                Key = keysDuplicates,
                                TableName = GetTableName()
                            }
                        });
                    }
                }

                TransactWriteItemsRequest placeOrderTransaction = new TransactWriteItemsRequest();
                placeOrderTransaction.TransactItems = actions;
                placeOrderTransaction.ReturnConsumedCapacity = (ReturnConsumedCapacity.TOTAL);

                // Run the transaction and process the result.
                try
                {
                    var iteme = Client.TransactWriteItemsAsync(placeOrderTransaction).Result;
                    Console.WriteLine("Transaction Successful");

                    entity.PartitionKey = newPartitionKey;
                    entity.RowKey = newRowKey;

                    var response = Save(entity, duplicatePK);

                    return response;
                }
                catch (ResourceNotFoundException rnf)
                {
                    Console.WriteLine("One of the table involved in the transaction is not found" + rnf.Message);
                }
                catch (InternalServerErrorException ise)
                {
                    Console.WriteLine("Internal Server Error" + ise.Message);
                }
                catch (TransactionCanceledException tce)
                {
                    Console.WriteLine("Transaction Canceled " + tce.Message);
                }
            }
            catch (Exception e)
            {
            }

            return GetDefaultTask("BAD");
        }

        public Task DeleteAsync(string partitionKey, string rowKey)
        {
            var result = string.IsNullOrEmpty(rowKey) ? Context.DeleteAsync<T>(partitionKey) : Context.DeleteAsync<T>(partitionKey, rowKey);

            return result;
        }

        public Task Delete(string partitionKey, string rowKey)
        {
            try
            {
                var result = DeleteAsync(partitionKey, rowKey);

                result.Wait();

                return result;
            }
            catch (Exception)
            {
            }

            return GetDefaultTask("BAD");
        }

        public Task DeleteInBatchAsync(List<T> entities)
        {
            if (entities.Count > 0)
            {
                var batchWriter = Context.CreateBatchWrite<T>();

                batchWriter.AddDeleteItems(entities);
                return batchWriter.ExecuteAsync();
            }

            return GetDefaultTask("OK");
        }

        public Task DeleteInBatch(List<T> entities)
        {
            try
            {
                var result = DeleteInBatchAsync(entities);

                result.Wait();

                return result;
            }
            catch (Exception)
            {
            }

            return GetDefaultTask("BAD");
        }

        private RegionEndpoint GetRegion(string region = "")
        {
            switch (region)
            {

                case "us-east-1":
                    return RegionEndpoint.USEast1;
                case "us-west-1":
                    return RegionEndpoint.USWest1;
                case "us-west-2":
                    return RegionEndpoint.USWest2;
                case "af-south-1":
                    return RegionEndpoint.AFSouth1;
                case "ap-east-1":
                    return RegionEndpoint.APEast1;
                case "ap-southeast-3":
                    return RegionEndpoint.APSoutheast3;
                case "ap-south-1":
                    return RegionEndpoint.APSouth1;
                case "ap-northeast-3":
                    return RegionEndpoint.APNortheast3;
                case "ap-northeast-2":
                    return RegionEndpoint.APNortheast2;
                case "ap-southeast-1":
                    return RegionEndpoint.APSoutheast1;
                case "ap-southeast-2":
                    return RegionEndpoint.APSoutheast2;
                case "ap-northeast-1":
                    return RegionEndpoint.APNortheast1;
                case "ca-central-1":
                    return RegionEndpoint.CACentral1;
                case "eu-central-1":
                    return RegionEndpoint.EUCentral1;
                case "eu-west-1":
                    return RegionEndpoint.EUWest1;
                case "eu-west-2":
                    return RegionEndpoint.EUWest2;
                case "eu-south-1":
                    return RegionEndpoint.EUSouth1;
                case "eu-west-3":
                    return RegionEndpoint.EUWest3;
                case "eu-north-1":
                    return RegionEndpoint.EUNorth1;
                case "me-south-1":
                    return RegionEndpoint.MESouth1;
                case "sa-east-1":
                    return RegionEndpoint.SAEast1;
                case "us-east-2":
                    return RegionEndpoint.USEast2;
                default:
                    return RegionEndpoint.USEast1;
            }
        }

        private Task GetDefaultTask(string status)
        {
            switch (status)
            {
                case "OK":
                    return Task.FromResult(new
                    {
                        IsCompletedSuccessfully = true
                    });
                case "BAD":
                    return Task.FromResult(new
                    {
                        IsCompletedSuccessfully = false
                    });
                default:
                    return Task.FromResult(new
                    {
                        IsCompletedSuccessfully = false
                    });
            }
        }
    }
}
