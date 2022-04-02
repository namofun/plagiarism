global using Microsoft.Azure.Cosmos;
using System.Linq.Expressions;
using System.Reflection;

namespace Xylab.DataAccess.Cosmos
{
    public class CosmosOptions
    {
        public Dictionary<MemberInfo, string> CustomPropertyMapping { get; } = new();

        public HashSet<Type> DeclaredTypes { get; } = new();

        public Dictionary<string, string> PartitionKeyMapping { get; } = new();

        public Dictionary<string, Action<IndexingPolicy>> CustomIndexingPolicy { get; } = new();

        public Dictionary<string, (string CollectionName, string Code)> StoredProcedures { get; } = new();

        public Action<CosmosClientOptions>? ConfigureClientOptions { get; set; }

        internal string ParseProperty<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberAccess
                || memberAccess.Expression != propertySelector.Parameters[0])
            {
                throw new ArgumentException("Invalid property selector, must be direct member access or dictionary access.");
            }

            if (CustomPropertyMapping.TryGetValue(
                memberAccess.Member,
                out string? propertyName))
            {
                return propertyName;
            }
            else if (memberAccess.Member
                .GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                ?.Name is string systemJsonPropertyName)
            {
                return systemJsonPropertyName;
            }
            else if (memberAccess.Member
                .GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>()
                ?.PropertyName is string newtonsoftJsonPropertyName)
            {
                return newtonsoftJsonPropertyName;
            }
            else
            {
                throw new ArgumentException("Invalid property, must be marked with [JsonPropertyName]");
            }
        }
    }
}
