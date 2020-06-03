using Antlr4.Grammar.Python;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Runtime.CompilerServices;
using static Antlr4.Grammar.Python.Python3Parser;

namespace Plag.Frontend.Python
{
    public class JplagListener : Python3BaseListener
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
            Structure.AddToken(new Token(token, symbol.Line, symbol.StartIndex, symbol.StopIndex + 1, Structure.FileId));
            return true;
        }

        public override void EnterAssertStmt(AssertStmtContext context)
            => Act(TokenConstants.ASSERT, context.Start);
        public override void EnterDecorated(DecoratedContext context)
            => Act(TokenConstants.DEC_BEGIN, context.Start);
        public override void ExitDecorated(DecoratedContext context)
            => Act(TokenConstants.DEC_END, context.Stop);
        public override void EnterRaiseStmt(RaiseStmtContext context)
            => Act(TokenConstants.RAISE, context.Start);
        public override void EnterExceptClause(ExceptClauseContext context)
            => Act(TokenConstants.EXCEPT_BEGIN, context.Start);
        public override void ExitExceptClause(ExceptClauseContext context)
            => Act(TokenConstants.EXCEPT_END, context.Stop);
        public override void EnterDictOrSetMaker(DictOrSetMakerContext context)
            => Act(TokenConstants.ARRAY, context.Start);
        public override void EnterReturnStmt(ReturnStmtContext context)
            => Act(TokenConstants.RETURN, context.Start);
        public override void EnterWhileStmt(WhileStmtContext context)
            => Act(TokenConstants.WHILE_BEGIN, context.Start);
        public override void ExitWhileStmt(WhileStmtContext context)
            => Act(TokenConstants.WHILE_END, context.Start);
        public override void EnterYieldArg(YieldArgContext context)
            => Act(TokenConstants.YIELD, context.Start);
        public override void EnterImportStmt(ImportStmtContext context)
            => Act(TokenConstants.IMPORT, context.Start);
        public override void EnterLambdef(LambdefContext context)
            => Act(TokenConstants.LAMBDA, context.Start);
        public override void EnterTryStmt(TryStmtContext context)
            => Act(TokenConstants.TRY_BEGIN, context.Start);
        public override void EnterBreakStmt(BreakStmtContext context)
            => Act(TokenConstants.BREAK, context.Start);
        public override void EnterTestlistCompArray(TestlistCompArrayContext context)
            => Act(TokenConstants.ARRAY, context.Start);
        public override void EnterTestlistCompLambda(TestlistCompLambdaContext context)
            => Act(TokenConstants.LAMBDA, context.Start);
        public override void EnterIfStmt(IfStmtContext context)
            => Act(TokenConstants.IF_BEGIN, context.Start);
        public override void ExitIfStmt(IfStmtContext context)
            => Act(TokenConstants.IF_END, context.Stop);
        public override void EnterWithStmt(WithStmtContext context)
            => Act(TokenConstants.WITH_BEGIN, context.Start);
        public override void ExitWithStmt(WithStmtContext context)
            => Act(TokenConstants.WITH_END, context.Stop);
        public override void EnterClassDef(ClassDefContext context)
            => Act(TokenConstants.CLASS_BEGIN, context.Start);
        public override void ExitClassDef(ClassDefContext context)
            => Act(TokenConstants.CLASS_END, context.Stop);
        public override void EnterTrailer(TrailerContext context)
            => Act(context.Start.Text == "(" ? TokenConstants.APPLY : TokenConstants.ARRAY, context.Start);
        public override void EnterFuncdef(FuncdefContext context)
            => Act(TokenConstants.METHOD_BEGIN, context.Start);
        public override void ExitFuncdef(FuncdefContext context)
            => Act(TokenConstants.METHOD_END, context.Stop);
        public override void EnterAugAssign(AugAssignContext context)
            => Act(TokenConstants.ASSIGN, context.Start);
        public override void EnterYieldStmt(YieldStmtContext context)
            => Act(TokenConstants.YIELD, context.Start);
        public override void EnterContinueStmt(ContinueStmtContext context)
            => Act(TokenConstants.CONTINUE, context.Start);
        public override void EnterForStmt(ForStmtContext context)
            => Act(TokenConstants.FOR_BEGIN, context.Start);
        public override void ExitForStmt(ForStmtContext context)
            => Act(TokenConstants.FOR_END, context.Stop);
        public override void EnterDelStmt(DelStmtContext context)
            => Act(TokenConstants.DEL, context.Start);

        public override void VisitTerminal(ITerminalNode node)
        {
            if (node.GetText() == "=")
                Act(TokenConstants.ASSIGN, node.Symbol);
            else if (node.GetText() == "finally")
                Act(TokenConstants.FINALLY, node.Symbol);
        }
    }
}
