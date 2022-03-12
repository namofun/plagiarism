using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.Tests
{
    internal class NullConfigurationRegistry : IConfigurationRegistry
    {
        public Task<Configuration> FindAsync(string config)
        {
            return Task.FromResult<Configuration>(null);
        }

        public Task<List<Configuration>> GetAsync(string name = null)
        {
            return Task.FromResult(new List<Configuration>());
        }

        public Task<bool?> GetBooleanAsync(string name)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<DateTimeOffset?> GetDateTimeOffsetAsync(string name)
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }

        public Task<int?> GetIntegerAsync(string name)
        {
            return Task.FromResult<int?>(null);
        }

        public Task<string> GetStringAsync(string name)
        {
            return Task.FromResult<string>("[]");
        }

        public Task<ILookup<string, Configuration>> ListAsync()
        {
            return Task.FromResult(Array.Empty<Configuration>().ToLookup(a => a.Name));
        }

        public Task<bool> UpdateAsync(string name, string newValue)
        {
            return Task.FromResult<bool>(false);
        }
    }
}
