﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SatelliteSite.Data.Match
{
    public class MatchPair
    {
        public int StartA { set; get; }
        public int StartB { set; get; }
        public int Length { set; get; }
        public int ContentStartA { set; get; }
        public int ContentEndA { set; get; }
        public int ContentStartB { set; get; }
        public int ContentEndB { set; get; }

        public MatchPair(int startA, int startB, int length,int contentStartA,int contentEndA,int contentStartB,int contentEndB)
        {
            StartA = startA;
            StartB = startB;
            Length = length;
            ContentStartA = contentStartA;
            ContentStartB = contentStartB;
            ContentEndA = contentEndA;
            ContentEndB = contentEndB;
        }
    }
}
