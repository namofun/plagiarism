using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Plag.Frontend.Python.Tests
{
    [TestClass]
    public class CompareWithJplag
    {
        [DataRow(
            @"prime=[2,3,5,7,11,13,17,19,23,29,31,37,41,43,47,53,59,61,67,71,73,79,83,89,97,101,103,107,109,113,127,131,137,139,149,151,157,163,167,173,179,181,191,193,197,199,211,223,227,229,233,239,241,251,257,263,269,271,277,281,283,293,307,311,313,317,331,337,347,349,353,359,367,373,379,383,389,397,401,409,419,421,431,433,439,443,449,457,461,463,467,479,487,491,499]
T=int(input())
for z in range(T):
    n,k=map(int,input().split(' '))
    y=k
    for i in prime:
        if(i*k<=n):
            y*=i
    print(y)
",
            "0\tASSIGN\r\n1\tARRAY\r\n2\tASSIGN\r\n3\tAPPLY\r\n4\tAPPLY\r\n5\tFOR{\r\n6\tAPPLY\r\n7\tASSIGN\r\n8\tAPPLY\r\n9\tAPPLY\r\n10\tARRAY\r\n11\tAPPLY\r\n12\tASSIGN\r\n13\tFOR{\r\n14\tIF{\r\n15\tASSIGN\r\n16\t}IF\r\n17\t}FOR\r\n18\tAPPLY\r\n19\t}FOR\r\n20\t********\r\n")]

        [TestMethod]
        public void ByDefault(string input, string output)
        {
            var lang = new Language(s => new JplagListener(s));
            var result = lang.Parse(new SubmissionString("main.py", input));
            Assert.IsFalse(result.Errors);
            Assert.AreEqual(output.Replace("\r\n", "\n"), result.ToString().Replace("\r\n", "\n"));
        }
    }
}
