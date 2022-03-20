#nullable enable
using Microsoft.Azure.Cosmos;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    internal static class CosmosInternalsExtension
    {
        public static Func<T1, T2, Task>? AsAsync<T1, T2>(this Action<T1, T2>? action)
        {
            return action == null ? null : (t1, t2) => { action(t1, t2); return Task.CompletedTask; };
        }

        public static Func<T1, T2, T3, Task>? AsAsync<T1, T2, T3>(this Action<T1, T2, T3>? action)
        {
            return action == null ? null : (t1, t2, t3) => { action(t1, t2, t3); return Task.CompletedTask; };
        }

        public static async Task ExecuteWithRetryAsync<T>(this CosmosPatch<T> patch)
        {
            for (int tries = 0; ; tries++)
            {
                try
                {
                    await patch.ExecuteAsync();
                    return;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (ex.RetryAfter.HasValue)
                    {
                        await Task.Delay(ex.RetryAfter.Value);
                    }
                    else if (tries >= 2)
                    {
                        throw;
                    }
                }
            }
        }

        public static string ParseProperty<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (propertySelector.Body is not MemberExpression memberAccess
                || memberAccess.Expression != propertySelector.Parameters[0])
            {
                throw new ArgumentException("Invalid property selector, must be direct member access or dictionary access.");
            }

            if (EntityJsonContractResolver.SpecialConfiguration.TryGetValue(
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
