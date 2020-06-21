using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Plag
{
    public class Matching : IEnumerable<MatchPair>
    {
        private readonly List<MatchPair> matchPairs;
        private bool ok;
        private int tokensMatched;
        private int maxMatched;

        public Submission SubmissionA { get; }

        public Submission SubmissionB { get; }

        public int SegmentCount => matchPairs.Count;

        public Matching(Submission subA, Submission subB)
        {
            matchPairs = new List<MatchPair>();
            SubmissionA = subA;
            SubmissionB = subB;
        }

        public void AddMatch(MatchPair match)
        {
            if (ok) throw new InvalidOperationException("Matching is finished.");
            matchPairs.Add(match);
        }

        public void Finish()
        {
            matchPairs.Sort((a, b) => a.StartA.CompareTo(b.StartA));
            ok = true;
            tokensMatched = matchPairs.Select(a => a.Length).Prepend(0).Sum();
            maxMatched = matchPairs.Select(a => a.Length).Prepend(0).Max();
        }

        private T ThrowIfNotFinished<T>(T result)
        {
            if (!ok) throw new InvalidOperationException("Matching not finished.");
            return result;
        }

        public int TokensMatched => ThrowIfNotFinished(tokensMatched);

        public int BiggestMatch => ThrowIfNotFinished(maxMatched);

        public double Percent => 200.0 * TokensMatched / (SubmissionA.IL.Size + SubmissionB.IL.Size - SubmissionA.IL.Files - SubmissionB.IL.Files);

        public double PercentA => TokensMatched * 100.0 / (SubmissionA.IL.Size - SubmissionA.IL.Files);

        public double PercentB => TokensMatched * 100.0 / (SubmissionB.IL.Size - SubmissionB.IL.Files);

        public double PercentMaxAB => Math.Max(PercentA, PercentB);

        public double PercentMinAB => Math.Min(PercentA, PercentB);

        public IEnumerator<MatchPair> GetEnumerator()
        {
            return matchPairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return matchPairs.GetEnumerator();
        }
    }
}
