using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plag.Backend.Entities;
using Plag.Backend.Models;
using System.Collections.Generic;
using System.Reflection;
using SystemJsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SystemJsonPropertyNameAttribute = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Plag.Backend.Connectors
{
    internal class EntityJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        public static Dictionary<MemberInfo, string> SpecialConfiguration { get; } = new()
        {
            { typeof(PlagiarismSet).GetProperty(nameof(PlagiarismSet.Id)), "id" },
            { typeof(SetEntity).GetProperty(nameof(PlagiarismSet.Id)), "id" },
            { typeof(Submission).GetProperty(nameof(Submission.ExternalId)), "id" },
            { typeof(SubmissionEntity).GetProperty(nameof(Submission.ExternalId)), "id" },
            { typeof(Report).GetProperty(nameof(Report.Id)), "id" },
            { typeof(ReportEntity).GetProperty(nameof(Report.Id)), "id" },
            { typeof(LanguageInfo).GetProperty(nameof(LanguageInfo.ShortName)), "id" },
        };

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property == null) return null;

            if (property.DeclaringType == typeof(PlagiarismSet)
                || property.DeclaringType == typeof(SetEntity)
                || property.DeclaringType == typeof(Submission)
                || property.DeclaringType == typeof(SubmissionEntity)
                || property.DeclaringType == typeof(Report)
                || property.DeclaringType == typeof(ReportEntity)
                || property.DeclaringType == typeof(LanguageInfo))
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

                if (SpecialConfiguration.TryGetValue(member, out string altName))
                {
                    property.PropertyName = altName;
                }
            }

            return property;
        }
    }
}
