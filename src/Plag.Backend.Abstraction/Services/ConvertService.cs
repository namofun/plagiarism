using Plag.Backend.Entities;
using System.Collections.Generic;

namespace Plag.Backend.Services
{
    public interface IConvertService
    {
        byte[] TokenSerialize(IReadOnlyList<Token> tokens);

        IReadOnlyList<Token> TokenDeserialize(byte[] source, ILanguage language);

        IReadOnlyList<FileMatch> MatchDeserialize(byte[] source);

        byte[] MatchSerialize(Matching matches, bool swapAB);
    }
}
