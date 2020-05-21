using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Plag.Integration.Tests
{
    [TestClass]
    public class CompareAndMatch
    {
        [DataRow(
            @"#include <iostream>
#include <cmath>
#include <cstring>
#include <cstdio>
#include <algorithm>
#include <cassert>
#include <map>
#include <vector>
using namespace std;
int n,m;
long long K;
const int N=110000;
vector<int> A[3],B[3];
long long get(long long lim){
        if (lim>=0){
                long long ans=(long long)(A[1].size())*m+(long long)(B[1].size())*(n-A[1].size());
                ans+=1ll*A[0].size()*B[2].size()+1ll*A[2].size()*B[0].size();
                for (int a=0;a<=2;a+=2){
                        int b=a;
                        int now=int(B[b].size())-1;
                        for (int i=0;i<A[a].size();++i){
                                while (now>=0&&1ll*A[a][i]*B[b][now]>lim) now--;
                                ans+=now+1;
                        }
                }
                return ans;
        } else if (lim<0){
                long long ans=0; lim=-lim;
                for (int a=0;a<=2;a+=2){
                        int b=2-a;
                        int now=int(B[b].size())-1;
                        for (int i=0;i<A[a].size();++i){
                                while (now>=0&&1ll*A[a][i]*B[b][now]>=lim) now--;
                                ans+=int(B[b].size())-now-1;
                        }
                }
                return ans;
        }
}
int main(){
        scanf(""%d%d%lld"",&n,&m,&K);
        K=1ll*n*m-K+1;
        for (int i=1;i<=n;i++){
                int k1; scanf(""%d"",&k1);
                if (k1>0) A[0].push_back(k1);
                else if (k1==0) A[1].push_back(k1);
                else A[2].push_back(-k1);
        }
        for (int i=1;i<=m;i++){
                int k1; scanf(""%d"",&k1);
                if (k1>0) B[0].push_back(k1);
                else if (k1==0) B[1].push_back(k1);
                else B[2].push_back(-k1);
        }
        for (int i=0;i<3;i++){
                sort(A[i].begin(),A[i].end());
                sort(B[i].begin(),B[i].end());
        }
        long long l=0,r=2e18,ans=0,bias=1e18;
        while (l<r){
                long long mid=(l+r>>1);
                if (get(mid-bias)>=K){
                        ans=mid-bias; r=mid;
                } else l=mid+1;
        }
        cout<<ans<<endl;
}",
            @"#include<bits/stdc++.h>
typedef long long ll;
using namespace std;
int n,m;ll K;
const int N=110000;
vector<int> A[3],B[3];
ll get(ll lim)
{
        if (lim>=0)
        {
                ll ans=(ll)(A[1].size())*m+(ll)(B[1].size())*(n-A[1].size());
                ans+=1ll*A[0].size()*B[2].size()+1ll*A[2].size()*B[0].size();
                for (int a=0;a<=2;a+=2)
                {
                        int b=a;
                        int now=int(B[b].size())-1;
                        for (int i=0;i<A[a].size();++i)
                        {
                                while (now>=0&&1ll*A[a][i]*B[b][now]>lim)
                                        now--;
                                ans+=now+1;
                        }
                }
                //cout<<""1 ""<<ans<<endl;
                return ans;
        }
        else if (lim<0)
        {
                lim*=-1;
                ll ans=0;
                for (int a=0;a<=2;a+=2)
                {
                        int b=2-a;
                        int now=int(B[b].size())-1;
                        for (int i=0;i<A[a].size();++i)
                        {
                                while (now>=0&&1ll*A[a][i]*B[b][now]>=lim)
                                        now--;
                                ans+=int(B[b].size())-now-1;
                        }
                }
                //cout<<""2: ""<<ans<<endl;
                return ans;
        }
}
int main()
{
        scanf(""%d%d%lld"",&n,&m,&K);
        K=1ll*n*m-K+1;
        for (int i=1;i<=n;i++)
        {
                int k1; scanf(""%d"",&k1);
                if (k1>0) A[0].push_back(k1);
                else if (k1==0) A[1].push_back(k1);
                else A[2].push_back(-k1);
        }
        for (int i=1;i<=m;i++)
        {
                int k1; scanf(""%d"",&k1);
                if (k1>0) B[0].push_back(k1);
                else if (k1==0) B[1].push_back(k1);
                else B[2].push_back(-k1);
        }
        for (int i=0;i<3;i++)
        {
                sort(A[i].begin(),A[i].end());
                sort(B[i].begin(),B[i].end());
        }
        ll l=0,r=2e12,ans=0,bias=1e12;
        while (l<=r)
        {
                ll mid=(l+r>>1);
                ll qwq=get(mid-bias);
                //cout<<qwq<<endl;
                if (qwq>=K)
                {
                        ans=mid-bias;
                        r=mid-1;
                }
                else
                        l=mid+1;
        }
        cout<<ans<<endl;
}
/*
1 3 3
-2
4 2 -2
*/",
            typeof(Plag.Frontend.Cpp.Language))]

        [DataRow(
            @"#include<iostream>
using namespace std;
#define DBL_MAX         1.7976931348623158e+308
int n;
double OPTIMAL_BST(double *p,double *q,int n,double **e,double **w,int **root)
{
	for(int i=1;i<=n+1;i++)
	{
		e[i][i-1]=q[i-1];
		w[i][i-1]=q[i-1];
	}
	for(int l=1;l<=n;l++)
	{
		for(int i=1;i<=n-l+1;i++)
		{	
			int j=i+l-1;
			e[i][j]=DBL_MAX ;
			w[i][j]=w[i][j-1]+p[j]+q[j];
			for(int r=i;r<=j;r++)
			{
				double t=e[i][r-1]+e[r+1][j]+w[i][j];
				
				if(t<e[i][j])
				{
					e[i][j]=t;
					root[i][j]=r;
				}
			}
		}
	}
	return e[1][n];
}

int main()
{
	cin>>n;
	double p[n+1];
	double q[n+1];
	p[0]=0;
	for(int i=1; i<=n; i++) cin>>p[i];
	for(int i=0; i<n+1; i++) cin>>q[i];
	double **e=new double *[n+2];
	for(int i=0;i<=n+1;i++) e[i]=new double[n+1];
	double **w=new double *[n+2];
	for(int i=0;i<=n+1;i++) w[i]=new double[n+1];
	int **root=new int *[n+1];
	for(int i=0;i<=n;i++) root[i]=new int[n+1];
	cout<<OPTIMAL_BST(p,q,n,e,w,root)<<endl;
	for (int i = 0; i < n+2; i++)     
    {  
        delete e[i],delete w[i];    
        e[i] = NULL,w[i] = NULL;
		if(i!=n+1)
		{
			delete root[i];
			root[i] = NULL;
		}
    }  
	delete []e,delete[]w,delete []root; 
    e = NULL,w=NULL,root=NULL;
	return 0;
}",
            @"#include<iostream>
#include <iomanip>
using namespace std;
#define DBL_MAX         1.7976931348623158e+308

double OPTIMAL_BST(double *p,double *q,int n,double **e,double **w,int **root)
{
	for(int i=1;i<=n+1;i++)
	{
		e[i][i-1]=q[i-1];
		w[i][i-1]=q[i-1];
	}
	for(int l=1;l<=n;l++)         /* ???????? */
	{
		for(int i=1;i<=n-l+1;i++)
		{
			int j=i+l-1;
			e[i][j]=DBL_MAX ;
			w[i][j]=w[i][j-1]+p[j]+q[j];
			for(int r=i;r<=j;r++)
			{
				double t=e[i][r-1]+e[r+1][j]+w[i][j];

				if(t<e[i][j])
				{
					e[i][j]=t;
					root[i][j]=r;
					//cout<<root[i][j]<<""\t""<<i<<"" ===============""<<j<<endl;
				}
			}
		}
	}
	return e[1][n];
}
void CONSTRUCT_OPTIAML_BST(int **root,int i,int j)
{
	//if(i==1 && j==n)printf(""K%d??\n"",root[i][j]);
	if(i < j)
	{
		//printf(""K%d?K%d????\n"",root[i][root[i][j]-1],root[i][j]);
		CONSTRUCT_OPTIAML_BST(root,i,root[i][j]-1);
		if(root[i][j]+1 < j) /* ?????????????root[5][4]?root[6][5] */
		//printf(""K%d?K%d????\n"",root[root[i][j]+1][j],root[i][j]);
		/* else  cout<<root[i][j]+1<<""\t""<<j<<endl; */
		CONSTRUCT_OPTIAML_BST(root,root[i][j]+1,j);
	}

	if(i == j)
	{
		//printf(""d%d?K%d????\n"",i-1,i);
		//printf(""d%d?K%d????\n"",i,i);
	}
	//if(i > j) printf(""d%d?K%d????\n"",j,j);
}
int main()
{
    int n;
    cin>>n;
    double p[n+2];
    double q[n+2];

    for(int i=1; i<=n; i++)
        cin>>p[i];
    for(int i=0; i<=n; i++)
        cin>>q[i];
	/* ???? */
	/* ?????e[1..n+1,0..n]  w[1..n+1,0..n] root[1..n,1..n] */
	double **e=new double *[n+2];
	for(int i=0;i<=n+1;i++) e[i]=new double[n+1];
	double **w=new double *[n+2];
	for(int i=0;i<=n+1;i++) w[i]=new double[n+1];
	int **root=new int *[n+1];
	for(int i=0;i<=n;i++) root[i]=new int[n+1];
	/* ???e?root */
	cout<<fixed<<setprecision(6)<<OPTIMAL_BST(p,q,n,e,w,root)<<endl;
	/* ??root */
	 CONSTRUCT_OPTIAML_BST(root,1,n);
	/* ???? */
	for (int i = 0; i < n+2; i++)
    {
        delete e[i],delete w[i];
        e[i] = NULL,w[i] = NULL;
		if(i!=n+1)
		{
			delete root[i];
			root[i] = NULL;
		}
    }
	delete []e,delete[]w,delete []root;
    e = NULL,w=NULL,root=NULL;
	return 0;
}",
            typeof(Plag.Frontend.Cpp.Language))]

        [DataRow(
            @"using System;

namespace Problem1
{
    public enum Suits
    {
        Clubs,Diamonds,Hearts,Spades
    }
    class Program
    {
        static void Main(string[] args)
        {
            var AllSuits = Enum.GetValues(typeof(Suits));
            Console.WriteLine(""Card Suits: "");
            foreach(var a in AllSuits)
            {
                Console.WriteLine(""Ordinal Value: {0},Name Value: {1}"", (int)a, a.ToString());
            }
        }
    }
}
",
            @"using System;

namespace Problem_1
{
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades,
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Card Suits: "");
            foreach (var t in Enum.GetValues(typeof(Suit)))
            {

				Console.WriteLine(""Ordinal value:{0}; Name value:{1}"", (int)t, t.ToString());

			}
		}
    }
}
",
            typeof(Plag.Frontend.Csharp.Language))]

        [TestMethod]
        public void DefaultCompare(string submit1, string submit2, Type language)
        {
            var lang = (ILanguage)Activator.CreateInstance(language);
            var sub1 = new Submission(lang, new SubmissionString("A.cpp", submit1));
            var sub2 = new Submission(lang, new SubmissionString("B.cpp", submit2));
            var compareResult = GSTiling.Compare(sub1, sub2, lang.MinimalTokenMatch);
        }
    }
}
