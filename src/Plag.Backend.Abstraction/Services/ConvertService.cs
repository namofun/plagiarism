using Plag.Backend.Entities;
using System.Collections.Generic;

namespace Plag.Backend.Services
{
    /// <summary>
    /// Conversion between entities and runtime poco.
    /// </summary>
    public interface IConvertService
    {
        /// <summary>
        /// Deserialize the matches.
        /// </summary>
        /// <param name="source">The byte array to deserialize.</param>
        /// <returns>The deserialized matches.</returns>
        IReadOnlyList<FileMatch> MatchDeserialize(byte[] source);
    }
}
