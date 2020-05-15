// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

parser grammar CSharpPreprocessorParser;

options { tokenVocab=CSharpLexer; }

@parser::header { using System.Linq; }

@parser::members
{Stack<bool> conditions = new Stack<bool>(new bool[] { true });
public HashSet<string> ConditionalSymbols = new HashSet<string>() { "DEBUG" };}

preprocessorDirective returns [bool value]
	: DEFINE CONDITIONAL_SYMBOL directiveNewLineOrSharp{ ConditionalSymbols.Add($CONDITIONAL_SYMBOL.text);
	   $value = conditions.All(c => c); } #preprocessorDeclaration

	| UNDEF CONDITIONAL_SYMBOL directiveNewLineOrSharp{ ConditionalSymbols.Remove($CONDITIONAL_SYMBOL.text);
	   $value = conditions.All(c => c); } #preprocessorDeclaration

	| IF expr=preprocessorExpression directiveNewLineOrSharp
	  { $value = $expr.value == "true" && conditions.All(c => c); conditions.Push($expr.value == "true"); }
	  #preprocessorConditional

	| ELIF expr=preprocessorExpression directiveNewLineOrSharp
	  { if (!conditions.Peek()) { conditions.Pop(); $value = $expr.value == "true" && conditions.All(c => c);
	     conditions.Push($expr.value == "true"); } else $value = false; }
	     #preprocessorConditional

	| ELSE directiveNewLineOrSharp
	  { if (!conditions.Peek()) { conditions.Pop(); $value = true && conditions.All(c => c); conditions.Push(true); }
	    else $value = false; }    #preprocessorConditional

	| ENDIF directiveNewLineOrSharp             { conditions.Pop(); $value = conditions.Peek(); }
	   #preprocessorConditional
	| LINE (DIGITS STRING? | DEFAULT | DIRECTIVE_HIDDEN) directiveNewLineOrSharp { $value = conditions.All(c => c); }
	   #preprocessorLine

	| ERROR TEXT directiveNewLineOrSharp       { $value = conditions.All(c => c); }   #preprocessorDiagnostic

	| WARNING TEXT directiveNewLineOrSharp     { $value = conditions.All(c => c); }   #preprocessorDiagnostic

	| REGION TEXT? directiveNewLineOrSharp      { $value = conditions.All(c => c); }   #preprocessorRegion

	| ENDREGION TEXT? directiveNewLineOrSharp  { $value = conditions.All(c => c); }   #preprocessorRegion

	| PRAGMA TEXT directiveNewLineOrSharp      { $value = conditions.All(c => c); }   #preprocessorPragma

	| NULLABLE TEXT directiveNewLineOrSharp      { $value = conditions.All(c => c); }   #preprocessorNullable
	;

directiveNewLineOrSharp
    : DIRECTIVE_NEW_LINE
    | EOF
    ;

preprocessorExpression returns [string value]
	: TRUE                                 { $value = "true"; }
	| FALSE                                { $value = "false"; }
	| CONDITIONAL_SYMBOL                   { $value = ConditionalSymbols.Contains($CONDITIONAL_SYMBOL.text) ? "true" : "false"; }
	| OPEN_PARENS expr=preprocessorExpression CLOSE_PARENS { $value = $expr.value; }
	| BANG expr=preprocessorExpression     { $value = $expr.value == "true" ? "false" : "true"; }
	| expr1=preprocessorExpression OP_EQ expr2=preprocessorExpression
	  { $value = ($expr1.value == $expr2.value ? "true" : "false"); }
	| expr1=preprocessorExpression OP_NE expr2=preprocessorExpression
	  { $value = ($expr1.value != $expr2.value ? "true" : "false"); }
	| expr1=preprocessorExpression OP_AND expr2=preprocessorExpression
	  { $value = ($expr1.value == "true" && $expr2.value == "true" ? "true" : "false"); }
	| expr1=preprocessorExpression OP_OR expr2=preprocessorExpression
	  { $value = ($expr1.value == "true" || $expr2.value == "true" ? "true" : "false"); }
	;