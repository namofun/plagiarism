using Antlr4.Grammar.Cpp;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Runtime.CompilerServices;

namespace Plag.Frontend.Cpp
{
    public class JplagListener : CPP14BaseListener
    {
        public Structure Structure { get; }

        public int Errors { get; private set; }

        public JplagListener(Structure structure)
        {
            Structure = structure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Act(TokenConstants token, IToken symbol)
        {
            if (token == TokenConstants.NUM_DIFF_TOKENS) return false;
            Structure.AddToken(new Token(token, symbol.Line, symbol.StartIndex, symbol.StopIndex + 1));
            return true;
        }

        public override void EnterRightShiftAssign(CPP14Parser.RightShiftAssignContext context)
        {
            Act(TokenConstants.C_ASSIGN, context.Start);
        }

        public override void VisitTerminal(ITerminalNode node)
        {
            Act(node.Symbol.Type switch
            {
                CPP14Lexer.LeftBrace => TokenConstants.C_BLOCK_BEGIN,
                CPP14Lexer.RightBrace => TokenConstants.C_BLOCK_END,
                CPP14Lexer.DoubleColon => TokenConstants.C_SCOPE,
                CPP14Lexer.Question => TokenConstants.C_QUESTIONMARK,
                CPP14Lexer.Ellipsis => TokenConstants.C_ELLIPSIS,
                CPP14Lexer.Assign => TokenConstants.C_ASSIGN,
                CPP14Lexer.AndAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.DivAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.LeftShiftAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.MinusAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.ModAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.OrAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.PlusAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.StarAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.XorAssign => TokenConstants.C_ASSIGN,
                CPP14Lexer.PlusPlus => TokenConstants.C_ASSIGN,
                CPP14Lexer.MinusMinus => TokenConstants.C_ASSIGN,
                CPP14Lexer.Auto => TokenConstants.C_AUTO,
                CPP14Lexer.Break => TokenConstants.C_BREAK,
                CPP14Lexer.Case => TokenConstants.C_CASE,
                CPP14Lexer.Catch => TokenConstants.C_CATCH,
                CPP14Lexer.Char => TokenConstants.C_CHAR,
                CPP14Lexer.Const => TokenConstants.C_CONST,
                CPP14Lexer.Continue => TokenConstants.C_CONTINUE,
                CPP14Lexer.Default => TokenConstants.C_DEFAULT,
                CPP14Lexer.Delete => TokenConstants.C_DELETE,
                CPP14Lexer.Do => TokenConstants.C_DO,
                CPP14Lexer.Double => TokenConstants.C_DOUBLE,
                CPP14Lexer.Else => TokenConstants.C_ELSE,
                CPP14Lexer.Enum => TokenConstants.C_ENUM,
                CPP14Lexer.Extern => TokenConstants.C_EXTERN,
                CPP14Lexer.Float => TokenConstants.C_FLOAT,
                CPP14Lexer.For => TokenConstants.C_FOR,
                CPP14Lexer.Friend => TokenConstants.C_FRIEND,
                CPP14Lexer.Goto => TokenConstants.C_GOTO,
                CPP14Lexer.If => TokenConstants.C_IF,
                CPP14Lexer.Inline => TokenConstants.C_INLINE,
                CPP14Lexer.Int => TokenConstants.C_INT,
                CPP14Lexer.Long => TokenConstants.C_LONG,
                CPP14Lexer.New => TokenConstants.C_NEW,
                CPP14Lexer.Private => TokenConstants.C_PRIVATE,
                CPP14Lexer.Protected => TokenConstants.C_PROTECTED,
                CPP14Lexer.Public => TokenConstants.C_PUBLIC,
                CPP14Lexer.Register => TokenConstants.C_REGISTER,
                CPP14Lexer.Return => TokenConstants.C_RETURN,
                CPP14Lexer.Short => TokenConstants.C_SHORT,
                CPP14Lexer.Signed => TokenConstants.C_SIGNED,
                CPP14Lexer.SizeOf => TokenConstants.C_SIZEOF,
                CPP14Lexer.Static => TokenConstants.C_STATIC,
                CPP14Lexer.Struct => TokenConstants.C_STRUCT,
                CPP14Lexer.Class => TokenConstants.C_CLASS,
                CPP14Lexer.Switch => TokenConstants.C_SWITCH,
                CPP14Lexer.Template => TokenConstants.C_TEMPLATE,
                CPP14Lexer.This => TokenConstants.C_THIS,
                CPP14Lexer.Try => TokenConstants.C_TRY,
                CPP14Lexer.TypeDef => TokenConstants.C_TYPEDEF,
                CPP14Lexer.Union => TokenConstants.C_UNION,
                CPP14Lexer.Unsigned => TokenConstants.C_UNSIGNED,
                CPP14Lexer.Virtual => TokenConstants.C_VIRTUAL,
                CPP14Lexer.Void => TokenConstants.C_VOID,
                CPP14Lexer.Volatile => TokenConstants.C_VOLANTILE,
                CPP14Lexer.While => TokenConstants.C_WHILE,
                CPP14Lexer.Operator => TokenConstants.C_OPERATOR,
                CPP14Lexer.Throw => TokenConstants.C_THROW,
                CPP14Lexer.Eof => TokenConstants.FILE_END,
                CPP14Lexer.Null => TokenConstants.C_NULL,
                CPP14Lexer.NullPtr => TokenConstants.C_NULL,
                _ => TokenConstants.NUM_DIFF_TOKENS
            },
            node.Symbol);
        }
    }
}
