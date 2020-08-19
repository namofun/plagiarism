using Antlr4.Runtime;
using Plag;
using SatelliteSite.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.Data
{
    public class SubmissionFileProxy : ISubmissionFile
    {
        private class ConcreteFileProxy : ISubmissionFile
        {
            public PlagiarismFile ConcreteFile { get; }

            public ConcreteFileProxy(PlagiarismFile file) => ConcreteFile = file;

            public string Path => ConcreteFile.FilePath;

            public bool IsLeaf => true;

            public int Id => ConcreteFile.FileId;

            public IEnumerator<ISubmissionFile> GetEnumerator()
            {
                yield return this;
            }

            public ICharStream Open()
            {
                return new AntlrInputStream(ConcreteFile.Content) { name = ConcreteFile.FilePath };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public IEnumerable<PlagiarismFile> SubmissionFiles { get; }

        public SubmissionFileProxy(PlagiarismSubmission file) => SubmissionFiles = file.Files;

        public SubmissionFileProxy(IEnumerable<PlagiarismFile> file) => SubmissionFiles = file;

        public string Path => "./";

        public bool IsLeaf => false;

        public int Id => -1;

        public ICharStream Open() => throw new InvalidOperationException();

        public IEnumerator<ISubmissionFile> GetEnumerator()
        {
            return SubmissionFiles.Select(f => new ConcreteFileProxy(f)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
