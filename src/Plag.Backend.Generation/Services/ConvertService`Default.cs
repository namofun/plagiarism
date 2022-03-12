using System;
using System.Collections.Generic;
using System.IO;
using Xylab.PlagiarismDetect.Frontend;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class DefaultConvertService2 : DefaultConvertService, IConvertService2
    {
        public byte[] MatchSerialize(Matching matching, bool swapAB)
        {
            var memory = new byte[20 * matching.SegmentCount];
            var ints = new int[5];

            for (int ii = 0; ii < matching.SegmentCount; ii++)
            {
                var i = matching[ii];

                if (swapAB)
                {
                    ints[0] = (matching.SubmissionB.IL[i.StartB].FileId << 16) | matching.SubmissionA.IL[i.StartA].FileId;
                    ints[1] = matching.SubmissionB.IL[i.StartB].Column;
                    ints[2] = matching.SubmissionB.IL[i.StartB + i.Length].Column;
                    ints[3] = matching.SubmissionA.IL[i.StartA].Column;
                    ints[4] = matching.SubmissionA.IL[i.StartA + i.Length].Column;
                }
                else
                {
                    ints[0] = (matching.SubmissionA.IL[i.StartA].FileId << 16) | matching.SubmissionB.IL[i.StartB].FileId;
                    ints[1] = matching.SubmissionA.IL[i.StartA].Column;
                    ints[2] = matching.SubmissionA.IL[i.StartA + i.Length].Column;
                    ints[3] = matching.SubmissionB.IL[i.StartB].Column;
                    ints[4] = matching.SubmissionB.IL[i.StartB + i.Length].Column;
                }

                Buffer.BlockCopy(ints, 0, memory, ii * 20, 20);
            }

            return memory;
        }

        public IReadOnlyList<Token> TokenDeserialize(byte[] source, ILanguage lang)
        {
            if (source == null || source.Length % 20 != 0)
                throw new InvalidDataException();
            var lst = new List<Token>(source.Length / 20);
            var ints = new int[5];

            for (int i = 0; i < source.Length; i += 20)
            {
                Buffer.BlockCopy(source, i, ints, 0, 20);
                lst.Add(lang.CreateToken(ints[3], ints[2], ints[0], ints[1], ints[4]));
            }

            return lst;
        }

        public byte[] TokenSerialize(IReadOnlyList<Token> tokens)
        {
            var memory = new byte[20 * tokens.Count];
            var ints = new int[5];

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                ints[0] = token.Column;
                ints[1] = token.Length;
                ints[2] = token.Line;
                ints[3] = token.Type;
                ints[4] = token.FileId;

                Buffer.BlockCopy(ints, 0, memory, i * 20, 20);
            }

            return memory;
        }
    }
}
