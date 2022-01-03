using Newtonsoft.Json.Serialization;
using System;

namespace Plag.Backend.Connectors
{
    internal class PdsJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            return base.CreateContract(objectType);
        }
    }
}
