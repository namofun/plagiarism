#nullable enable
using System;

namespace Plag.Backend.Jobs
{
    public static class CorrelationRecord
    {
        public static string New(string intention, string? parent = null)
        {
            Guid recordId = Guid.NewGuid();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string record = $"{intention}|{timestamp}|{recordId}";
            if (!string.IsNullOrEmpty(parent)) record += "%" + parent;
            return record;
        }

        public static string Parent(string record)
        {
            if (record.StartsWith("continuation|"))
            {
                record = record.Split('%')[0];
            }

            return record;
        }
    }
}
