using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class PdsRegistry
    {
        public static IReadOnlyDictionary<string, Plag.ILanguage> SupportedLanguages { get; }
            = new Dictionary<string, Plag.ILanguage>
            {
                ["csharp8"] = new Plag.Frontend.Csharp.Language(),
                ["cpp14"] = new Plag.Frontend.Cpp.Language(),
                ["java9"] = new Plag.Frontend.Java.Language(),
                ["py3"] = new Plag.Frontend.Python.Language(),
            };

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
