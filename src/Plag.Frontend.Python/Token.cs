namespace Plag.Frontend.Python
{
	public class Token : Plag.Token
	{
		public Token(TokenConstants type, int line, int start, int end, int fid) :
			base((int)type, line, start, end - start + 1, fid) { }

		public new TokenConstants Type => (TokenConstants)base.Type;

		public override string ToString() => TypeToString(Type);

		public override int NumberOfTokens() => (int)TokenConstants.NUM_DIFF_TOKENS;

        internal static string TypeToString(TokenConstants token)
        {
			return token switch
			{
				TokenConstants.FILE_END => "********",
				TokenConstants.SEPARATOR_TOKEN => "METHOD_SEPARATOR",
				TokenConstants.IMPORT => "IMPORT",
				TokenConstants.CLASS_BEGIN => "CLASS{",
				TokenConstants.CLASS_END => "}CLASS",
				TokenConstants.METHOD_BEGIN => "METHOD{",
				TokenConstants.METHOD_END => "}METHOD",
				TokenConstants.ASSIGN => "ASSIGN",
				TokenConstants.WHILE_BEGIN => "WHILE{",
				TokenConstants.WHILE_END => "}WHILE",
				TokenConstants.FOR_BEGIN => "FOR{",
				TokenConstants.FOR_END => "}FOR",
				TokenConstants.TRY_BEGIN => "TRY{",
				TokenConstants.EXCEPT_BEGIN => "CATCH{",
				TokenConstants.EXCEPT_END => "}CATCH",
				TokenConstants.FINALLY => "FINALLY",
				TokenConstants.IF_BEGIN => "IF{",
				TokenConstants.IF_END => "}IF",
				TokenConstants.APPLY => "APPLY",
				TokenConstants.BREAK => "BREAK",
				TokenConstants.CONTINUE => "CONTINUE",
				TokenConstants.RETURN => "RETURN",
				TokenConstants.RAISE => "RAISE",
				TokenConstants.DEC_BEGIN => "DECOR{",
				TokenConstants.DEC_END => "}DECOR",
				TokenConstants.LAMBDA => "LAMBDA",
				TokenConstants.ARRAY => "ARRAY",
				TokenConstants.ASSERT => "ASSERT",
				TokenConstants.YIELD => "YIELD",
				TokenConstants.DEL => "DEL",
				TokenConstants.WITH_BEGIN => "WITH{",
				TokenConstants.WITH_END => "}WITH",
				_ => "<UNBEKANNT>",
			};
		}
    }
}
