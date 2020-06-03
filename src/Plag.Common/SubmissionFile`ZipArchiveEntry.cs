using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO.Compression;

namespace Plag
{
    public class SubmissionZipArchiveEntry : ISubmissionFile
    {
        public SubmissionZipArchiveEntry(ZipArchiveEntry entry, int id)
        {
            Entry = entry;
            Id = id;
        }

        public IEnumerator<ISubmissionFile> GetEnumerator()
        {
            yield return this;
        }

        public string Path => Entry.FullName;

        public ZipArchiveEntry Entry { get; }

        public bool IsLeaf => true;

        public int Id { get; }

        public ICharStream Open() => new AntlrInputStream(Entry.Open()) { name = Path };
    }
}
