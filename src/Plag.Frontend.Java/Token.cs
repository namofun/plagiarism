namespace Xylab.PlagiarismDetect.Frontend.Java
{
	public class Token : Frontend.Token
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
				TokenConstants.J_PACKAGE => "PACKAGE",
				TokenConstants.J_IMPORT => "IMPORT",
				TokenConstants.J_CLASS_BEGIN => "CLASS{",
				TokenConstants.J_CLASS_END => "}CLASS",
				TokenConstants.J_METHOD_BEGIN => "METHOD{",
				TokenConstants.J_METHOD_END => "}METHOD",
				TokenConstants.J_VARDEF => "VARDEF",
				TokenConstants.J_SYNC_BEGIN => "SYNC{",
				TokenConstants.J_SYNC_END => "}SYNC",
				TokenConstants.J_DO_BEGIN => "DO{",
				TokenConstants.J_DO_END => "}DO",
				TokenConstants.J_WHILE_BEGIN => "WHILE{",
				TokenConstants.J_WHILE_END => "}WHILE",
				TokenConstants.J_FOR_BEGIN => "FOR{",
				TokenConstants.J_FOR_END => "}FOR",
				TokenConstants.J_SWITCH_BEGIN => "SWITCH{",
				TokenConstants.J_SWITCH_END => "}SWITCH",
				TokenConstants.J_CASE => "CASE",
				TokenConstants.J_TRY_BEGIN => "TRY{",
				TokenConstants.J_CATCH_BEGIN => "CATCH{",
				TokenConstants.J_CATCH_END => "}CATCH",
				TokenConstants.J_FINALLY => "FINALLY",
				TokenConstants.J_IF_BEGIN => "IF{",
				TokenConstants.J_ELSE => "ELSE",
				TokenConstants.J_IF_END => "}IF",
				TokenConstants.J_COND => "COND",
				TokenConstants.J_BREAK => "BREAK",
				TokenConstants.J_CONTINUE => "CONTINUE",
				TokenConstants.J_RETURN => "RETURN",
				TokenConstants.J_THROW => "THROW",
				TokenConstants.J_IN_CLASS_BEGIN => "INCLASS{",
				TokenConstants.J_IN_CLASS_END => "}INCLASS",
				TokenConstants.J_APPLY => "APPLY",
				TokenConstants.J_NEWCLASS => "NEWCLASS",
				TokenConstants.J_NEWARRAY => "NEWARRAY",
				TokenConstants.J_ASSIGN => "ASSIGN",
				TokenConstants.J_INTERFACE_BEGIN => "INTERF{",
				TokenConstants.J_INTERFACE_END => "}INTERF",
				TokenConstants.J_CONSTR_BEGIN => "CONSTR{",
				TokenConstants.J_CONSTR_END => "}CONSTR",
				TokenConstants.J_INIT_BEGIN => "INIT{",
				TokenConstants.J_INIT_END => "}INIT",
				TokenConstants.J_VOID => "VOID",
				TokenConstants.J_ARRAY_INIT_BEGIN => "ARRINIT{",
				TokenConstants.J_ARRAY_INIT_END => "ARRINIT}",
				TokenConstants.J_ENUM_BEGIN => "ENUM{",
				TokenConstants.J_ENUM_CLASS_BEGIN => "ENUM_CLA",
				TokenConstants.J_ENUM_END => "}ENUM",
				TokenConstants.J_GENERIC => "GENERIC",
				TokenConstants.J_ASSERT => "ASSERT",
				TokenConstants.J_ANNO => "ANNO",
				TokenConstants.J_ANNO_MARKER => "ANNOMARK",
				TokenConstants.J_ANNO_M_BEGIN => "ANNO_M{",
				TokenConstants.J_ANNO_M_END => "}ANNO_M",
				TokenConstants.J_ANNO_T_BEGIN => "ANNO_T{",
				TokenConstants.J_ANNO_T_END => "}ANNO_T",
				TokenConstants.J_ANNO_C_BEGIN => "ANNO_C{",
				TokenConstants.J_ANNO_C_END => "}ANNO_C",
				TokenConstants.J_MODULE_BEGIN => "MODULE{",
				TokenConstants.J_MODULE_END => "}MODULE",
				TokenConstants.J_EXPORTS => "EXPORTS",
				TokenConstants.J_PROVIDES => "PROVIDES",
				TokenConstants.J_REQUIRES => "REQUIRES",
				TokenConstants.J_TRY_WITH_RESOURCE => "TRY_RES",
				_ => "<UNBEKANNT>",
			};
		}
    }
}
