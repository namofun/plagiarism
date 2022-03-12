using Antlr4.Runtime;

namespace Xylab.PlagiarismDetect.Frontend.Csharp
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
				TokenConstants.NAMESPACE_BEGIN => "NAMESPACE{",
				TokenConstants.NAMESPACE_END => "}NAMESPACE",
				TokenConstants.CLASS_BEGIN => "CLASS{",
				TokenConstants.CLASS_END => "}CLASS",
				TokenConstants.METHOD_BEGIN => "METHOD{",
				TokenConstants.METHOD_END => "}METHOD",
				TokenConstants.VARDEF => "VARDEF",
				TokenConstants.LOCK_BEGIN => "LOCK{",
				TokenConstants.LOCK_END => "}LOCK",
				TokenConstants.DO_BEGIN => "DO{",
				TokenConstants.DO_END => "}DO",
				TokenConstants.WHILE_BEGIN => "WHILE{",
				TokenConstants.WHILE_END => "}WHILE",
				TokenConstants.FOR_BEGIN => "FOR{",
				TokenConstants.FOR_END => "}FOR",
				TokenConstants.SWITCH_BEGIN => "SWITCH{",
				TokenConstants.SWITCH_END => "}SWITCH",
				TokenConstants.CASE => "CASE",
				TokenConstants.TRY_BEGIN => "TRY{",
				TokenConstants.CATCH_BEGIN => "CATCH{",
				TokenConstants.CATCH_END => "}CATCH",
				TokenConstants.FINALLY => "FINALLY",
				TokenConstants.TRY_END => "}TRY",
				TokenConstants.IF_BEGIN => "IF{",
				TokenConstants.ELSE => "ELSE",
				TokenConstants.IF_END => "}IF",
				TokenConstants.COND => "COND",
				TokenConstants.BREAK => "BREAK",
				TokenConstants.CONTINUE => "CONTINUE",
				TokenConstants.RETURN => "RETURN",
				TokenConstants.THROW => "THROW",
				TokenConstants.INTERFACE_BEGIN => "INTERF{",
				TokenConstants.INTERFACE_END => "}INTERF",
				TokenConstants.ENUM_BEGIN => "ENUM{",
				TokenConstants.ENUM_END => "}ENUM",
				TokenConstants.UNSAFE => "UNSAFE",
				TokenConstants.LAMBDA => "LAMBDA",
				TokenConstants.GENERIC => "GENERIC",
				TokenConstants.NEW_ARRAY => "NEWARRAY",
				TokenConstants.NEW_OBJECT => "NEWOBJ",
				TokenConstants.USING_RESOURCE => "USING",
				TokenConstants.ASSIGN => "ASSIGN",
				TokenConstants.METHOD_INVOCATION => "CALL",
				TokenConstants.STRUCT_BEGIN => "STRUCT{",
				TokenConstants.STRUCT_END => "}STRUCT",
				TokenConstants.ATTRIBUTE_BEGIN => "ATTR{",
				TokenConstants.ATTRIBUTE_END => "ATTR}",
				TokenConstants.GENERIC_CONSTRAINT => "WHERE",
				TokenConstants.PATTERN_MATCHING => "PATTERN",
				TokenConstants.PROPERTY_BEGIN => "PROPERTY{",
				TokenConstants.PROPERTY_END => "}PROPERTY",
				TokenConstants.EVENT_BEGIN => "EVENT{",
				TokenConstants.EVENT_END => "}EVENT",
				TokenConstants.FIXED_POINTER => "FIXED",
				TokenConstants.DELEGATE_DECL => "DELEGATE",
				TokenConstants.LINQ_QUERY_BODY => "LINQ",
				TokenConstants.RANGE => "RANGE",
				TokenConstants.FOREACH_BEGIN => "FOREACH{",
				TokenConstants.FOREACH_END => "}FOREACH",
				_ => "<UNBEKANNT>",
			};
		}
    }
}
