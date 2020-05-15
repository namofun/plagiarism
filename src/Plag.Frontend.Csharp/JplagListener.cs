using Antlr4.Grammar.Csharp;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Runtime.CompilerServices;
using static Antlr4.Grammar.Csharp.CSharpParser;

namespace Plag.Frontend.Csharp
{
    public class JplagListener : CSharpParserBaseListener
    {
        public Structure Structure { get; }

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

        public override void EnterNamespaceDeclaration(NamespaceDeclarationContext context)
            => Act(TokenConstants.NAMESPACE_BEGIN, context.Start);

        public override void ExitNamespaceDeclaration(NamespaceDeclarationContext context)
            => Act(TokenConstants.NAMESPACE_END, context.Stop);

        public override void EnterClassDefinition(ClassDefinitionContext context)
            => Act(TokenConstants.CLASS_BEGIN, context.Start);

        public override void ExitClassDefinition(ClassDefinitionContext context)
            => Act(TokenConstants.CLASS_END, context.Stop);

        public override void EnterMethodDeclaration(MethodDeclarationContext context)
            => Act(TokenConstants.METHOD_BEGIN, context.Start);

        public override void ExitMethodDeclaration(MethodDeclarationContext context)
            => Act(TokenConstants.METHOD_END, context.Start);

        public override void EnterLocalVariableDeclaration(LocalVariableDeclarationContext context)
            => Act(TokenConstants.VARDEF, context.Start);

        public override void EnterLockStatement(LockStatementContext context)
            => Act(TokenConstants.LOCK_BEGIN, context.Start);

        public override void ExitLockStatement(LockStatementContext context)
            => Act(TokenConstants.LOCK_END, context.Stop);

        public override void EnterDoStatement(DoStatementContext context)
            => Act(TokenConstants.DO_BEGIN, context.Start);

        public override void ExitDoStatement(DoStatementContext context)
            => Act(TokenConstants.DO_END, context.Stop);

        public override void EnterWhileStatement(WhileStatementContext context)
            => Act(TokenConstants.WHILE_BEGIN, context.Start);

        public override void ExitWhileStatement(WhileStatementContext context)
            => Act(TokenConstants.WHILE_END, context.Stop);

        public override void EnterForStatement(ForStatementContext context)
            => Act(TokenConstants.FOR_BEGIN, context.Start);

        public override void ExitForStatement(ForStatementContext context)
            => Act(TokenConstants.FOR_END, context.Stop);

        public override void EnterForeachStatement(ForeachStatementContext context)
            => Act(TokenConstants.FOREACH_BEGIN, context.Start);

        public override void ExitForeachStatement(ForeachStatementContext context)
            => Act(TokenConstants.FOREACH_END, context.Stop);

        public override void EnterSwitchStatement(SwitchStatementContext context)
            => Act(TokenConstants.SWITCH_BEGIN, context.Start);

        public override void ExitSwitchStatement(SwitchStatementContext context)
            => Act(TokenConstants.SWITCH_END, context.Stop);

        public override void EnterSwitchExpression(SwitchExpressionContext context)
            => Act(TokenConstants.SWITCH_BEGIN, context.Start);

        public override void ExitSwitchExpression(SwitchExpressionContext context)
            => Act(TokenConstants.SWITCH_END, context.Stop);

        public override void EnterSwitchLabel(SwitchLabelContext context)
            => Act(TokenConstants.CASE, context.Start);

        public override void EnterSwitchExpressionArm(SwitchExpressionArmContext context)
            => Act(TokenConstants.CASE, context.Start);

        public override void EnterTryStatement(TryStatementContext context)
            => Act(TokenConstants.TRY_BEGIN, context.Start);

        public override void EnterFinallyClause(FinallyClauseContext context)
            => Act(TokenConstants.FINALLY, context.Start);

        public override void ExitTryStatement(TryStatementContext context)
            => Act(TokenConstants.TRY_END, context.Stop);

        public override void EnterSpecificCatchClause(SpecificCatchClauseContext context)
            => Act(TokenConstants.CATCH_BEGIN, context.Start);

        public override void ExitSpecificCatchClause(SpecificCatchClauseContext context)
            => Act(TokenConstants.CATCH_END, context.Start);

        public override void EnterGeneralCatchClause(GeneralCatchClauseContext context)
            => Act(TokenConstants.CATCH_BEGIN, context.Start);

        public override void ExitGeneralCatchClause(GeneralCatchClauseContext context)
            => Act(TokenConstants.CATCH_END, context.Stop);

        public override void EnterConditionalExpression(ConditionalExpressionContext context)
            => Act(TokenConstants.COND, context.Start);

        public override void EnterIfStatement(IfStatementContext context)
            => Act(TokenConstants.IF_BEGIN, context.Start);

        public override void ExitIfStatement(IfStatementContext context)
            => Act(TokenConstants.IF_END, context.Stop);

        public override void EnterBreakStatement(BreakStatementContext context)
            => Act(TokenConstants.BREAK, context.Start);

        public override void EnterContinueStatement(ContinueStatementContext context)
            => Act(TokenConstants.CONTINUE, context.Start);

        public override void EnterReturnStatement(ReturnStatementContext context)
            => Act(TokenConstants.RETURN, context.Start);

        public override void EnterThrowStatement(ThrowStatementContext context)
            => Act(TokenConstants.THROW, context.Start);

        public override void EnterThrowExpression(ThrowExpressionContext context)
            => Act(TokenConstants.THROW, context.Start);

        public override void EnterInterfaceDefinition(InterfaceDefinitionContext context)
            => Act(TokenConstants.INTERFACE_BEGIN, context.Start);

        public override void ExitInterfaceDefinition(InterfaceDefinitionContext context)
            => Act(TokenConstants.INTERFACE_END, context.Stop);

        public override void EnterEnumDefinition(EnumDefinitionContext context)
            => Act(TokenConstants.ENUM_BEGIN, context.Start);

        public override void ExitEnumDefinition(EnumDefinitionContext context)
            => Act(TokenConstants.ENUM_END, context.Stop);

        public override void EnterUnsafeStatement(UnsafeStatementContext context)
            => Act(TokenConstants.UNSAFE, context.Start);

        public override void EnterFixedStatement(FixedStatementContext context)
            => Act(TokenConstants.FIXED_POINTER, context.Start);

        public override void EnterLambdaExpression(LambdaExpressionContext context)
            => Act(TokenConstants.LAMBDA, context.Start);

        public override void EnterTypeParameterConstraintsClauses(TypeParameterConstraintsClausesContext context)
            => Act(TokenConstants.GENERIC_CONSTRAINT, context.Start);

        public override void EnterTypeParameterList(TypeParameterListContext context)
            => Act(TokenConstants.GENERIC, context.Start);

        public override void EnterArrayInitializer(ArrayInitializerContext context)
            => Act(TokenConstants.NEW_ARRAY, context.Start);

        public override void EnterObjectCreationExpressionFull(ObjectCreationExpressionFullContext context)
            => Act(TokenConstants.NEW_OBJECT, context.Start);

        public override void EnterUsingStatement(UsingStatementContext context)
            => Act(TokenConstants.USING_RESOURCE, context.Start);

        public override void EnterAssignment(AssignmentContext context)
            => Act(TokenConstants.ASSIGN, context.Start);

        public override void EnterMethodInvocation(MethodInvocationContext context)
            => Act(TokenConstants.METHOD_INVOCATION, context.Start);

        public override void EnterStructDefinition(StructDefinitionContext context)
            => Act(TokenConstants.STRUCT_BEGIN, context.Start);

        public override void ExitStructDefinition(StructDefinitionContext context)
            => Act(TokenConstants.STRUCT_END, context.Stop);

        public override void EnterGlobalAttributeSection(GlobalAttributeSectionContext context)
            => Act(TokenConstants.ATTRIBUTE_BEGIN, context.Start);

        public override void ExitGlobalAttributeSection(GlobalAttributeSectionContext context)
            => Act(TokenConstants.ATTRIBUTE_END, context.Stop);

        public override void EnterAttributeSection(AttributeSectionContext context)
            => Act(TokenConstants.ATTRIBUTE_BEGIN, context.Start);

        public override void ExitAttributeSection(AttributeSectionContext context)
            => Act(TokenConstants.ATTRIBUTE_END, context.Stop);

        public override void EnterIsType(IsTypeContext context)
            => Act(TokenConstants.PATTERN_MATCHING, context.Start);

        public override void EnterPropertyDeclaration(PropertyDeclarationContext context)
            => Act(TokenConstants.PROPERTY_BEGIN, context.Start);

        public override void ExitPropertyDeclaration(PropertyDeclarationContext context)
            => Act(TokenConstants.PROPERTY_END, context.Stop);

        public override void EnterEventDeclaration(EventDeclarationContext context)
            => Act(TokenConstants.EVENT_BEGIN, context.Start);

        public override void ExitEventDeclaration(EventDeclarationContext context)
            => Act(TokenConstants.EVENT_END, context.Stop);

        public override void EnterDelegateDefinition(DelegateDefinitionContext context)
            => Act(TokenConstants.DELEGATE_DECL, context.Start);

        public override void EnterQueryExpression(QueryExpressionContext context)
            => Act(TokenConstants.LINQ_QUERY_BODY, context.Start);

        public override void EnterRangeExpressionImpl(RangeExpressionImplContext context)
            => Act(TokenConstants.RANGE, context.Start);

        public override void VisitTerminal(ITerminalNode node)
        {
            Act(node.Symbol.Type switch
            {
                CSharpLexer.ELSE => TokenConstants.ELSE,
                _ => TokenConstants.NUM_DIFF_TOKENS,
            },
            node.Symbol);
        }
    }
}
