using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite.PlagModule
{
    public static class PdsRegistry
    {
        public static IReadOnlyDictionary<string, Plag.ILanguage> SupportedLanguages { get; }
            = new Dictionary<string, Plag.ILanguage>
            {
                ["csharp8"] = new Plag.Frontend.Csharp.Language(),
                ["cpp14"] = new Plag.Frontend.Cpp.Language(),
                ["java9"] = new Plag.Frontend.Java.Language(),
                ["py3"] = new Plag.Frontend.Python.Language(),
            };

        public static byte[] Serialize(IReadOnlyList<Plag.Token> tokens)
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

        public static byte[] Serialize(Plag.Matching matching, bool swapAB)
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

        public static async Task SerializeAsync(Stream stream, IReadOnlyList<Plag.Token> tokens)
        {
            var memory = new byte[20 * 10240];
            var ints = new int[5];

            for (int i = 0; i < tokens.Count; i += 10240)
            {
                var ub = i + 10240 < tokens.Count ? 10240 : tokens.Count - i;
                for (int j = 0; j < ub; j++)
                {
                    var token = tokens[i + j];
                    ints[0] = token.Column;
                    ints[1] = token.Length;
                    ints[2] = token.Line;
                    ints[3] = token.Type;
                    ints[4] = token.FileId;

                    Buffer.BlockCopy(ints, 0, memory, j * 20, 20);
                }

                await stream.WriteAsync(memory, 0, 20 * ub);
            }
        }

        public static List<Plag.Token> Deserialize(byte[] source, Plag.ILanguage lang)
        {
            if (source == null || source.Length % 20 != 0)
                throw new InvalidDataException();
            var lst = new List<Plag.Token>(source.Length / 20);
            var ints = new int[5];

            for (int i = 0; i < source.Length; i += 20)
            {
                Buffer.BlockCopy(source, i, ints, 0, 20);
                lst.Add(lang.CreateToken(ints[3], ints[2], ints[0], ints[1], ints[4]));
            }

            return lst;
        }

        public static List<Entities.MatchPair> Deserialize(byte[] source)
        {
            if (source == null || source.Length % 20 != 0)
                throw new InvalidDataException();
            var lst = new List<Entities.MatchPair>(source.Length / 20);
            var ints = new int[5];

            for (int i = 0, j = 0; i < source.Length; i += 20, j++)
            {
                Buffer.BlockCopy(source, i, ints, 0, 20);
                lst.Add(new Entities.MatchPair
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

        public static async Task<List<Plag.Token>> DeserializeAsync(Stream stream, Plag.ILanguage lang)
        {
            int len = (int)stream.Length;
            if (len % 20 != 0) throw new InvalidDataException();
            var lst = new List<Plag.Token>(len / 20);

            var memory = new byte[20 * 10240];
            var ints = new int[5];
            int t = 0;

            while (t < len)
            {
                var cnt = await stream.ReadAsync(memory, 0, 20 * 10240);
                t += cnt;
                if (cnt % 20 != 0) throw new IOException("Length wrong");
                for (int i = 0; i < cnt; i += 20)
                {
                    Buffer.BlockCopy(memory, i, ints, 0, 20);
                    lst.Add(lang.CreateToken(ints[3], ints[2], ints[0], ints[1], ints[4]));
                }
            }

            return lst;
        }
    }
}
