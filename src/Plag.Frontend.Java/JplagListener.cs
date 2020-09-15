using Antlr4.Grammar.Java;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Runtime.CompilerServices;
using static Antlr4.Grammar.Java.Java9Parser;

namespace Plag.Frontend.Java
{
    public class JplagListener : Java9BaseListener
    {
        public Structure Structure { get; }

        public int Errors { get; private set; }

        public JplagListener(Structure structure)
        {
            Structure = structure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Act(TokenConstants token, IToken symbol, IToken symbol2 = null)
        {
            if (token == TokenConstants.NUM_DIFF_TOKENS) return false;
            Structure.AddToken(new Token(token, symbol.Line, symbol.StartIndex, (symbol2?.StopIndex ?? symbol.StopIndex) + 1, Structure.FileId));
            return true;
        }

        public override void EnterReturnStatement(ReturnStatementContext context)
            => Act(TokenConstants.J_RETURN, context.Start);
        public override void EnterVariableDeclarator(VariableDeclaratorContext context)
            => Act(TokenConstants.J_VARDEF, context.Start);
        public override void EnterConstantDeclaration(ConstantDeclarationContext context)
            => Act(TokenConstants.J_VARDEF, context.Start);
        public override void EnterExplicitConstructorInvocation(ExplicitConstructorInvocationContext context)
            => Act(TokenConstants.J_APPLY, context.Start);

        public override void EnterAnnotationTypeDeclaration(AnnotationTypeDeclarationContext context)
            => Act(TokenConstants.J_ANNO_T_BEGIN, context.Start);
        public override void ExitAnnotationTypeDeclaration(AnnotationTypeDeclarationContext context)
            => Act(TokenConstants.J_ANNO_T_END, context.Stop);
        public override void EnterAnnotation(AnnotationContext context)
            => Act(TokenConstants.J_ANNO, context.Start);

        private void EnterClassInstanceCreation(ParserRuleContext context)
        {
            Act(TokenConstants.J_NEWCLASS, context.Start);
            var context2 = context.GetRuleContext<TypeArgumentsOrDiamondContext>(0);
            if (context2 != null)
                Act(TokenConstants.J_GENERIC, context2.Start);
            var context3 = context.GetRuleContext<ClassBodyContext>(0);
            if (context3 != null)
                Act(TokenConstants.J_IN_CLASS_BEGIN, context3.Start);
        }

        private void ExitClassInstanceCreation(ParserRuleContext context)
        {
            if (context.GetRuleContext<ClassBodyContext>(0) != null)
                Act(TokenConstants.J_IN_CLASS_END, context.Stop);
        }

        public override void EnterClassInstanceCreationExpression(ClassInstanceCreationExpressionContext context)
            => EnterClassInstanceCreation(context);
        public override void EnterClassInstanceCreationExpression_lfno_primary(ClassInstanceCreationExpression_lfno_primaryContext context)
            => EnterClassInstanceCreation(context);
        public override void EnterClassInstanceCreationExpression_lf_primary(ClassInstanceCreationExpression_lf_primaryContext context)
            => EnterClassInstanceCreation(context);
        public override void ExitClassInstanceCreationExpression(ClassInstanceCreationExpressionContext context)
            => ExitClassInstanceCreation(context);
        public override void ExitClassInstanceCreationExpression_lfno_primary(ClassInstanceCreationExpression_lfno_primaryContext context)
            => ExitClassInstanceCreation(context);
        public override void ExitClassInstanceCreationExpression_lf_primary(ClassInstanceCreationExpression_lf_primaryContext context)
            => ExitClassInstanceCreation(context);

        public override void EnterNormalClassDeclaration(NormalClassDeclarationContext context)
            => Act(TokenConstants.J_CLASS_BEGIN, context.Start);
        public override void ExitNormalClassDeclaration(NormalClassDeclarationContext context)
            => Act(TokenConstants.J_CLASS_END, context.Stop);
        public override void EnterNormalInterfaceDeclaration(NormalInterfaceDeclarationContext context)
            => Act(TokenConstants.J_INTERFACE_BEGIN, context.Start);
        public override void ExitNormalInterfaceDeclaration(NormalInterfaceDeclarationContext context)
            => Act(TokenConstants.J_INTERFACE_END, context.Stop);
        public override void EnterMethodDeclaration(MethodDeclarationContext context)
            => Act(TokenConstants.J_METHOD_BEGIN, context.Start);
        public override void ExitMethodDeclaration(MethodDeclarationContext context)
            => Act(TokenConstants.J_METHOD_END, context.Stop);
        public override void EnterConstructorDeclaration(ConstructorDeclarationContext context)
            => Act(TokenConstants.J_CONSTR_BEGIN, context.Start);
        public override void ExitConstructorDeclaration(ConstructorDeclarationContext context)
            => Act(TokenConstants.J_CONSTR_END, context.Stop);
        public override void EnterEnumDeclaration(EnumDeclarationContext context)
            => Act(TokenConstants.J_ENUM_BEGIN, context.Start);
        public override void ExitEnumDeclaration(EnumDeclarationContext context)
            => Act(TokenConstants.J_ENUM_END, context.Stop);
        public override void ExitEnumConstantList(EnumConstantListContext context)
            => Act(TokenConstants.J_ENUM_CLASS_BEGIN, context.Stop);
        public override void EnterStaticInitializer(StaticInitializerContext context)
            => Act(TokenConstants.J_INIT_BEGIN, context.Start);
        public override void ExitStaticInitializer(StaticInitializerContext context)
            => Act(TokenConstants.J_INIT_END, context.Stop);

        public override void EnterImportDeclaration(ImportDeclarationContext context)
            => Act(TokenConstants.J_IMPORT, context.Start);
        // TODO: if (!ctx.annotation().isEmpty()) { do nothing }
        public override void EnterPackageDeclaration(PackageDeclarationContext context)
            => Act(TokenConstants.J_PACKAGE, context.Start);

        public override void EnterVariableInitializer(VariableInitializerContext context)
            => Act(context.Parent is ArrayInitializerContext ? TokenConstants.NUM_DIFF_TOKENS : TokenConstants.J_ASSIGN, context.Start);

        public override void EnterMethodInvocation(MethodInvocationContext context)
            => Act(TokenConstants.J_APPLY, context.Start);
        public override void EnterMethodInvocation_lfno_primary(MethodInvocation_lfno_primaryContext context)
            => Act(TokenConstants.J_APPLY, context.Start);
        public override void EnterMethodInvocation_lf_primary(MethodInvocation_lf_primaryContext context)
            => Act(TokenConstants.J_APPLY, context.Start);

        public override void EnterConditionalExpression(ConditionalExpressionContext context)
            => Act(TokenConstants.J_COND, context.Start);
        public override void EnterAssignment(AssignmentContext context)
            => Act(TokenConstants.J_ASSIGN, context.Start);
        public override void EnterMethodInvocationGenericExplicit(MethodInvocationGenericExplicitContext context)
            => Act(TokenConstants.J_APPLY, context.Start);

        public override void EnterArrayCreationExpression(ArrayCreationExpressionContext context)
            => Act(TokenConstants.J_NEWARRAY, context.Start);

        public override void EnterTryStatement(TryStatementContext context)
            => Act(TokenConstants.J_TRY_BEGIN, context.Start);
        public override void EnterFinallyStmt(FinallyStmtContext context)
            => Act(TokenConstants.J_FINALLY, context.Start);
        public override void EnterCatchClause(CatchClauseContext context)
            => Act(TokenConstants.J_CATCH_BEGIN, context.Start);
        public override void ExitCatchClause(CatchClauseContext context)
            => Act(TokenConstants.J_CATCH_END, context.Stop);
        public override void EnterTryWithResourcesStatement(TryWithResourcesStatementContext context)
            => Act(TokenConstants.J_TRY_WITH_RESOURCE, context.Start);
        public override void EnterThrowStatement(ThrowStatementContext context)
            => Act(TokenConstants.J_THROW, context.Start);

        public override void EnterArrayInitializer(ArrayInitializerContext context)
            => Act(TokenConstants.J_ARRAY_INIT_BEGIN, context.Start);
        public override void ExitArrayInitializer(ArrayInitializerContext context)
            => Act(TokenConstants.J_ARRAY_INIT_END, context.Stop);

        public override void EnterModuleDeclaration(ModuleDeclarationContext context)
            => Act(TokenConstants.J_MODULE_BEGIN, context.Start);
        public override void ExitModuleDeclaration(ModuleDeclarationContext context)
            => Act(TokenConstants.J_MODULE_END, context.Stop);
        public override void EnterModuProvidesStmt(ModuProvidesStmtContext context)
            => Act(TokenConstants.J_PROVIDES, context.Start);
        public override void EnterModuRequiresStmt(ModuRequiresStmtContext context)
            => Act(TokenConstants.J_REQUIRES, context.Start);
        public override void EnterModuExportStmt(ModuExportStmtContext context)
            => Act(TokenConstants.J_EXPORTS, context.Start);

        public override void EnterSynchronizedStatement(SynchronizedStatementContext context)
            => Act(TokenConstants.J_SYNC_BEGIN, context.Start);
        public override void ExitSynchronizedStatement(SynchronizedStatementContext context)
            => Act(TokenConstants.J_SYNC_END, context.Stop);

        public override void EnterAssertStatement(AssertStatementContext context)
            => Act(TokenConstants.J_ASSERT, context.Start);
        public override void EnterSwitchStatement(SwitchStatementContext context)
            => Act(TokenConstants.J_SWITCH_BEGIN, context.Start);
        public override void ExitSwitchStatement(SwitchStatementContext context)
            => Act(TokenConstants.J_SWITCH_END, context.Stop);
        public override void ExitSwitchLabel(SwitchLabelContext context)
            => Act(TokenConstants.J_CASE, context.Stop);
        public override void EnterIfThenElseStatement(IfThenElseStatementContext context)
            => Act(TokenConstants.J_IF_BEGIN, context.Start);
        public override void EnterIfThenElseStatementNoShortIf(IfThenElseStatementNoShortIfContext context)
            => Act(TokenConstants.J_IF_BEGIN, context.Start);
        public override void EnterIfThenStatement(IfThenStatementContext context)
            => Act(TokenConstants.J_IF_BEGIN, context.Start);
        public override void ExitIfThenElseStatement(IfThenElseStatementContext context)
            => Act(TokenConstants.J_IF_END, context.Stop);
        public override void ExitIfThenElseStatementNoShortIf(IfThenElseStatementNoShortIfContext context)
            => Act(TokenConstants.J_IF_END, context.Stop);
        public override void ExitIfThenStatement(IfThenStatementContext context)
            => Act(TokenConstants.J_IF_END, context.Stop);

        public override void EnterForStatement(ForStatementContext context)
            => Act(TokenConstants.J_FOR_BEGIN, context.Start);
        public override void EnterForStatementNoShortIf(ForStatementNoShortIfContext context)
            => Act(TokenConstants.J_FOR_BEGIN, context.Start);
        public override void ExitForStatement(ForStatementContext context)
            => Act(TokenConstants.J_FOR_END, context.Stop);
        public override void ExitForStatementNoShortIf(ForStatementNoShortIfContext context)
            => Act(TokenConstants.J_FOR_END, context.Stop);

        public override void EnterContinueStatement(ContinueStatementContext context)
            => Act(TokenConstants.J_CONTINUE, context.Start);
        public override void EnterBreakStatement(BreakStatementContext context)
            => Act(TokenConstants.J_BREAK, context.Start);

        public override void EnterWhileStatement(WhileStatementContext context)
            => Act(TokenConstants.J_WHILE_BEGIN, context.Start);
        public override void ExitWhileStatement(WhileStatementContext context)
            => Act(TokenConstants.J_WHILE_END, context.Stop);
        public override void EnterWhileStatementNoShortIf(WhileStatementNoShortIfContext context)
            => Act(TokenConstants.J_WHILE_BEGIN, context.Start);
        public override void ExitWhileStatementNoShortIf(WhileStatementNoShortIfContext context)
            => Act(TokenConstants.J_WHILE_END, context.Stop);
        public override void EnterDoStatement(DoStatementContext context)
            => Act(TokenConstants.J_DO_BEGIN, context.Start);
        public override void ExitDoStatement(DoStatementContext context)
            => Act(TokenConstants.J_DO_END, context.Stop);
        


        public override void VisitTerminal(ITerminalNode node)
            => Act(node.Symbol.Text == "else" ? TokenConstants.J_ELSE : TokenConstants.NUM_DIFF_TOKENS, node.Symbol);
    }
}
