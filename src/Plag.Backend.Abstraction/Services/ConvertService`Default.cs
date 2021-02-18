using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Plag.Backend.Services
{
    public class DefaultConvertService : IConvertService
    {
        public static IReadOnlyList<FileMatch> DeserializeMatch(byte[] source)
        {
            if (source == null || source.Length % 20 != 0)
                throw new InvalidDataException();
            var lst = new List<FileMatch>(source.Length / 20);
            var ints = new int[5];

            for (int i = 0, j = 0; i < source.Length; i += 20, j++)
            {
                Buffer.BlockCopy(source, i, ints, 0, 20);
                lst.Add(new FileMatch
                {
                    ContentStartA = ints[1],
                    ContentEndA = ints[2],
                    ContentStartB = ints[3],
                    ContentEndB = ints[4],
                    FileA = ints[0] >> 16,
                    FileB = ints[0] & 0xffff,
                    MatchingId = j,
                });
            }

            return lst;
        }

        public IReadOnlyList<FileMatch> MatchDeserialize(byte[] source)
        {
            return DeserializeMatch(source);
        }
    }
}
