namespace Antlr4.Runtime.Atn
{
    public class ResettablePredictionContextCache : PredictionContextCache
    {
        public void Reset()
        {
            cache.Clear();
        }
    }
}
