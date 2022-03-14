using System.Collections.Generic;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.Entities
{
    public class ServiceGraphEntity : MetadataEntity<Dictionary<string, ServiceVertex>>
    {
        public ServiceGraphEntity()
        {
            Type = ServiceGraphTypeKey;
            Data = new();
        }
    }
}
