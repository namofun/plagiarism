namespace Plag.Frontend.Csharp
{
	public class Token : Plag.Token
    {
        public Token(TokenConstants type, int line, int start, int end) : base((int)type, line, start, end - start + 1) { }

        public new TokenConstants Type => (TokenConstants)base.Type;

		public override string ToString() => TypeToString(Type);

		public override int NumberOfTokens() => (int)TokenConstants.NUM_DIFF_TOKENS;

        internal static string TypeToString(TokenConstants token)
        {
			return token switch
			{
				TokenConstants.FILE_END => "********",
				TokenConstants.C_BLOCK_BEGIN => "BLOCK{",
				TokenConstants.C_BLOCK_END => "}BLOCK",
				TokenConstants.C_SCOPE => "SCOPE",
				TokenConstants.C_QUESTIONMARK => "COND",
				TokenConstants.C_ELLIPSIS => "...",
				TokenConstants.C_ASSIGN => "ASSIGN",
				TokenConstants.C_DOT => "DOT",
				TokenConstants.C_ARROW => "ARROW",
				TokenConstants.C_DOTSTAR => "DOTSTAR",
				TokenConstants.C_ARROWSTAR => "ARROWSTAR",
				TokenConstants.C_AUTO => "AUTO",
				TokenConstants.C_BREAK => "BREAK",
				TokenConstants.C_CASE => "CASE",
				TokenConstants.C_CATCH => "CATCH",
				TokenConstants.C_CHAR => "CHAR",
				TokenConstants.C_CONST => "CONST",
				TokenConstants.C_CONTINUE => "CONTINUE",
				TokenConstants.C_DEFAULT => "DEFAULT",
				TokenConstants.C_DELETE => "DELETE",
				TokenConstants.C_DO => "DO",
				TokenConstants.C_DOUBLE => "DOUBLE",
				TokenConstants.C_ELSE => "ELSE",
				TokenConstants.C_ENUM => "ENUM",
				TokenConstants.C_EXTERN => "EXTERN",
				TokenConstants.C_FLOAT => "FLOAT",
				TokenConstants.C_FOR => "FOR",
				TokenConstants.C_FRIEND => "FRIEND",
				TokenConstants.C_GOTO => "GOTO",
				TokenConstants.C_IF => "IF",
				TokenConstants.C_INLINE => "INLINE",
				TokenConstants.C_INT => "INT",
				TokenConstants.C_LONG => "LONG",
				TokenConstants.C_NEW => "NEW",
				TokenConstants.C_PRIVATE => "PRIVATE",
				TokenConstants.C_PROTECTED => "PROTECTED",
				TokenConstants.C_PUBLIC => "PUBLIC",
				TokenConstants.C_REDECLARED => "REDECLARED",
				TokenConstants.C_REGISTER => "REGISTER",
				TokenConstants.C_RETURN => "RETURN",
				TokenConstants.C_SHORT => "SHORT",
				TokenConstants.C_SIGNED => "SIGNED",
				TokenConstants.C_SIZEOF => "SIZEOF",
				TokenConstants.C_STATIC => "STATIC",
				TokenConstants.C_STRUCT => "STRUCT",
				TokenConstants.C_CLASS => "CLASS",
				TokenConstants.C_SWITCH => "SWITCH",
				TokenConstants.C_TEMPLATE => "TEMPLATE",
				TokenConstants.C_THIS => "THIS",
				TokenConstants.C_TRY => "TRY",
				TokenConstants.C_TYPEDEF => "TYPEDEF",
				TokenConstants.C_UNION => "UNION",
				TokenConstants.C_UNSIGNED => "UNSIGNED",
				TokenConstants.C_VIRTUAL => "VIRTUAL",
				TokenConstants.C_VOID => "VOID",
				TokenConstants.C_VOLANTILE => "VOLANTILE",
				TokenConstants.C_WHILE => "WHILE",
				TokenConstants.C_OPERATOR => "OPERATOR",
				TokenConstants.C_THROW => "THROW",
				TokenConstants.C_ID => "ID",
				TokenConstants.C_FUN => "FUN",
				TokenConstants.C_NULL => "NULL",
				_ => "<UNBEKANNT>",
			};
		}
    }
}
