using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xylab.DataAccess.Cosmos;
using Xylab.PlagiarismDetect.Backend.Entities;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    public class CosmosConnection : ConnectionBase, ICosmosConnection
    {
        internal const string QueryServiceGraph = nameof(QueryServiceGraph);

        public Container<SetEntity> Sets { get; }
        public Container<SubmissionEntity> Submissions { get; }
        public Container<ReportEntity> Reports { get; }
        public Container<MetadataEntity> Metadata { get; }

        public CosmosConnection(
            IOptions<PlagBackendCosmosOptions> options,
            ILogger<CosmosConnection> logger,
            ITelemetryClient telemetryClient)
            : base(
                  options.Value.ConnectionString,
                  options.Value.DatabaseName,
                  CreateOptions(),
                  logger,
                  telemetryClient)
        {
            Sets = Container<SetEntity>(nameof(Metadata));
            Submissions = Container<SubmissionEntity>(nameof(Submissions));
            Reports = Container<ReportEntity>(nameof(Reports));
            Metadata = Container<MetadataEntity>(nameof(Metadata));
        }

        private static CosmosOptions CreateOptions() => new()
        {
            PartitionKeyMapping =
            {
                [nameof(Metadata)] = "/type",
                [nameof(Submissions)] = "/setid",
                [nameof(Reports)] = "/setid",
            },

            CustomIndexingPolicy =
            {
                [nameof(Submissions)] = ip =>
                {
                    ip.ExcludedPaths.Add(new() { Path = "/error/?" });
                    ip.ExcludedPaths.Add(new() { Path = "/tokens/?" });
                    ip.ExcludedPaths.Add(new() { Path = "/files/?" });
                },

                [nameof(Reports)] = ip =>
                {
                    ip.ExcludedPaths.Add(new() { Path = "/matches/?" });
                    ip.ExcludedPaths.Add(new() { Path = "/biggest_match/?" });
                    ip.ExcludedPaths.Add(new() { Path = "/tokens_matched/?" });
                },
            },

            StoredProcedures =
            {
                [QueryServiceGraph] = (nameof(Metadata), GetStoredProcedureCodeAsync(QueryServiceGraph)),
            },

            CustomPropertyMapping =
            {
                { typeof(PlagiarismSet).GetProperty(nameof(PlagiarismSet.Id)), "id" },
                { typeof(SetEntity).GetProperty(nameof(SetEntity.Id)), "id" },
                { typeof(Submission).GetProperty(nameof(Submission.ExternalId)), "id" },
                { typeof(SubmissionEntity).GetProperty(nameof(SubmissionEntity.ExternalId)), "id" },
                { typeof(Report).GetProperty(nameof(Report.Id)), "id" },
                { typeof(ReportEntity).GetProperty(nameof(ReportEntity.Id)), "id" },
                { typeof(LanguageInfo).GetProperty(nameof(LanguageInfo.ShortName)), "id" },
            },

            DeclaredTypes =
            {
                typeof(PlagiarismSet),
                typeof(SetEntity),
                typeof(ServiceVertex),
                typeof(ServiceEdge),
                typeof(Submission),
                typeof(SubmissionEntity),
                typeof(Report),
                typeof(Comparison),
                typeof(ReportEntity),
                typeof(LanguageInfo),
            },
        };

        private static string GetStoredProcedureCodeAsync(string name)
        {
            string sproc = $"Xylab.PlagiarismDetect.Backend.QueryProvider.{name}.js";
            using Stream stream = typeof(CosmosConnection).Assembly.GetManifestResourceStream(sproc)
                ?? throw new InvalidDataException();

            using StreamReader sr = new(stream);
            return sr.ReadToEnd();
        }
    }

    public static class CosmosContainerStoredProcedureExtensions
    {
        public static Task<List<ServiceVertex>> QueryServiceGraphAsync(
            this Container<MetadataEntity> container,
            SetGuid setId,
            string language,
            int inclusiveCategory,
            int exclusiveCategory)
        {
            return container.ExecuteStoredProcedureAsync<List<ServiceVertex>>(
                CosmosConnection.QueryServiceGraph,
                new PartitionKey(MetadataEntity.ServiceGraphTypeKey),
                new object[] { setId.ToString(), language, inclusiveCategory, exclusiveCategory },
                new() { EnableScriptLogging = false });
        }
    }
}
