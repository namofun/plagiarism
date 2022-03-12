using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xylab.PlagiarismDetect.Backend.Entities;
using Xylab.PlagiarismDetect.Backend.Models;
using SystemJsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SystemJsonIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition;
using SystemJsonPropertyNameAttribute = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Xylab.PlagiarismDetect.Backend.QueryProvider
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

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType == typeof(ReportJustification)
                || objectType == typeof(ReportState))
            {
                contract.Converter = new StringEnumConverter();
            }

            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property == null) return null;

            if (property.DeclaringType == typeof(PlagiarismSet)
                || property.DeclaringType == typeof(SetEntity)
                || property.DeclaringType == typeof(Submission)
                || property.DeclaringType == typeof(SubmissionEntity)
                || property.DeclaringType == typeof(Report)
                || property.DeclaringType == typeof(Comparison)
                || property.DeclaringType == typeof(ReportEntity)
                || property.DeclaringType == typeof(LanguageInfo))
            {
                if (member.GetCustomAttribute<SystemJsonIgnoreAttribute>() is SystemJsonIgnoreAttribute systemIgnore)
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
