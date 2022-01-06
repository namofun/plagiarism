using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Plag.Backend.QueryProvider
{
    public abstract class CosmosQueryProvider<TEntity>
    {
        protected readonly Container _container;
        protected readonly PartitionKey _partitionKey;
        protected readonly ILogger _logger;

        protected CosmosQueryProvider(Container container, PartitionKey partitionKey, ILogger logger)
        {
            _container = container;
            _partitionKey = partitionKey;
            _logger = logger;
        }

        protected string ParseProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberAccess
                || memberAccess.Expression != propertySelector.Parameters[0])
            {
                throw new ArgumentException("Invalid property selector, must be direct member access.");
            }

            string propertyName = memberAccess.Member
                .GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                ?.Name
                ?? throw new ArgumentException("Invalid property, must be marked with [JsonPropertyName]");

            return propertyName;
        }
    }
}
