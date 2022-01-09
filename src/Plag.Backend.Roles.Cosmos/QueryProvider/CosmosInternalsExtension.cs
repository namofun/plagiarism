using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.QueryProvider
{
    internal static class CosmosInternalsExtension
    {
        public static async Task<StoredProcedureResponse> CreateStoredProcedureIfNotExistsAsync(
            this Scripts scripts,
            StoredProcedureProperties storedProcedureProperties,
            RequestOptions requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await scripts.CreateStoredProcedureAsync(
                    storedProcedureProperties,
                    requestOptions,
                    cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return null;
            }
        }
    }
}
