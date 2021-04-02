using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Plag.Frontend.Cpp.Tests
{
    [TestClass]
    public class CanRun
    {
        [DataRow(@"#include <iostream.h>

using namespace std;

int main()
{
    if (true and true and (false or (not false))) {
        cout << ""Hello World!"";
    }
    return 0;
}

void TemplateArgsTest(vector<ClassA> args, vector <ClassB> args2)
{
}

#define Verify(cond, msg)                                                                                              \
    do                                                                                                                 \
    {                                                                                                                  \
        if (!(cond))                                                                                                   \
        {                                                                                                              \
            verRaiseVerifyExceptionIfNeeded(INDEBUG(msg) DEBUGARG(__FILE__) DEBUGARG(__LINE__));                       \
        }                                                                                                              \
    } while (0)

void f() {
  int b = 0;
}
")]

        [DataRow("std::priority_queue<pair<int,int>, vector<pair<int,int>>, greater<pair<int,int>>> pq;")]

        [DataRow(@"#include <cstdio>

int main() {
    printf(""aaa""
""bb"");
    return 0;
}
")]

        [TestMethod]
        public void Parse(string content)
        {
            var lang = new Language();
            var stu = lang.Parse(new SubmissionString("A.cpp", content));
            Assert.IsFalse(stu.Errors, stu.ErrorInfo.ToString());
        }
    }
}
