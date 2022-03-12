using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;

namespace Xylab.PlagiarismDetect.Frontend
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
            int t = 0;
            foreach (var item in Zip.Entries)
                foreach (var suffix in Accept)
                    if (item.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        yield return new SubmissionZipArchiveEntry(item, ++t);
        }

        public ZipArchive Zip { get; }

        public IReadOnlyCollection<string> Accept { get; }

        public string Path => "./";

        public bool IsLeaf => false;

        public int Id => -1;

        public ICharStream Open() => throw new InvalidOperationException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
