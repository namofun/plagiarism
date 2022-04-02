using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend
{
    /// <summary>
    /// Options class for Azure Cosmos DB in plagiarism detection service.
    /// </summary>
    public class PlagBackendCosmosOptions
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
        /// Configure the cosmos connection with the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The configured options.</returns>
        public PlagBackendCosmosOptions WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// Configure the cosmos connection with the database name.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns>The configured options.</returns>
        public PlagBackendCosmosOptions WithDatabaseName(string databaseName)
        {
            DatabaseName = databaseName;
            return this;
        }

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
