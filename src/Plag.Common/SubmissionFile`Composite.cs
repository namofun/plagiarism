using Antlr4.Runtime;
using System;
using System.Collections.Generic;

namespace Plag
{
    public class SubmissionComposite : List<ISubmissionFile>, ISubmissionFile
    {
        public string Path => "./";

        public bool IsLeaf => false;

        public int Id => -1;

        public ICharStream Open() => throw new InvalidOperationException();

        public static IEnumerable<ISubmissionFile> ExtendToLeaf(ISubmissionFile file)
        {
            if (file.IsLeaf)
            {
                yield return file;
            }
            else
            {
                foreach (var item in file)
                {
                    foreach (var item2 in ExtendToLeaf(item))
                    {
                        yield return item2;
                    }
                }
            }
        }
    }
}
