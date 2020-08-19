using System;
using System.Collections.Generic;
using System.Linq;

namespace Plag
{
    /// <summary>
    /// Hash table implemented by JPlag
    /// </summary>
    public class HashTable
    {
        private static readonly int[] prim;
        private readonly List<int>[] data;
        private readonly int size;

        static HashTable()
        {
            var prims = new List<int> { 1 };
            var isnp = new bool[16002];
            for (int i = 0; i < 16002; i++) isnp[i] = false;

            for (int i = 2; i <= 16001; i++)
            {
                if (!isnp[i]) prims.Add(i);
                foreach (int j in prims)
                {
                    int k = i * j;
                    if (k > isnp.Length) break;
                    isnp[k] = true;
                    if (i % j == 0) break;
                }
            }

            prim = prims.ToArray();
        }

        public HashTable(int _size)
        {
            size = _size >= 16001 ? 16001 : prim.SkipWhile(i => i <= _size).First();
            data = new List<int>[size];
        }

        public void Add(int hash, int datum) => this[hash % size].Add(datum);

        public List<int> this[int hash] => data[hash % size] ??= new List<int>();

        public bool TryGet(int hash, out List<int> lst)
        {
            lst = data[hash % size];
            return lst != null;
        }

        public void CountDist(int[] dist)
        {
            for (int i = 0; i < size; i++)
                dist[data[i] == null ? 0 : Math.Min(data[i].Count, dist.Length - 1)]++;
        }
    }
}
