#nullable enable

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    internal static class CosmosInternalsExtension
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

        public static async Task CreateStoredProcedureIfNotExistsAsync(
            this Scripts scripts,
            StoredProcedureProperties storedProcedureProperties,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await scripts.CreateStoredProcedureAsync(
                    storedProcedureProperties,
                    requestOptions,
                    cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
            }
        }

        public static Func<T1, T2, Task>? AsAsync<T1, T2>(this Action<T1, T2>? action)
        {
            return action == null ? null : (t1, t2) => { action(t1, t2); return Task.CompletedTask; };
        }

        public static Func<T1, T2, T3, Task>? AsAsync<T1, T2, T3>(this Action<T1, T2, T3>? action)
        {
            return action == null ? null : (t1, t2, t3) => { action(t1, t2, t3); return Task.CompletedTask; };
        }

        public static Task<TransactionalBatchResponse> ExecuteNonTransactionalAsync(this TransactionalBatch batch, CancellationToken cancellationToken = default)
        {
            TransactionalBatchRequestOptions options = new();
            AddRequestHeadersPropertySetter(options, MakeBatchNotTranscation);
            return batch.ExecuteAsync(options, cancellationToken);
        }

        public static string ParseProperty<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberAccess
                || memberAccess.Expression != propertySelector.Parameters[0])
            {
                throw new ArgumentException("Invalid property selector, must be direct member access.");
            }

            if (!EntityJsonContractResolver.SpecialConfiguration.TryGetValue(
                memberAccess.Member,
                out string? propertyName))
            {
                propertyName = memberAccess.Member
                    .GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                    ?.Name
                    ?? throw new ArgumentException("Invalid property, must be marked with [JsonPropertyName]");
            }

            return propertyName;
        }
    }
}
