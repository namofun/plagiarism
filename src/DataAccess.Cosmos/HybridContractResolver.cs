using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using SystemJsonConverterAttribute = System.Text.Json.Serialization.JsonConverterAttribute;
using SystemJsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SystemJsonIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition;
using SystemJsonPropertyNameAttribute = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using SystemJsonStringEnumConverter = System.Text.Json.Serialization.JsonStringEnumConverter;

namespace Xylab.DataAccess.Cosmos
{
    internal class HybridContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly CosmosOptions _options;

        public HybridContractResolver(CosmosOptions options)
        {
            _options = options;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType.GetCustomAttribute<SystemJsonConverterAttribute>()
                is SystemJsonConverterAttribute systemConverter
                && systemConverter.ConverterType == typeof(SystemJsonStringEnumConverter))
            {
                contract.Converter = new StringEnumConverter();
            }

            return contract;
        }

        protected override JsonProperty? CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property == null) return null;

            if (_options.DeclaredTypes.Contains(property.DeclaringType))
            {
                if (member.GetCustomAttribute<SystemJsonIgnoreAttribute>()
                    is SystemJsonIgnoreAttribute systemIgnore)
                {
                    switch (systemIgnore.Condition)
                    {
                        case SystemJsonIgnoreCondition.Always:
                            return null;

                        case SystemJsonIgnoreCondition.WhenWritingDefault:
                        case SystemJsonIgnoreCondition.WhenWritingNull:
                            property.NullValueHandling = NullValueHandling.Ignore;
                            break;
                    }
                }

                if (member.GetCustomAttribute<SystemJsonPropertyNameAttribute>()
                    is SystemJsonPropertyNameAttribute nameAttribute)
                {
                    property.PropertyName = nameAttribute.Name;
                }

                if (_options.CustomPropertyMapping.TryGetValue(member, out string? altName))
                {
                    property.PropertyName = altName;
                }
            }

            return property;
        }
    }
}
