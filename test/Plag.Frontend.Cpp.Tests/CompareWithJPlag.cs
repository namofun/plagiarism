using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xylab.PlagiarismDetect.Frontend.Cpp.Tests
{
    [TestClass]
    public class CompareWithJPlag
    {
		[DataRow(@"int main()
{
	return NULL;
}",
			"0\tINT\r\n1\tBLOCK{\r\n2\tRETURN\r\n3\tNULL\r\n4\t}BLOCK\r\n5\t********\r\n")]
        
		[DataRow(@"#include <iostream>

#define MAIN() do { \
        for (int i = 0; i < 100; i++) \
                for (int j = 0; j < 100; j++) \
                        for (int k = 0; k < 10; k++) \
                                for (int l = 0; l < 10; l++) \
                                        a[i][j][k][l] += a[i][j][k][l-1]; \
} while (false)

int a[100][100][10][10];

int main()
{
        MAIN();
        int t = 100 * 9 + 8 & 7;
        do { int x = 100 * 99 + 8; } while (false);
        return 0;
}", 
			"0\tINT\r\n1\tINT\r\n2\tBLOCK{\r\n3\tINT\r\n4\tASSIGN\r\n5\tDO\r\n6\tBLOCK{\r\n7\tINT\r\n" +
			"8\tASSIGN\r\n9\t}BLOCK\r\n10\tWHILE\r\n11\tRETURN\r\n12\t}BLOCK\r\n13\t********\r\n")]

        [DataRow(@"#include <bits/stdc++.h>
using namespace std;
char buf[110][110];
const int dx[8] = { 0, 0, 1, -1, 1, -1, 1, -1 };
const int dy[8] = { 1, -1, 0, 0, 1, 1, -1, -1 };

int solve(int ii, int jj)
{
	int count = 1;
	static pair<int,int> Q[10010];
	int front = 0, rear = 0;
	Q[rear++] = make_pair(ii, jj);
	buf[ii][jj] = '.';

	while (front < rear)
	{
		int x = Q[front].first;
		int y = Q[front].second;
		front++;
		
		for (int i = 0; i < 8; i++)
		if (buf[x+dx[i]][y+dy[i]] == 'W')
		{
			buf[x+dx[i]][y+dy[i]] = '.';
			Q[rear++] = make_pair(x+dx[i], y+dy[i]);
			count++;
		}
	}
	
	fprintf(stderr, ""(%d,%d)=%d\n"", ii, jj, count);
	return count * (count-1) / 2;
}

int main()
{
	int n, m;
	scanf(""%d %d"", &n, &m);
	for (int i = 1; i <= n; i++)
		scanf(""%s"", buf[i]+1);
	int ans = 0;
	for (int i = 1; i <= n; i++)
		for (int j = 1; j <= m; j++)
			if (buf[i][j] == 'W')
				ans += solve(i, j);
	printf(""%d\n"", ans);
	return 0;
}", 
			"0\tCHAR\r\n1\tCONST\r\n2\tINT\r\n3\tASSIGN\r\n4\tBLOCK{\r\n5\t}BLOCK\r\n6\tCONST\r\n" +
			"7\tINT\r\n8\tASSIGN\r\n9\tBLOCK{\r\n10\t}BLOCK\r\n11\tINT\r\n12\tINT\r\n13\tINT\r\n" +
			"14\tBLOCK{\r\n15\tINT\r\n16\tASSIGN\r\n17\tSTATIC\r\n18\tINT\r\n19\tINT\r\n20\tINT\r\n" +
			"21\tASSIGN\r\n22\tASSIGN\r\n23\tASSIGN\r\n24\tASSIGN\r\n25\tASSIGN\r\n26\tWHILE\r\n" +
			"27\tBLOCK{\r\n28\tINT\r\n29\tASSIGN\r\n30\tINT\r\n31\tASSIGN\r\n32\tASSIGN\r\n33\tFOR\r\n" +
			"34\tINT\r\n35\tASSIGN\r\n36\tASSIGN\r\n37\tIF\r\n38\tBLOCK{\r\n39\tASSIGN\r\n40\tASSIGN\r\n" +
			"41\tASSIGN\r\n42\tASSIGN\r\n43\t}BLOCK\r\n44\t}BLOCK\r\n45\tRETURN\r\n46\t}BLOCK\r\n47\tINT\r\n" +
			"48\tBLOCK{\r\n49\tINT\r\n50\tFOR\r\n51\tINT\r\n52\tASSIGN\r\n53\tASSIGN\r\n54\tINT\r\n" +
			"55\tASSIGN\r\n56\tFOR\r\n57\tINT\r\n58\tASSIGN\r\n59\tASSIGN\r\n60\tFOR\r\n61\tINT\r\n" +
			"62\tASSIGN\r\n63\tASSIGN\r\n64\tIF\r\n65\tASSIGN\r\n66\tRETURN\r\n67\t}BLOCK\r\n68\t********\r\n")]

		[TestMethod]
		public void ByDefault(string input, string output)
        {
            var lang = new Language(s => new JplagListener(s));
            var result = lang.Parse(new SubmissionString("main.cpp", input));
            Assert.IsFalse(result.Errors);
            Assert.AreEqual(output.Replace("\r\n", "\n"), result.ToString().Replace("\r\n", "\n"));
        }
    }
}
