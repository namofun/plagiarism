using Newtonsoft.Json;
using Plag.Backend.Connectors;
using Plag.Backend.Models;
using System;
using System.Collections.Generic;

namespace Plag.Backend
{
    /// <summary>
    /// Options class for Azure Cosmos DB in plagiarism detection service.
    /// </summary>
    public class PdsCosmosOptions
    {
        /// <summary>
        /// Azure Cosmos DB account connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Azure Cosmos DB database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Seeds of languages, or not needed to be initialized if null
        /// </summary>
        public List<LanguageInfo> LanguageSeeds { get; set; }

        /// <summary>
        /// Json serializer options
        /// </summary>
        public JsonSerializerSettings Serialization { get; set; } = new()
        {
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.None,
            ContractResolver = new EntityJsonContractResolver(),
        };

        /// <summary>
        /// Validates the configuration completeness.
        /// </summary>
        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException("No connection string configured for PDS cosmos.");
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                throw new InvalidOperationException("No database name configured for PDS cosmos.");
            }
        }
    }
}
