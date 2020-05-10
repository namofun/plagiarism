/*******************************************************************************
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Camilo Sanchez (Camiloasc1)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ******************************************************************************/
grammar CPP14;
/*Basic concepts*/


translationUnit
   : declarationSeq? EOF
   ;
/*Expressions*/


primaryExpression
   : literal
   | This
   | '(' expression ')'
   | idExpression
   | lambdaExpression
   ;

idExpression
   : unqualifiedId
   | qualifiedId
   ;

unqualifiedId
   : Identifier
   | operatorFunctionId
   | conversionFunctionId
   | literalOperatorId
   | '~' className
   | '~' declTypeSpecifier
   | templateId
   ;

qualifiedId
   : nestedNameSpecifier Template? unqualifiedId
   ;

nestedNameSpecifier
   : '::'
   | theTypeName '::'
   | namespaceName '::'
   | declTypeSpecifier '::'
   | nestedNameSpecifier Identifier '::'
   | nestedNameSpecifier Template? simpleTemplateId '::'
   ;

lambdaExpression
   : lambdaIntroducer lambdaDeclarator? compoundStatement
   ;

lambdaIntroducer
   : '[' lambdaCapture? ']'
   ;

lambdaCapture
   : captureDefault
   | captureList
   | captureDefault ',' captureList
   ;

captureDefault
   : '&'
   | '='
   ;

captureList
   : capture '...'?
   | captureList ',' capture '...'?
   ;

capture
   : simpleCapture
   | initCapture
   ;

simpleCapture
   : Identifier
   | '&' Identifier
   | This
   ;

initCapture
   : Identifier initializer
   | '&' Identifier initializer
   ;

lambdaDeclarator
   : '(' parameterDeclarationClause ')' Mutable? exceptionSpecification? attributeSpecifierSeq? trailingReturnType?
   ;

postfixExpression
   : primaryExpression
   | postfixExpression '[' expression ']'
   | postfixExpression '[' bracedInitList ']'
   | postfixExpression '(' expressionList? ')'
   | simpleTypeSpecifier '(' expressionList? ')'
   | typeNameSpecifier '(' expressionList? ')'
   | simpleTypeSpecifier bracedInitList
   | typeNameSpecifier bracedInitList
   | postfixExpression '.' Template? idExpression
   | postfixExpression '->' Template? idExpression
   | postfixExpression '.' pseudoDestructorName
   | postfixExpression '->' pseudoDestructorName
   | postfixExpression '++'
   | postfixExpression '--'
   | DynamicCast '<' theTypeId '>' '(' expression ')'
   | StaticCast '<' theTypeId '>' '(' expression ')'
   | ReinterpretCast '<' theTypeId '>' '(' expression ')'
   | ConstCast '<' theTypeId '>' '(' expression ')'
   | typeIdOfTheTypeId '(' expression ')'
   | typeIdOfTheTypeId '(' theTypeId ')'
   ;
/*
add a middle layer to eliminate duplicated function declarations
*/


typeIdOfExpr
   : TypeId
   ;

typeIdOfTheTypeId
   : TypeId
   ;

expressionList
   : initializerList
   ;

pseudoDestructorName
   : nestedNameSpecifier? theTypeName '::' '~' theTypeName
   | nestedNameSpecifier Template simpleTemplateId '::' '~' theTypeName
   | nestedNameSpecifier? '~' theTypeName
   | '~' declTypeSpecifier
   ;

unaryExpression
   : postfixExpression
   | '++' castExpression
   | '--' castExpression
   | unaryOperator castExpression
   | SizeOf unaryExpression
   | SizeOf '(' theTypeId ')'
   | SizeOf '...' '(' Identifier ')'
   | AlignOf '(' theTypeId ')'
   | noExceptExpression
   | newExpression
   | deleteExpression
   ;

unaryOperator
   : '|'
   | '*'
   | '&'
   | '+'
   | '!'
   | '~'
   | '-'
   | 'not'
   ;

newExpression
   : '::'? New newPlacement? newTypeId newInitializer?
   | '::'? New newPlacement? '(' theTypeId ')' newInitializer?
   ;

newPlacement
   : '(' expressionList ')'
   ;

newTypeId
   : typeSpecifierSeq newDeclarator?
   ;

newDeclarator
   : ptrOperator newDeclarator?
   | noPtrNewDeclarator
   ;

noPtrNewDeclarator
   : '[' expression ']' attributeSpecifierSeq?
   | noPtrNewDeclarator '[' constantExpression ']' attributeSpecifierSeq?
   ;

newInitializer
   : '(' expressionList? ')'
   | bracedInitList
   ;

deleteExpression
   : '::'? Delete castExpression
   | '::'? Delete '[' ']' castExpression
   ;

noExceptExpression
   : NoExcept '(' expression ')'
   ;

castExpression
   : unaryExpression
   | '(' theTypeId ')' castExpression
   ;

pmExpression
   : castExpression
   | pmExpression '.*' castExpression
   | pmExpression '->*' castExpression
   ;

multiplicativeExpression
   : pmExpression
   | multiplicativeExpression '*' pmExpression
   | multiplicativeExpression '/' pmExpression
   | multiplicativeExpression '%' pmExpression
   ;

additiveExpression
   : multiplicativeExpression
   | additiveExpression '+' multiplicativeExpression
   | additiveExpression '-' multiplicativeExpression
   ;

shiftExpression
   : additiveExpression
   | shiftExpression shiftOperator additiveExpression
   ;

shiftOperator
  : RightShift
  | LeftShift
  ;

relationalExpression
   : shiftExpression
   | relationalExpression '<' shiftExpression
   | relationalExpression '>' shiftExpression
   | relationalExpression '<=' shiftExpression
   | relationalExpression '>=' shiftExpression
   ;

equalityExpression
   : relationalExpression
   | equalityExpression '==' relationalExpression
   | equalityExpression '!=' relationalExpression
   ;

andExpression
   : equalityExpression
   | andExpression '&' equalityExpression
   ;

exclusiveOrExpression
   : andExpression
   | exclusiveOrExpression '^' andExpression
   ;

inclusiveOrExpression
   : exclusiveOrExpression
   | inclusiveOrExpression '|' exclusiveOrExpression
   ;

logicalAndExpression
   : inclusiveOrExpression
   | logicalAndExpression '&&' inclusiveOrExpression
   | logicalAndExpression 'and' inclusiveOrExpression
   ;

logicalOrExpression
   : logicalAndExpression
   | logicalOrExpression '||' logicalAndExpression
   | logicalOrExpression 'or' logicalAndExpression
   ;

conditionalExpression
   : logicalOrExpression
   | logicalOrExpression '?' expression ':' assignmentExpression
   ;

assignmentExpression
   : conditionalExpression
   | logicalOrExpression assignmentOperator initializerClause
   | throwExpression
   ;

assignmentOperator
   : '='
   | '*='
   | '/='
   | '%='
   | '+='
   | '-='
   | RightShiftAssign
   | LeftShiftAssign
   | '&='
   | '^='
   | '|='
   ;

expression
   : assignmentExpression
   | expression ',' assignmentExpression
   ;

constantExpression
   : conditionalExpression
   ;
/*Statements*/


statement
   : labeledStatement
   | attributeSpecifierSeq? expressionStatement
   | attributeSpecifierSeq? compoundStatement
   | attributeSpecifierSeq? selectionStatement
   | attributeSpecifierSeq? iterationStatement
   | attributeSpecifierSeq? jumpStatement
   | declarationStatement
   | attributeSpecifierSeq? tryBlock
   ;

labeledStatement
   : attributeSpecifierSeq? Identifier ':' statement
   | attributeSpecifierSeq? Case constantExpression ':' statement
   | attributeSpecifierSeq? Default ':' statement
   ;

expressionStatement
   : expression? ';'
   ;

compoundStatement
   : '{' statementSeq? '}'
   ;

statementSeq
   : statement
   | statementSeq statement
   ;

selectionStatement
   : If '(' condition ')' statement
   | If '(' condition ')' statement Else statement
   | Switch '(' condition ')' statement
   ;

condition
   : expression
   | attributeSpecifierSeq? declSpecifierSeq declarator '=' initializerClause
   | attributeSpecifierSeq? declSpecifierSeq declarator bracedInitList
   ;

iterationStatement
   : While '(' condition ')' statement
   | Do statement While '(' expression ')' ';'
   | For '(' forInitStatement condition? ';' expression? ')' statement
   | For '(' forRangeDeclaration ':' forRangeInitializer ')' statement
   ;

forInitStatement
   : expressionStatement
   | simpleDeclaration
   ;

forRangeDeclaration
   : attributeSpecifierSeq? declSpecifierSeq declarator
   ;

forRangeInitializer
   : expression
   | bracedInitList
   ;

jumpStatement
   : Break ';'
   | Continue ';'
   | Return expression? ';'
   | Return bracedInitList ';'
   | Goto Identifier ';'
   ;

declarationStatement
   : blockDeclaration
   ;
/*Declarations*/


declarationSeq
   : declaration
   | declarationSeq declaration
   ;

declaration
   : blockDeclaration
   | functionDefinition
   | templateDeclaration
   | explicitInstantiation
   | explicitSpecialization
   | linkageSpecification
   | namespaceDefinition
   | emptyDeclaration
   | attributeDeclaration
   | Directive
   | MultiLineMacro
   ;

blockDeclaration
   : simpleDeclaration
   | asmDefinition
   | namespaceAliasDefinition
   | usingDeclaration
   | usingDirective
   | staticAssertDeclaration
   | aliasDeclaration
   | opaqueEnumDeclaration
   ;

aliasDeclaration
   : Using Identifier attributeSpecifierSeq? '=' theTypeId ';'
   ;

simpleDeclaration
   : declSpecifierSeq? initDeclaratorList? ';'
   | attributeSpecifierSeq declSpecifierSeq? initDeclaratorList ';'
   ;

staticAssertDeclaration
   : StaticAssert '(' constantExpression ',' StringLiteral ')' ';'
   ;

emptyDeclaration
   : ';'
   ;

attributeDeclaration
   : attributeSpecifierSeq ';'
   ;

declSpecifier
   : storageClassSpecifier
   | typeSpecifier
   | functionSpecifier
   | Friend
   | TypeDef
   | Constexpr
   ;

declSpecifierSeq
   : declSpecifier attributeSpecifierSeq?
   | declSpecifier declSpecifierSeq
   ;

storageClassSpecifier
   : Register
   | Static
   | ThreadLocal
   | Extern
   | Mutable
   ;

functionSpecifier
   : Inline
   | Virtual
   | Explicit
   ;

typedefName
   : Identifier
   ;

typeSpecifier
   : trailingTypeSpecifier
   | classSpecifier
   | enumSpecifier
   ;

trailingTypeSpecifier
   : simpleTypeSpecifier
   | elaboratedTypeSpecifier
   | typeNameSpecifier
   | cvQualifier
   ;

typeSpecifierSeq
   : typeSpecifier attributeSpecifierSeq?
   | typeSpecifier typeSpecifierSeq
   ;

trailingTypeSpecifierSeq
   : trailingTypeSpecifier attributeSpecifierSeq?
   | trailingTypeSpecifier trailingTypeSpecifierSeq
   ;

simpleTypeSpecifier
   : nestedNameSpecifier? theTypeName
   | nestedNameSpecifier Template simpleTemplateId
   | Char
   | Char16
   | Char32
   | Wchar
   | Bool
   | Short
   | Int
   | Long
   | Signed
   | Unsigned
   | Float
   | Double
   | Void
   | Auto
   | declTypeSpecifier
   ;

theTypeName
   : className
   | enumName
   | typedefName
   | simpleTemplateId
   ;

declTypeSpecifier
   : DeclType '(' expression ')'
   | DeclType '(' Auto ')'
   ;

elaboratedTypeSpecifier
   : classKey attributeSpecifierSeq? nestedNameSpecifier? Identifier
   | classKey simpleTemplateId
   | classKey nestedNameSpecifier Template? simpleTemplateId
   | Enum nestedNameSpecifier? Identifier
   ;

enumName
   : Identifier
   ;

enumSpecifier
   : enumHead '{' enumeratorList? '}'
   | enumHead '{' enumeratorList ',' '}'
   ;

enumHead
   : enumKey attributeSpecifierSeq? Identifier? enumBase?
   | enumKey attributeSpecifierSeq? nestedNameSpecifier Identifier enumBase?
   ;

opaqueEnumDeclaration
   : enumKey attributeSpecifierSeq? Identifier enumBase? ';'
   ;

enumKey
   : Enum
   | Enum Class
   | Enum Struct
   ;

enumBase
   : ':' typeSpecifierSeq
   ;

enumeratorList
   : enumeratorDefinition
   | enumeratorList ',' enumeratorDefinition
   ;

enumeratorDefinition
   : enumerator
   | enumerator '=' constantExpression
   ;

enumerator
   : Identifier
   ;

namespaceName
   : originalNamespaceName
   | namespaceAlias
   ;

originalNamespaceName
   : Identifier
   ;

namespaceDefinition
   : namedNamespaceDefinition
   | unnamedNamespaceDefinition
   ;

namedNamespaceDefinition
   : originalNamespaceDefinition
   | extensionNamespaceDefinition
   ;

originalNamespaceDefinition
   : Inline? Namespace Identifier '{' namespaceBody '}'
   ;

extensionNamespaceDefinition
   : Inline? Namespace originalNamespaceName '{' namespaceBody '}'
   ;

unnamedNamespaceDefinition
   : Inline? Namespace '{' namespaceBody '}'
   ;

namespaceBody
   : declarationSeq?
   ;

namespaceAlias
   : Identifier
   ;

namespaceAliasDefinition
   : Namespace Identifier '=' qualifiedNamespaceSpecifier ';'
   ;

qualifiedNamespaceSpecifier
   : nestedNameSpecifier? namespaceName
   ;

usingDeclaration
   : Using TypeName? nestedNameSpecifier unqualifiedId ';'
   | Using '::' unqualifiedId ';'
   ;

usingDirective
   : attributeSpecifierSeq? Using Namespace nestedNameSpecifier? namespaceName ';'
   ;

asmDefinition
   : Asm '(' StringLiteral ')' ';'
   ;

linkageSpecification
   : Extern StringLiteral '{' declarationSeq? '}'
   | Extern StringLiteral declaration
   ;

attributeSpecifierSeq
   : attributeSpecifier
   | attributeSpecifierSeq attributeSpecifier
   ;

attributeSpecifier
   : '[' '[' attributeList ']' ']'
   | alignmentSpecifier
   ;

alignmentSpecifier
   : AlignAs '(' theTypeId '...'? ')'
   | AlignAs '(' constantExpression '...'? ')'
   ;

attributeList
   : attribute?
   | attributeList ',' attribute?
   | attribute '...'
   | attributeList ',' attribute '...'
   ;

attribute
   : attributeToken attributeArgumentClause?
   ;

attributeToken
   : Identifier
   | attributeScopedToken
   ;

attributeScopedToken
   : attributeNamespace '::' Identifier
   ;

attributeNamespace
   : Identifier
   ;

attributeArgumentClause
   : '(' balancedTokenSeq ')'
   ;

balancedTokenSeq
   : balancedToken?
   | balancedTokenSeq balancedToken
   ;

balancedToken
   : '(' balancedTokenSeq ')'
   | '[' balancedTokenSeq ']'
   | '{' balancedTokenSeq '}'
   | ~('('|')'|'{'|'}'|'['|']')+
   ;
/*Declarators*/


initDeclaratorList
   : initDeclarator
   | initDeclaratorList ',' initDeclarator
   ;

initDeclarator
   : declarator initializer?
   ;

declarator
   : ptrDeclarator
   | noPtrDeclarator parametersAndQualifiers trailingReturnType
   ;

ptrDeclarator
   : noPtrDeclarator
   | ptrOperator ptrDeclarator
   ;

noPtrDeclarator
   : declaratorId attributeSpecifierSeq?
   | noPtrDeclarator parametersAndQualifiers
   | noPtrDeclarator '[' constantExpression? ']' attributeSpecifierSeq?
   | '(' ptrDeclarator ')'
   ;

parametersAndQualifiers
   : '(' parameterDeclarationClause ')' cvQualifierSeq? refQualifier? exceptionSpecification? attributeSpecifierSeq?
   ;

trailingReturnType
   : '->' trailingTypeSpecifierSeq abstractDeclarator?
   ;

ptrOperator
   : '*' attributeSpecifierSeq? cvQualifierSeq?
   | '&' attributeSpecifierSeq?
   | '&&' attributeSpecifierSeq?
   | nestedNameSpecifier '*' attributeSpecifierSeq? cvQualifierSeq?
   ;

cvQualifierSeq
   : cvQualifier cvQualifierSeq?
   ;

cvQualifier
   : Const
   | Volatile
   ;

refQualifier
   : '&'
   | '&&'
   ;

declaratorId
   : '...'? idExpression
   ;

theTypeId
   : typeSpecifierSeq abstractDeclarator?
   ;

abstractDeclarator
   : ptrAbstractDeclarator
   | noPtrAbstractDeclarator? parametersAndQualifiers trailingReturnType
   | abstractPackDeclarator
   ;

ptrAbstractDeclarator
   : noPtrAbstractDeclarator
   | ptrOperator ptrAbstractDeclarator?
   ;

noPtrAbstractDeclarator
   : noPtrAbstractDeclarator parametersAndQualifiers
   | parametersAndQualifiers
   | noPtrAbstractDeclarator '[' constantExpression? ']' attributeSpecifierSeq?
   | '[' constantExpression? ']' attributeSpecifierSeq?
   | '(' ptrAbstractDeclarator ')'
   ;

abstractPackDeclarator
   : noPtrAbstractPackDeclarator
   | ptrOperator abstractPackDeclarator
   ;

noPtrAbstractPackDeclarator
   : noPtrAbstractPackDeclarator parametersAndQualifiers
   | noPtrAbstractPackDeclarator '[' constantExpression? ']' attributeSpecifierSeq?
   | '...'
   ;

parameterDeclarationClause
   : parameterDeclarationList? '...'?
   | parameterDeclarationList ',' '...'
   ;

parameterDeclarationList
   : parameterDeclaration
   | parameterDeclarationList ',' parameterDeclaration
   ;

parameterDeclaration
   : attributeSpecifierSeq? declSpecifierSeq declarator
   | attributeSpecifierSeq? declSpecifierSeq declarator '=' initializerClause
   | attributeSpecifierSeq? declSpecifierSeq abstractDeclarator?
   | attributeSpecifierSeq? declSpecifierSeq abstractDeclarator? '=' initializerClause
   ;

functionDefinition
   : attributeSpecifierSeq? declSpecifierSeq? declarator virtSpecifierSeq? functionBody
   ;

functionBody
   : ctorInitializer? compoundStatement
   | functionTryBlock
   | '=' Default ';'
   | '=' Delete ';'
   ;

initializer
   : braceOrEqualInitializer
   | '(' expressionList ')'
   ;

braceOrEqualInitializer
   : '=' initializerClause
   | bracedInitList
   ;

initializerClause
   : assignmentExpression
   | bracedInitList
   ;

initializerList
   : initializerClause '...'?
   | initializerList ',' initializerClause '...'?
   ;

bracedInitList
   : '{' initializerList ','? '}'
   | '{' '}'
   ;
/*Classes*/


className
   : Identifier
   | simpleTemplateId
   ;

classSpecifier
   : classHead '{' memberSpecification? '}'
   ;

classHead
   : classKey attributeSpecifierSeq? classHeadName classVirtSpecifier? baseClause?
   | classKey attributeSpecifierSeq? baseClause?
   ;

classHeadName
   : nestedNameSpecifier? className
   ;

classVirtSpecifier
   : Final
   ;

classKey
   : Class
   | Struct
   | Union
   ;

memberSpecification
   : memberDeclaration memberSpecification?
   | accessSpecifier ':' memberSpecification?
   ;

memberDeclaration
   : attributeSpecifierSeq? declSpecifierSeq? memberDeclaratorList? ';'
   | functionDefinition
   | usingDeclaration
   | staticAssertDeclaration
   | templateDeclaration
   | aliasDeclaration
   | emptyDeclaration
   ;

memberDeclaratorList
   : memberDeclarator
   | memberDeclaratorList ',' memberDeclarator
   ;

memberDeclarator
   : declarator virtSpecifierSeq? pureSpecifier?
   | declarator braceOrEqualInitializer?
   | Identifier? attributeSpecifierSeq? ':' constantExpression
   ;

virtSpecifierSeq
   : virtSpecifier
   | virtSpecifierSeq virtSpecifier
   ;

virtSpecifier
   : Override
   | Final
   ;
/*
pureSpecifier:
   '=' '0'//Conflicts with the lexer
 ;
 */


pureSpecifier
   : Assign val = OctalLiteral
   {if ($val.text != "0") throw new InputMismatchException(this);}
   ;
/*Derived classes*/


baseClause
   : ':' baseSpecifierList
   ;

baseSpecifierList
   : baseSpecifier '...'?
   | baseSpecifierList ',' baseSpecifier '...'?
   ;

baseSpecifier
   : attributeSpecifierSeq? baseTypeSpecifier
   | attributeSpecifierSeq? Virtual accessSpecifier? baseTypeSpecifier
   | attributeSpecifierSeq? accessSpecifier Virtual? baseTypeSpecifier
   ;

classOrDeclType
   : nestedNameSpecifier? className
   | declTypeSpecifier
   ;

baseTypeSpecifier
   : classOrDeclType
   ;

accessSpecifier
   : Private
   | Protected
   | Public
   ;
/*Special member functions*/


conversionFunctionId
   : Operator conversionTypeId
   ;

conversionTypeId
   : typeSpecifierSeq conversionDeclarator?
   ;

conversionDeclarator
   : ptrOperator conversionDeclarator?
   ;

ctorInitializer
   : ':' memInitializerList
   ;

memInitializerList
   : memInitializer '...'?
   | memInitializer '...'? ',' memInitializerList
   ;

memInitializer
   : memInitializerId '(' expressionList? ')'
   | memInitializerId bracedInitList
   ;

memInitializerId
   : classOrDeclType
   | Identifier
   ;
/*Overloading*/


operatorFunctionId
   : Operator theOperator
   ;

literalOperatorId
   : Operator StringLiteral Identifier
   | Operator UserDefinedStringLiteral
   ;
/*Templates*/


templateDeclaration
   : Template '<' templateParameterList '>' declaration
   ;

templateParameterList
   : templateParameter
   | templateParameterList ',' templateParameter
   ;

templateParameter
   : typeParameter
   | parameterDeclaration
   ;

typeParameter
   : Class '...'? Identifier?
   | Class Identifier? '=' theTypeId
   | TypeName '...'? Identifier?
   | TypeName Identifier? '=' theTypeId
   | Template '<' templateParameterList '>' Class '...'? Identifier?
   | Template '<' templateParameterList '>' Class Identifier? '=' idExpression
   ;

simpleTemplateId
   : templateName '<' templateArgumentList? '>'
   ;

templateId
   : simpleTemplateId
   | operatorFunctionId '<' templateArgumentList? '>'
   | literalOperatorId '<' templateArgumentList? '>'
   ;

templateName
   : Identifier
   ;

templateArgumentList
   : templateArgument '...'?
   | templateArgumentList ',' templateArgument '...'?
   ;

templateArgument
   : theTypeId
   | constantExpression
   | idExpression
   ;

typeNameSpecifier
   : TypeName nestedNameSpecifier Identifier
   | TypeName nestedNameSpecifier Template? simpleTemplateId
   ;

explicitInstantiation
   : Extern? Template declaration
   ;

explicitSpecialization
   : Template '<' '>' declaration
   ;
/*Exception handling*/


tryBlock
   : Try compoundStatement handlerSeq
   ;

functionTryBlock
   : Try ctorInitializer? compoundStatement handlerSeq
   ;

handlerSeq
   : handler handlerSeq?
   ;

handler
   : Catch '(' exceptionDeclaration ')' compoundStatement
   ;

exceptionDeclaration
   : attributeSpecifierSeq? typeSpecifierSeq declarator
   | attributeSpecifierSeq? typeSpecifierSeq abstractDeclarator?
   | '...'
   ;

throwExpression
   : Throw assignmentExpression?
   ;

exceptionSpecification
   : dynamicExceptionSpecification
   | noExceptSpecification
   ;

dynamicExceptionSpecification
   : Throw '(' typeIdList? ')'
   ;

typeIdList
   : theTypeId '...'?
   | typeIdList ',' theTypeId '...'?
   ;

noExceptSpecification
   : NoExcept '(' constantExpression ')'
   | NoExcept
   ;
/*Preprocessing directives*/


MultiLineMacro
   : '#' (~ [\n]*? '\\' '\r'? '\n')+ ~ [\n]+ -> channel (HIDDEN)
   ;

Directive
   : '#' ~ [\n]* -> channel (HIDDEN)
   ;
/*Lexer*/

/*Keywords*/


AlignAs
   : 'alignas'
   ;

AlignOf
   : 'alignof'
   ;

Asm
   : 'asm'
   ;

Auto
   : 'auto'
   ;

Bool
   : 'bool'
   ;

Break
   : 'break'
   ;

Case
   : 'case'
   ;

Catch
   : 'catch'
   ;

Char
   : 'char'
   ;

Char16
   : 'char16_t'
   ;

Char32
   : 'char32_t'
   ;

Class
   : 'class'
   ;

Const
   : 'const'
   ;

Constexpr
   : 'constexpr'
   ;

ConstCast
   : 'const_cast'
   ;

Continue
   : 'continue'
   ;

DeclType
   : 'decltype'
   ;

Default
   : 'default'
   ;

Delete
   : 'delete'
   ;

Do
   : 'do'
   ;

Double
   : 'double'
   ;

DynamicCast
   : 'dynamic_cast'
   ;

Else
   : 'else'
   ;

Enum
   : 'enum'
   ;

Explicit
   : 'explicit'
   ;

Export
   : 'export'
   ;

Extern
   : 'extern'
   ;

False
   : 'false'
   ;

Final
   : 'final'
   ;

Float
   : 'float'
   ;

For
   : 'for'
   ;

Friend
   : 'friend'
   ;

Goto
   : 'goto'
   ;

If
   : 'if'
   ;

Inline
   : 'inline'
   ;

Int
   : 'int'
   ;

Long
   : 'long'
   ;

Mutable
   : 'mutable'
   ;

Namespace
   : 'namespace'
   ;

New
   : 'new'
   ;

NoExcept
   : 'noexcept'
   ;

NullPtr
   : 'nullptr'
   ;

Null
   : 'NULL'
   ;

Operator
   : 'operator'
   ;

Override
   : 'override'
   ;

Private
   : 'private'
   ;

Protected
   : 'protected'
   ;

Public
   : 'public'
   ;

Register
   : 'register'
   ;

ReinterpretCast
   : 'reinterpret_cast'
   ;

Return
   : 'return'
   ;

Short
   : 'short'
   ;

Signed
   : 'signed'
   ;

SizeOf
   : 'sizeof'
   ;

Static
   : 'static'
   ;

StaticAssert
   : 'static_assert'
   ;

StaticCast
   : 'static_cast'
   ;

Struct
   : 'struct'
   ;

Switch
   : 'switch'
   ;

Template
   : 'template'
   ;

This
   : 'this'
   ;

ThreadLocal
   : 'thread_local'
   ;

Throw
   : 'throw'
   ;

True
   : 'true'
   ;

Try
   : 'try'
   ;

TypeDef
   : 'typedef'
   ;

TypeId
   : 'typeid'
   ;

TypeName
   : 'typename'
   ;

Union
   : 'union'
   ;

Unsigned
   : 'unsigned'
   ;

Using
   : 'using'
   ;

Virtual
   : 'virtual'
   ;

Void
   : 'void'
   ;

Volatile
   : 'volatile'
   ;

Wchar
   : 'wchar_t'
   ;

While
   : 'while'
   ;
/*Operators*/


LeftParen
   : '('
   ;

RightParen
   : ')'
   ;

LeftBracket
   : '['
   ;

RightBracket
   : ']'
   ;

LeftBrace
   : '{'
   ;

RightBrace
   : '}'
   ;

Plus
   : '+'
   ;

Minus
   : '-'
   ;

Star
   : '*'
   ;

Div
   : '/'
   ;

Mod
   : '%'
   ;

Caret
   : '^'
   ;

And
   : '&'
   ;

Or
   : '|'
   ;

Tilde
   : '~'
   ;

Not
   : '!'
   | 'not'
   ;

Assign
   : '='
   ;

Less
   : '<'
   ;

Greater
   : '>'
   ;

PlusAssign
   : '+='
   ;

MinusAssign
   : '-='
   ;

StarAssign
   : '*='
   ;

DivAssign
   : '/='
   ;

ModAssign
   : '%='
   ;

XorAssign
   : '^='
   ;

AndAssign
   : '&='
   ;

OrAssign
   : '|='
   ;

LeftShift
   : '<<'
   ;

RightShift
   :
   '>>'
   ;

LeftShiftAssign
   : '<<='
   ;

RightShiftAssign
   :
   '>>='
   ;

Equal
   : '=='
   ;

NotEqual
   : '!='
   ;

LessEqual
   : '<='
   ;

GreaterEqual
   : '>='
   ;

AndAnd
   : '&&'
   | 'and'
   ;

OrOr
   : '||'
   | 'or'
   ;

PlusPlus
   : '++'
   ;

MinusMinus
   : '--'
   ;

Comma
   : ','
   ;

ArrowStar
   : '->*'
   ;

Arrow
   : '->'
   ;

Question
   : '?'
   ;

Colon
   : ':'
   ;

DoubleColon
   : '::'
   ;

Semi
   : ';'
   ;

Dot
   : '.'
   ;

DotStar
   : '.*'
   ;

Ellipsis
   : '...'
   ;

theOperator
   : New
   | Delete
   | New '[' ']'
   | Delete '[' ']'
   | '+'
   | '-'
   | '*'
   | '/'
   | '%'
   | '^'
   | '&'
   | '|'
   | '~'
   | '!'
   | 'not'
   | '='
   | '<'
   | '>'
   | '+='
   | '-='
   | '*='
   | '/='
   | '%='
   | '^='
   | '&='
   | '|='
   | LeftShift
   | RightShift
   | RightShiftAssign
   | LeftShiftAssign
   | '=='
   | '!='
   | '<='
   | '>='
   | '&&'
   | 'and'
   | '||'
   | 'or'
   | '++'
   | '--'
   | ','
   | '->*'
   | '->'
   | '(' ')'
   | '[' ']'
   ;
/*Lexer*/


fragment Hexquad
   : HEXADECIMALDIGIT HEXADECIMALDIGIT HEXADECIMALDIGIT HEXADECIMALDIGIT
   ;

fragment UniversalCharacterName
   : '\\u' Hexquad
   | '\\U' Hexquad Hexquad
   ;

Identifier
   :
/*
   IdentifierNonDigit
   | Identifier IdentifierNonDigit
   | Identifier DIGIT
   */
   IdentifierNonDigit (IdentifierNonDigit | DIGIT)*
   | Null
   ;

fragment IdentifierNonDigit
   : NONDIGIT
   | UniversalCharacterName
   ;

fragment NONDIGIT
   : [a-zA-Z_]
   ;

fragment DIGIT
   : [0-9]
   ;

literal
   : IntegerLiteral
   | CharacterLiteral
   | FloatingLiteral
   | StringLiteral
   | booleanLiteral
   | pointerLiteral
   | userDefinedLiteral
   ;

IntegerLiteral
   : DecimalLiteral IntegerSuffix?
   | OctalLiteral IntegerSuffix?
   | HexadecimalLiteral IntegerSuffix?
   | BinaryLiteral IntegerSuffix?
   ;

DecimalLiteral
   : NONZERODIGIT ('\''? DIGIT)*
   ;

OctalLiteral
   : '0' ('\''? OCTALDIGIT)*
   ;

HexadecimalLiteral
   : ('0x' | '0X') HEXADECIMALDIGIT ('\''? HEXADECIMALDIGIT)*
   ;

BinaryLiteral
   : ('0b' | '0B') BINARYDIGIT ('\''? BINARYDIGIT)*
   ;

fragment NONZERODIGIT
   : [1-9]
   ;

fragment OCTALDIGIT
   : [0-7]
   ;

fragment HEXADECIMALDIGIT
   : [0-9a-fA-F]
   ;

fragment BINARYDIGIT
   : [01]
   ;

IntegerSuffix
   : UnsignedSuffix LongSuffix?
   | UnsignedSuffix LongLongSuffix?
   | LongSuffix UnsignedSuffix?
   | LongLongSuffix UnsignedSuffix?
   ;

fragment UnsignedSuffix
   : [uU]
   ;

fragment LongSuffix
   : [lL]
   ;

fragment LongLongSuffix
   : 'll'
   | 'LL'
   ;

CharacterLiteral
   : '\'' Cchar+ '\''
   | 'u' '\'' Cchar+ '\''
   | 'U' '\'' Cchar+ '\''
   | 'L' '\'' Cchar+ '\''
   ;

fragment Cchar
   : ~ ['\\\r\n]
   | EscapeSequence
   | UniversalCharacterName
   ;

fragment EscapeSequence
   : SimpleEscapeSequence
   | OctalEscapeSequence
   | HexadecimalEscapeSequence
   ;

fragment SimpleEscapeSequence
   : '\\\''
   | '\\"'
   | '\\?'
   | '\\\\'
   | '\\a'
   | '\\b'
   | '\\f'
   | '\\n'
   | '\\r'
   | '\\t'
   | '\\v'
   ;

fragment OctalEscapeSequence
   : '\\' OCTALDIGIT
   | '\\' OCTALDIGIT OCTALDIGIT
   | '\\' OCTALDIGIT OCTALDIGIT OCTALDIGIT
   ;

fragment HexadecimalEscapeSequence
   : '\\x' HEXADECIMALDIGIT+
   ;

FloatingLiteral
   : FractionalConstant ExponentPart? FloatingSuffix?
   | DigitSequence ExponentPart FloatingSuffix?
   ;

fragment FractionalConstant
   : DigitSequence? '.' DigitSequence
   | DigitSequence '.'
   ;

fragment ExponentPart
   : 'e' SIGN? DigitSequence
   | 'E' SIGN? DigitSequence
   ;

fragment SIGN
   : [+-]
   ;

fragment DigitSequence
   : DIGIT ('\''? DIGIT)*
   ;

fragment FloatingSuffix
   : [flFL]
   ;

StringLiteral
   : EncodingPrefix? '"' Schar* '"'
   | EncodingPrefix? 'R' RawString
   ;

fragment EncodingPrefix
   : 'u8'
   | 'u'
   | 'U'
   | 'L'
   ;

fragment Schar
   : ~ ["\\\r\n]
   | EscapeSequence
   | UniversalCharacterName
   ;

fragment RawString
   : '"' .*? '(' .*? ')' .*? '"'
   ;

booleanLiteral
   : False
   | True
   ;

pointerLiteral
   : NullPtr
   | Null
   ;

userDefinedLiteral
   : UserDefinedIntegerLiteral
   | UserDefinedFloatingLiteral
   | UserDefinedStringLiteral
   | UserDefinedCharacterLiteral
   ;

UserDefinedIntegerLiteral
   : DecimalLiteral UdSuffix
   | OctalLiteral UdSuffix
   | HexadecimalLiteral UdSuffix
   | BinaryLiteral UdSuffix
   ;

UserDefinedFloatingLiteral
   : FractionalConstant ExponentPart? UdSuffix
   | DigitSequence ExponentPart UdSuffix
   ;

UserDefinedStringLiteral
   : StringLiteral UdSuffix
   ;

UserDefinedCharacterLiteral
   : CharacterLiteral UdSuffix
   ;

fragment UdSuffix
   : Identifier
   ;

WhiteSpace
   : [ \t]+ -> skip
   ;

NewLine
   : ('\r' '\n'? | '\n') -> skip
   ;

BlockComment
   : '/*' .*? '*/' -> skip
   ;

LineComment
   : '//' ~ [\r\n]* -> skip
   ;
