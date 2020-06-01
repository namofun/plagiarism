using System;
using System.Collections.Generic;
using System.Text;

namespace SatelliteSite.Data.Match
{
    public class MatchPair
    {
        public int StartA { set; get; }
        public int StartB { set; get; }
        public int Length { set; get; }

        public MatchPair(int startA, int startB, int length)
        {
            StartA = startA;
            StartB = startB;
            Length = length;
        }
    }
}
