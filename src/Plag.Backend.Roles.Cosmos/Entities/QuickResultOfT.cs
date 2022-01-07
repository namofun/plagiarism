using Newtonsoft.Json;

namespace Plag.Backend
{
    internal class QuickResult<TResult>
    {
        [JsonProperty("result")]
        public TResult Result { get; set; }
    }
}
