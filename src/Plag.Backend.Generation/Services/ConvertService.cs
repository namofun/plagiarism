﻿using System.Collections.Generic;
using Xylab.PlagiarismDetect.Frontend;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    /// <summary>
    /// Conversion between entities and runtime poco.
    /// </summary>
    public interface IConvertService2 : IConvertService
    {
        /// <summary>
        /// Serialize the tokens.
        /// </summary>
        /// <param name="tokens">The tokens to serialize.</param>
        /// <returns>The serialized byte array.</returns>
        byte[] TokenSerialize(IReadOnlyList<Token> tokens);

        /// <summary>
        /// Deserialize the tokens.
        /// </summary>
        /// <param name="source">The byte array to deserialized.</param>
        /// <param name="language">The target language.</param>
        /// <returns>The deserialized tokens.</returns>
        IReadOnlyList<Token> TokenDeserialize(byte[] source, ILanguage language);

        /// <summary>
        /// Serialize the matches.
        /// </summary>
        /// <param name="matches">The match to serialize.</param>
        /// <param name="swapAB">Whether to swap A and B in serialization.</param>
        /// <returns>The serialized byte array.</returns>
        byte[] MatchSerialize(Matching matches, bool swapAB);
    }
}
