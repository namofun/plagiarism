using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Xylab.DataAccess.Cosmos
{
    internal class QueryProvider
    {
        private static readonly EventId EventId = new(10060, "CosmosDbQuery");
        private readonly ITelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly Container _container;

        public QueryProvider(
            ITelemetryClient telemetryClient,
            ILogger logger,
            Container container,
            CosmosOptions options)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
            _container = container;
            Options = options;
        }

        public CosmosOptions Options { get; }

        private static void InjectSuccess<T>(IDependencyTracker dependencyTracker, Response<T> response)
        {
            dependencyTracker.Success = true;
            dependencyTracker.ResultCode = ((int)response.StatusCode).ToString();
            dependencyTracker.Metrics["RequestCharge"] = response.RequestCharge;
            dependencyTracker.Properties["ActivityId"] = response.ActivityId;

            if (response is FeedResponse<T> feedResponse)
            {
                dependencyTracker.Metrics["ItemCount"] = feedResponse.Count;
            }
        }

        private static void InjectFailure(IDependencyTracker dependencyTracker, CosmosException cosmosException)
        {
            dependencyTracker.Success = false;
            dependencyTracker.ResultCode = ((int)cosmosException.StatusCode).ToString();
            dependencyTracker.Metrics["RequestCharge"] = cosmosException.RequestCharge;
            dependencyTracker.Properties["ActivityId"] = cosmosException.ActivityId;
        }

        private async Task<FeedResponse<TEntity>> Feed<TEntity>(
            FeedIterator<TEntity> feedIterator,
            CancellationToken cancellationToken)
        {
            using IDependencyTracker dependencyTracker =
                _telemetryClient.StartOperation(
                    "Azure DocumentDB",
                    _container.Database.Client.Endpoint.Host,
                    $"FEED {_container.Database.Id}/{_container.Id}");

            try
            {
                FeedResponse<TEntity> response = await feedIterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                InjectSuccess(dependencyTracker, response);
                return response;
            }
            catch (CosmosException ex)
            {
                InjectFailure(dependencyTracker, ex);
                throw;
            }
            catch
            {
                dependencyTracker.Success = false;
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(dependencyTracker);
            }
        }

        public async Task<TResult> Query<TResult, TResponse>(
            string operationName,
            string descriptionalQueryText,
            Func<Container, Task<TResponse>> queryExecutor)
            where TResponse : Response<TResult>
        {
            Stopwatch timer = Stopwatch.StartNew();
            using IDependencyTracker dependencyTracker =
                _telemetryClient.StartOperation(
                    "Azure DocumentDB",
                    _container.Database.Client.Endpoint.Host,
                    $"{operationName} {_container.Database.Id}/{_container.Id}");
            dependencyTracker.Data = descriptionalQueryText;

            try
            {
                TResponse response = await queryExecutor(_container).ConfigureAwait(false);

                timer.Stop();
                InjectSuccess(dependencyTracker, response);
                _logger.LogInformation(EventId,
                    "Executed from [{ContainerName}] within {ElapsedTime}ms.\r\n{QueryText}",
                    _container.Id, timer.ElapsedMilliseconds, descriptionalQueryText);
                return response;
            }
            catch (CosmosException ex)
            {
                timer.Stop();
                InjectFailure(dependencyTracker, ex);
                _logger.LogError(EventId, ex,
                    "Failed to execute from [{ContainerName}] within {ElapsedTime}ms.\r\n{QueryText}",
                    _container.Id, timer.ElapsedMilliseconds, descriptionalQueryText);
                throw;
            }
            catch
            {
                dependencyTracker.Success = false;
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(dependencyTracker);
            }
        }

        public async Task<List<TEntity>> Query<TEntity>(
            QueryDefinition sql,
            PartitionKey? partitionKey,
            CancellationToken cancellationToken)
        {
            using IDependencyTracker dependencyTracker =
                _telemetryClient.StartOperation(
                    "Azure DocumentDB",
                    _container.Database.Client.Endpoint.Host,
                    $"QUERY {_container.Database.Id}/{_container.Id}");
            dependencyTracker.Data = sql.QueryText;

            Stopwatch timer = Stopwatch.StartNew();
            List<TEntity> result = new();
            QueryRequestOptions options = new() { PartitionKey = partitionKey };

            try
            {
                using FeedIterator<TEntity> iterator = _container.GetItemQueryIterator<TEntity>(sql, requestOptions: options);
                while (iterator.HasMoreResults)
                {
                    foreach (TEntity item in await Feed(iterator, cancellationToken))
                    {
                        result.Add(item);
                    }
                }

                timer.Stop();
                _logger.LogInformation(EventId,
                    "Queried from [{ContainerName}] within {ElapsedTime}ms, {Count} results.\r\n{QueryText}",
                    _container.Id + (partitionKey == null ? "" : "(pk=" + partitionKey + ")"), timer.ElapsedMilliseconds, result.Count, sql.QueryText);
                return result;
            }
            catch (CosmosException ex)
            {
                timer.Stop();
                _logger.LogError(EventId, ex,
                    "Failed to query from [{ContainerName}] within {ElapsedTime}ms.\r\n{QueryText}",
                    _container.Id + (partitionKey == null ? "" : "(pk=" + partitionKey + ")"), timer.ElapsedMilliseconds, sql.QueryText);
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(dependencyTracker);
            }
        }

        public async Task<TransactionalBatchResponse> Query(
            TransactionalBatch transactionalBatch,
            TransactionalBatchRequestOptions? options,
            bool transactional,
            string queryDescription,
            CancellationToken cancellationToken)
        {
            options ??= new();
            if (!transactional)
            {
                options.AddRequestHeaders = c =>
                {
                    c.Set("x-ms-cosmos-batch-atomic", bool.FalseString);
                    c.Add("x-ms-cosmos-batch-continue-on-error", bool.TrueString);
                };
            }

            Stopwatch timer = Stopwatch.StartNew();
            using IDependencyTracker dependencyTracker =
                _telemetryClient.StartOperation(
                    "Azure DocumentDB",
                    _container.Database.Client.Endpoint.Host,
                    $"BATCH {_container.Database.Id}/{_container.Id}");
            dependencyTracker.Data = queryDescription;

            try
            {
                TransactionalBatchResponse response = await transactionalBatch.ExecuteAsync(options, cancellationToken).ConfigureAwait(false);

                timer.Stop();
                dependencyTracker.Success = true;
                dependencyTracker.ResultCode = ((int)response.StatusCode).ToString();
                dependencyTracker.Metrics["RequestCharge"] = response.RequestCharge;
                dependencyTracker.Metrics["ItemCount"] = response.Count;
                dependencyTracker.Properties["ActivityId"] = response.ActivityId;
                _logger.LogInformation(EventId,
                    "Executed from [{ContainerName}] within {ElapsedTime}ms.\r\n{QueryText}",
                    _container.Id, timer.ElapsedMilliseconds, dependencyTracker.Data);

                return response;
            }
            catch (CosmosException ex)
            {
                timer.Stop();
                InjectFailure(dependencyTracker, ex);
                _logger.LogError(EventId, ex,
                    "Failed to execute from [{ContainerName}] within {ElapsedTime}ms.\r\n{QueryText}",
                    _container.Id, timer.ElapsedMilliseconds, dependencyTracker.Data);
                throw;
            }
            catch
            {
                dependencyTracker.Success = false;
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(dependencyTracker);
            }
        }

        public Batch<TEntity> CreateBatch<TEntity>(PartitionKey partitionKey)
        {
            return new(_container.CreateTransactionalBatch(partitionKey), this, partitionKey);
        }
    }
}
