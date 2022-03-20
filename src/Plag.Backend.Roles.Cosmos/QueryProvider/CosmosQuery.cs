using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    internal class CosmosQuery
    {
        private static readonly Action<Headers> MakeBatchNotTranscation = c =>
        {
            c.Set("x-ms-cosmos-batch-atomic", bool.FalseString);
            c.Add("x-ms-cosmos-batch-continue-on-error", bool.TrueString);
        };

        private static readonly Action<TransactionalBatchRequestOptions, Action<Headers>> AddRequestHeadersPropertySetter
            = typeof(TransactionalBatchRequestOptions)
                .GetProperty(
                    "AddRequestHeaders",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .SetMethod!
                .CreateDelegate<Action<TransactionalBatchRequestOptions, Action<Headers>>>();

        private static readonly EventId EventId = new(10060, "CosmosDbQuery");
        private readonly ITelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly Container _container;

        public CosmosQuery(
            ITelemetryClient telemetryClient,
            ILogger logger,
            Container container)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
            _container = container;
        }

        private static void InjectSuccess<T>(IDependencyTracker dependencyTracker, Response<T> response)
        {
            dependencyTracker.Success = true;
            dependencyTracker.ResultCode = ((int)response.StatusCode).ToString();
            dependencyTracker.Metrics["RequestCharge"] = response.RequestCharge;
            dependencyTracker.Properties["ActivityId"] = response.ActivityId;
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
            TransactionalBatchRequestOptions options,
            bool transactional,
            CancellationToken cancellationToken)
        {
            options ??= new();
            if (!transactional) AddRequestHeadersPropertySetter(options, MakeBatchNotTranscation);

            Stopwatch timer = Stopwatch.StartNew();
            using IDependencyTracker dependencyTracker =
                _telemetryClient.StartOperation(
                    "Azure DocumentDB",
                    _container.Database.Client.Endpoint.Host,
                    $"BATCH {_container.Database.Id}/{_container.Id}");
            dependencyTracker.Data = transactional ? "TRANSACTION BATCH" : "NON-TRANSACTION BATCH";

            try
            {
                TransactionalBatchResponse response = await transactionalBatch.ExecuteAsync(options, cancellationToken).ConfigureAwait(false);

                timer.Stop();
                dependencyTracker.Success = true;
                dependencyTracker.ResultCode = ((int)response.StatusCode).ToString();
                dependencyTracker.Metrics["RequestCharge"] = response.RequestCharge;
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

        public CosmosBatch<TEntity> CreateBatch<TEntity>(PartitionKey partitionKey)
        {
            return new(_container.CreateTransactionalBatch(partitionKey), this);
        }
    }
}
