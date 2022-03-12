using Antlr4.Runtime;
using Xylab.PlagiarismDetect.Backend.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xylab.PlagiarismDetect.Frontend
{
    public class SubmissionFileProxy : ISubmissionFile
    {
        private class ConcreteFileProxy : ISubmissionFile
        {
            public SubmissionFile ConcreteFile { get; }

            public ConcreteFileProxy(SubmissionFile file) => ConcreteFile = file;

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

        public IEnumerable<SubmissionFile> SubmissionFiles { get; }

        public SubmissionFileProxy(Backend.Models.Submission file) => SubmissionFiles = file.Files;

        public SubmissionFileProxy(IEnumerable<SubmissionFile> file) => SubmissionFiles = file;

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
