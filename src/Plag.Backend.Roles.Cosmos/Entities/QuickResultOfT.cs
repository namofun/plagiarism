using Newtonsoft.Json;

namespace Xylab.PlagiarismDetect.Backend
{
    internal class QuickResult<TResult>
    {
        [JsonProperty("result")]
        public TResult Result { get; set; }
    }
}
