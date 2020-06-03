using Antlr4.Runtime;
using Plag;
using SatelliteSite.Data.Submit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.Data
{
    public class SubmissionFileProxy : ISubmissionFile
    {
        private class ConcreteFileProxy : ISubmissionFile
        {
            public File ConcreteFile { get; }

            public ConcreteFileProxy(File file) => ConcreteFile = file;

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
        }

        public Submit.Submission Submission { get; }

        public SubmissionFileProxy(Submit.Submission file) => Submission = file;

        public string Path => "./";

        public bool IsLeaf => true;

        public int Id => -1;

        public ICharStream Open() => throw new InvalidOperationException();

        public IEnumerator<ISubmissionFile> GetEnumerator()
        {
            return Submission.Files.Select(f => new ConcreteFileProxy(f)).GetEnumerator();
        }
    }
}
