using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Plag
{
    public class SubmissionZipArchive : ISubmissionFile
    {
        public SubmissionZipArchive(ZipArchive zipArchive, IReadOnlyCollection<string> suffix)
        {
            Zip = zipArchive;
            Accept = suffix;
        }

        public IEnumerator<ISubmissionFile> GetEnumerator()
        {
            foreach (var item in Zip.Entries)
                foreach (var suffix in Accept)
                    if (item.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        yield return new SubmissionZipArchiveEntry(item);
        }

        public ZipArchive Zip { get; }

        public IReadOnlyCollection<string> Accept { get; }

        public string Path => "./";

        public bool IsLeaf => false;

        public ICharStream Open() => throw new InvalidOperationException();
    }
}
