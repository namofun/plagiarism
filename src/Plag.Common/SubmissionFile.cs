using Antlr4.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace Plag
{
    public interface ISubmissionFile : IEnumerable<ISubmissionFile>
    {
        string Path { get; }

        bool IsLeaf { get; }

        ICharStream Open();

        int Id { get; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
