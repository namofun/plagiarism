using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plag.Backend.Models;
using System.Reflection;
using SystemJsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SystemJsonPropertyNameAttribute = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Plag.Backend.Connectors
{
    internal class PdsJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property == null) return null;

            if (property.DeclaringType == typeof(PlagiarismSet))
            {
                if (member.GetCustomAttribute<SystemJsonIgnoreAttribute>() != null)
                {
                    return null;
                }

                SystemJsonPropertyNameAttribute nameAttribute = member.GetCustomAttribute<SystemJsonPropertyNameAttribute>();
                if (nameAttribute != null)
                {
                    property.PropertyName = nameAttribute.Name;
                }

                if (property.DeclaringType == typeof(PlagiarismSet)
                    && property.PropertyName == "setid"
                    && member.Name == "Id")
                {
                    property.PropertyName = "id";
                }
            }

            return property;
        }
    }
}
