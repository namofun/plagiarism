using System.Collections.Generic;

namespace Plag
{
	public static class GSTiling
    {
		/// <summary>
		/// creating hashes.
		/// The hash-code will be written in every Token for the next&lt;hash_length&gt; token.
		/// (includes the Token itself)
		/// Das Ganze laeuft in linearer Zeit.
		/// condition: 1 &lt; hashLength &lt; 26   !!!
		/// </summary>
		/// <param name="s">Code strucutre</param>
		/// <param name="hashLength">Hash length</param>
		/// <param name="makeTable">Make table?</param>
		private static void CreateHashes(Structure s, int hashLength, bool makeTable)
		{
			// Hier wird die obere Grenze der Hash-Laenge festgelegt.
			// Sie ist bestimmt durch die Bitzahl des 'int' Datentyps und der Anzahl
			// der Token.

			if (hashLength < 1) hashLength = 1;
			hashLength = (hashLength < 26 ? hashLength : 25);
			if (s.Size < hashLength) return;

			int modulo = ((1 << 6) - 1);   // Modulo 64!
			int loops = s.Size - hashLength;

			s.CreateHashes(makeTable ? 3 * loops : default(int?), (s, table) =>
			{
				int hash = 0;
				int hashedLength = 0;

				for (int i = 0; i < hashLength; i++)
				{
					hash = (2 * hash) + (s[i].Type & modulo);
					hashedLength++;
					if (s[i].Marked) hashedLength = 0;
				}

				int factor = hashLength != 1 ? (2 << (hashLength - 2)) : 1;

				if (makeTable)
				{
					for (int i = 0; i < loops; i++)
					{
						if (hashedLength >= hashLength)
						{
							s[i].Hash = hash;
							table.Add(hash, i);   // add into hashtable
						}
						else
						{
							s[i].Hash = -1;
						}

						hash -= factor * (s[i].Type & modulo);
						hash = (2 * hash) + (s[i + hashLength].Type & modulo);
						if (s[i + hashLength].Marked)
							hashedLength = 0;
						else
							hashedLength++;
					}
				}
				else
				{
					for (int i = 0; i < loops; i++)
					{
						s[i].Hash = (hashedLength >= hashLength) ? hash : -1;
						hash -= factor * (s[i].Type & modulo);
						hash = (2 * hash) + (s[i + hashLength].Type & modulo);
						if (s[i + hashLength].Marked)
							hashedLength = 0;
						else
							hashedLength++;
					}
				}

				return hashLength;
			});
		}

		public static Matching Compare(Submission subA, Submission subB, int mml)
		{
			if (subA.IL.Size > subB.IL.Size)
				(subA, subB) = (subB, subA);

			// if hashtable exists in first but not in second structure: flip around!
			if (subB.IL.Table == null && subA.IL.Table != null)
				(subA, subB) = (subB, subA);
			
			// first parameter should contain the smaller sequence!!!
			var (A, B) = (subA.IL, subB.IL);

			// FILE_END used as pivot
			var (lenA, lenB) = (A.Size - 1, B.Size - 1);
			var allMatches = new Matching(subA, subB);
			if (lenA < mml || lenB < mml)
				return allMatches;

			for (int i = 0; i <= lenA; i++)
				A[i].Marked = A[i].Type == (int)TokenConstants.FILE_END
					|| A[i].Type == (int)TokenConstants.SEPARATOR_TOKEN;
			for (int i = 0; i <= lenB; i++)
				B[i].Marked = B[i].Type == (int)TokenConstants.FILE_END
					|| B[i].Type == (int)TokenConstants.SEPARATOR_TOKEN;
			if (A.HashLength != mml)
				CreateHashes(A, mml, false);
			if (B.HashLength != mml || B.Table == null)
				CreateHashes(B, mml, true);

			int maxmatch;
			var matches = new List<MatchPair>();

			do
			{
				maxmatch = mml;
				matches.Clear();

				for (int x = 0; x <= lenA - maxmatch; x++)
				{
					if (A[x].Marked || A[x].Hash == -1 || !B.Table.TryGet(A[x].Hash, out var elemsB))
						continue;

					//inner: for (int i = 1; i <= elemsB[0]; i++)
					foreach (var y in elemsB)
					{
						if (B[y].Marked || maxmatch > lenB - y) continue;

						int j, hx, hy;
						bool breakout = false;

						for (j = maxmatch - 1; j >= 0; j--)
						{
							//begins comparison from behind
							if (A[hx = x + j].Type != B[hy = y + j].Type || A[hx].Marked || B[hy].Marked)
							{
								breakout = true;
								break;
							}
						}

						if (breakout) continue;

						// expand match
						j = maxmatch;

						while (A[hx = x + j].Type == B[hy = y + j].Type && !A[hx].Marked && !B[hy].Marked)
							j++;

						if (j > maxmatch)
						{
							// new biggest match? -> delete current smaller
							matches.Clear();
							maxmatch = j;
						}

						matches.Add(new MatchPair(x, y, j)); // add match
					}
				}

				for (int i = matches.Count - 1; i >= 0; i--)
				{
					int x = matches[i].StartA;  // begining of sequence A
					int y = matches[i].StartB;  // begining of sequence B
					allMatches.AddMatch(matches[i]);

					// in order that "Match" will be newly build     (because reusing)
					for (int j = matches[i].Length; j > 0; j--)
						A[x++].Marked = B[y++].Marked = true;   // mark all Token!
				}
			}
			while (maxmatch != mml);

			allMatches.Finish();
			return allMatches;
		}
    }
}
