using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
{
    internal class HybridCosmosSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        private readonly JsonSerializerSettings SerializerSettings;

        public HybridCosmosSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            SerializerSettings = jsonSerializerSettings;
        }

        public override T FromStream<T>(Stream stream)
        {
            try
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    throw new InvalidOperationException("Stream is not supported here.");
                }

                using StreamReader reader = new(stream);
                using JsonTextReader reader2 = new(reader);
                return GetSerializer().Deserialize<T>(reader2);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream memoryStream = new();
            using (StreamWriter streamWriter = new(memoryStream, DefaultEncoding, 1024, leaveOpen: true))
            using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.Formatting = Formatting.None;
                GetSerializer().Serialize(jsonWriter, input);
                jsonWriter.Flush();
                streamWriter.Flush();
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private JsonSerializer GetSerializer()
        {
            return JsonSerializer.Create(SerializerSettings);
        }
    }
}
