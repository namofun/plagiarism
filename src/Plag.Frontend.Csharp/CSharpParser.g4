// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

parser grammar CSharpParser;

options { tokenVocab=CSharpLexer; }

// entry point
compilationUnit
	: BYTE_ORDER_MARK? externAliasDirectives? usingDirectives?
	  globalAttributeSection* namespaceMemberDeclarations? EOF
	;

//B.2 Syntactic grammar

//B.2.1 Basic concepts

namespaceOrTypeName 
	: (identifier typeArgumentList? | qualifiedAliasMember) ('.' identifier typeArgumentList?)*
	;

//B.2.2 Types
type
	: baseType ('?' | rankSpecifier | '*')*
	;

baseType
	: simpleType
	| classType  // represents types: enum, class, interface, delegate, typeParameter
	| VOID '*'
	| tupleType
	;

tupleType
    : '(' tupleElement (',' tupleElement)+ ')'
    ;

tupleElement
    : type identifier?
    ;

simpleType 
	: numericType
	| BOOL
	;

numericType 
	: integralType
	| floatingPointType
	| DECIMAL
	;

integralType 
	: SBYTE
	| BYTE
	| SHORT
	| USHORT
	| INT
	| UINT
	| LONG
	| ULONG
	| CHAR
	;

floatingPointType 
	: FLOAT
	| DOUBLE
	;

/** namespaceOrTypeName, OBJECT, STRING */
classType 
	: namespaceOrTypeName
	| OBJECT
	| DYNAMIC
	| STRING
	;

typeArgumentList 
	: '<' type ( ',' type)* '>'
	;

//B.2.4 Expressions
argumentList 
	: argument ( ',' argument)*
	;

argument
	: (identifier ':')? refout=(REF | OUT | IN)? (VAR | type)? expression
	;

expression
	: assignment
	| nonAssignmentExpression
	| REF nonAssignmentExpression
	;

nonAssignmentExpression
	: lambdaExpression
	| queryExpression
	| conditionalExpression
	;

assignment
	: unaryExpression assignmentOperator expression
	| (primaryExpression '??=')+ (unaryExpression | throwExpression)
	;

assignmentOperator
	: '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | rightShiftAssignment
	;

conditionalExpression
	: nullCoalescingExpression ('?' throwableExpression ':' throwableExpression)?
	;

nullCoalescingExpression
	: conditionalOrExpression ('??' (nullCoalescingExpression | throwExpression))?
	;

conditionalOrExpression
	: conditionalAndExpression (OP_OR conditionalAndExpression)*
	;

conditionalAndExpression
	: inclusiveOrExpression (OP_AND inclusiveOrExpression)*
	;

inclusiveOrExpression
	: exclusiveOrExpression ('|' exclusiveOrExpression)*
	;

exclusiveOrExpression
	: andExpression ('^' andExpression)*
	;

andExpression
	: equalityExpression ('&' equalityExpression)*
	;

equalityExpression
	: relationalExpression ((OP_EQ | OP_NE)  relationalExpression)*
	;

relationalExpression
	: shiftExpression (('<' | '>' | '<=' | '>=') shiftExpression | IS isType | AS type)*
	;

shiftExpression
	: additiveExpression (('<<' | rightShift)  additiveExpression)*
	;

additiveExpression
	: multiplicativeExpression (('+' | '-')  multiplicativeExpression)*
	;

multiplicativeExpression
	: switchExpression (('*' | '/' | '%')  switchExpression)*
	;

switchExpression
    : rangeExpression ('switch' '{' (switchExpressionArms ','?)? '}')?
    ;

switchExpressionArms
    : switchExpressionArm (',' switchExpressionArm)*
    ;

switchExpressionArm
    : expression caseGuard? rightArrow throwableExpression
    ;

rangeExpression
    : unaryExpression                               #unaryExpressionImpl
    | unaryExpression? OP_RANGE unaryExpression?    #rangeExpressionImpl
    ;

// https://msdn.microsoft.com/library/6a71f45d(v=vs.110).aspx
unaryExpression
	: primaryExpression
	| '+' unaryExpression
	| '-' unaryExpression
	| BANG unaryExpression
	| '~' unaryExpression
	| '++' unaryExpression
	| '--' unaryExpression
	| OPEN_PARENS type CLOSE_PARENS unaryExpression
	| AWAIT unaryExpression // C# 5
	| '&' unaryExpression
	| '*' unaryExpression
	| '^' unaryExpression // C# 8 ranges
	;

primaryExpression  // Null-conditional operators C# 6: https://msdn.microsoft.com/en-us/library/dn986595.aspx
	: pe=primaryExpressionStart '!'? bracketExpression* '!'?
	  (((memberAccess | methodInvocation | '++' | '--' | '->' identifier) '!'?) bracketExpression* '!'?)*
	;

primaryExpressionStart
	: literal                                   #literalExpression
	| identifier typeArgumentList?              #simpleNameExpression
	| OPEN_PARENS expression CLOSE_PARENS       #parenthesisExpressions
	| predefinedType                            #memberAccessExpression
	| qualifiedAliasMember                      #memberAccessExpression
	| LITERAL_ACCESS                            #literalAccessExpression
	| THIS                                      #thisReferenceExpression
	| BASE ('.' identifier typeArgumentList? | '[' expressionList ']') #baseAccessExpression
	| NEW (type ( objectCreationExpression
	             | objectOrCollectionInitializer
	             | '[' expressionList ']' rankSpecifier* arrayInitializer?
	             | rankSpecifier+ arrayInitializer)
	      | anonymousObjectInitializer
	      | rankSpecifier arrayInitializer)                         #objectCreationExpressionFull
	| OPEN_PARENS argument ( ',' argument )+ CLOSE_PARENS           #tupleExpression
	| TYPEOF OPEN_PARENS (unboundTypeName | type | VOID) CLOSE_PARENS   #typeofExpression
	| CHECKED OPEN_PARENS expression CLOSE_PARENS                   #checkedExpression
	| UNCHECKED OPEN_PARENS expression CLOSE_PARENS                 #uncheckedExpression
	| DEFAULT (OPEN_PARENS type CLOSE_PARENS)?                     #defaultValueExpression
	| ASYNC? DELEGATE (OPEN_PARENS explicitAnonymousFunctionParameterList? CLOSE_PARENS)? block #anonymousMethodExpression
	| SIZEOF OPEN_PARENS type CLOSE_PARENS                          #sizeofExpression
	// C# 6: https://msdn.microsoft.com/en-us/library/dn986596.aspx
	| NAMEOF OPEN_PARENS (identifier '.')* identifier CLOSE_PARENS  #nameofExpression
	;

throwableExpression
	: expression
	| throwExpression
	;

throwExpression
	: THROW expression
	;

memberAccess
	: '?'? '.' identifier typeArgumentList?
	;

bracketExpression
	: '?'? '[' indexerArgument ( ',' indexerArgument)* ']'
	;

indexerArgument
	: (identifier ':')? expression
	;

predefinedType
	: BOOL | BYTE | CHAR | DECIMAL | DOUBLE | FLOAT | INT | LONG
	| OBJECT | SBYTE | SHORT | STRING | UINT | ULONG | USHORT
	;

expressionList
	: expression (',' expression)*
	;

objectOrCollectionInitializer
	: objectInitializer
	| collectionInitializer
	;

objectInitializer
	: OPEN_BRACE (memberInitializerList ','?)? CLOSE_BRACE
	;

memberInitializerList
	: memberInitializer (',' memberInitializer)*
	;

memberInitializer
	: (identifier | '[' expression ']') '=' initializerValue // C# 6
	;

initializerValue
	: expression
	| objectOrCollectionInitializer
	;

collectionInitializer
	: OPEN_BRACE elementInitializer (',' elementInitializer)* ','? CLOSE_BRACE
	;

elementInitializer
	: nonAssignmentExpression
	| OPEN_BRACE expressionList CLOSE_BRACE
	;

anonymousObjectInitializer
	: OPEN_BRACE (memberDeclaratorList ','?)? CLOSE_BRACE
	;

memberDeclaratorList
	: memberDeclarator ( ',' memberDeclarator)*
	;

memberDeclarator
	: primaryExpression
	| identifier '=' expression
	;

unboundTypeName
	: identifier ( genericDimensionSpecifier? | '::' identifier genericDimensionSpecifier?)
	  ('.' identifier genericDimensionSpecifier?)*
	;

genericDimensionSpecifier
	: '<' ','* '>'
	;

isType
	: baseType (rankSpecifier | '*')* '?'? isTypePatternArms? identifier?
	;

isTypePatternArms
	: '{' isTypePatternArm (',' isTypePatternArm)* '}'
	;

isTypePatternArm
	: identifier ':' expression
	;

lambdaExpression
	: ASYNC? anonymousFunctionSignature rightArrow anonymousFunctionBody
	;

anonymousFunctionSignature
	: OPEN_PARENS CLOSE_PARENS
	| OPEN_PARENS explicitAnonymousFunctionParameterList CLOSE_PARENS
	| OPEN_PARENS implicitAnonymousFunctionParameterList CLOSE_PARENS
	| identifier
	;

explicitAnonymousFunctionParameterList
	: explicitAnonymousFunctionParameter ( ',' explicitAnonymousFunctionParameter)*
	;

explicitAnonymousFunctionParameter
	: refout=(REF | OUT | IN)? type identifier
	;

implicitAnonymousFunctionParameterList
	: identifier (',' identifier)*
	;

anonymousFunctionBody
	: throwableExpression
	| block
	;

queryExpression
	: fromClause queryBody
	;

fromClause
	: FROM type? identifier IN expression
	;

queryBody
	: queryBodyClause* selectOrGroupClause queryContinuation?
	;

queryBodyClause
	: fromClause
	| letClause
	| whereClause
	| combinedJoinClause
	| orderbyClause
	;

letClause
	: LET identifier '=' expression
	;

whereClause
	: WHERE expression
	;

combinedJoinClause
	: JOIN type? identifier IN expression ON expression EQUALS expression (INTO identifier)?
	;

orderbyClause
	: ORDERBY ordering (','  ordering)*
	;

ordering
	: expression dir=(ASCENDING | DESCENDING)?
	;

selectOrGroupClause
	: SELECT expression
	| GROUP expression BY expression
	;

queryContinuation
	: INTO identifier queryBody
	;

//B.2.5 Statements
statement
	: labeledStatement
	| declarationStatement
	| embeddedStatement
	;

declarationStatement
	: localVariableDeclaration ';'
	| localConstantDeclaration ';'
	| localFunctionDeclaration
	;

localFunctionDeclaration
    : localFunctionHeader localFunctionBody
    ;

localFunctionHeader
    : localFunctionModifiers? returnType identifier typeParameterList?
        OPEN_PARENS formalParameterList? CLOSE_PARENS typeParameterConstraintsClauses?
    ;

localFunctionModifiers
    : (ASYNC | UNSAFE) STATIC?
    | STATIC (ASYNC | UNSAFE)
    ;

localFunctionBody
    : block
    | rightArrow throwableExpression ';'
    ;

labeledStatement
	: identifier ':' statement  
	;

embeddedStatement
	: block
	| simpleEmbeddedStatement
	;

simpleEmbeddedStatement
	: ';'                                                         #theEmptyStatement
	| expression ';'                                              #expressionStatement

	// selection statements
	| IF OPEN_PARENS expression CLOSE_PARENS ifBody (ELSE ifBody)?               #ifStatement
    | SWITCH OPEN_PARENS expression CLOSE_PARENS OPEN_BRACE switchSection* CLOSE_BRACE           #switchStatement

    // iteration statements
	| WHILE OPEN_PARENS expression CLOSE_PARENS embeddedStatement                                        #whileStatement
	| DO embeddedStatement WHILE OPEN_PARENS expression CLOSE_PARENS ';'                                 #doStatement
	| FOR OPEN_PARENS forInitializer? ';' expression? ';' forIterator? CLOSE_PARENS embeddedStatement    #forStatement
	| AWAIT? FOREACH OPEN_PARENS localVariableType identifier IN expression CLOSE_PARENS embeddedStatement    #foreachStatement

    // jump statements
	| BREAK ';'                                                   #breakStatement
	| CONTINUE ';'                                                #continueStatement
	| GOTO (identifier | CASE expression | DEFAULT) ';'           #gotoStatement
	| RETURN expression? ';'                                      #returnStatement
	| THROW expression? ';'                                       #throwStatement

	| TRY block (catchClauses finallyClause? | finallyClause)     #tryStatement
	| CHECKED block                                               #checkedStatement
	| UNCHECKED block                                             #uncheckedStatement
	| LOCK OPEN_PARENS expression CLOSE_PARENS embeddedStatement                  #lockStatement
	| USING OPEN_PARENS resourceAcquisition CLOSE_PARENS embeddedStatement        #usingStatement
	| YIELD (RETURN expression | BREAK) ';'                       #yieldStatement

	// unsafe statements
	| UNSAFE block                                                                       #unsafeStatement
	| FIXED OPEN_PARENS pointerType fixedPointerDeclarators CLOSE_PARENS embeddedStatement            #fixedStatement
	;

block
	: OPEN_BRACE statementList? CLOSE_BRACE
	;

localVariableDeclaration
	: (USING | REF | REF READONLY)? localVariableType localVariableDeclarator ( ','  localVariableDeclarator)*
	| FIXED pointerType fixedPointerDeclarators
	;

localVariableType 
	: VAR
	| type
	;

localVariableDeclarator
	: identifier ('=' REF? localVariableInitializer)?
	;

localVariableInitializer
	: expression
	| arrayInitializer
	| stackallocInitializer
	;

localConstantDeclaration
	: CONST type constantDeclarators
	;

ifBody
	: block
	| simpleEmbeddedStatement
	;

switchSection
	: switchLabel+ statementList
	;

switchLabel
	: CASE expression caseGuard? ':'
	| DEFAULT ':'
	;

caseGuard
	: WHEN expression
	;

statementList
	: statement+
	;

forInitializer
	: localVariableDeclaration
	| expression (','  expression)*
	;

forIterator
	: expression (','  expression)*
	;

catchClauses
	: specificCatchClause (specificCatchClause)* generalCatchClause?
	| generalCatchClause
	;

specificCatchClause
	: CATCH OPEN_PARENS classType identifier? CLOSE_PARENS exceptionFilter? block
	;

generalCatchClause
	: CATCH exceptionFilter? block
	;

exceptionFilter // C# 6
	: WHEN OPEN_PARENS expression CLOSE_PARENS
	;

finallyClause
	: FINALLY block
	;

resourceAcquisition
	: localVariableDeclaration
	| expression
	;

//B.2.6 Namespaces;
namespaceDeclaration
	: NAMESPACE qi=qualifiedIdentifier namespaceBody ';'?
	;

qualifiedIdentifier
	: identifier ( '.'  identifier )*
	;

namespaceBody
	: OPEN_BRACE externAliasDirectives? usingDirectives? namespaceMemberDeclarations? CLOSE_BRACE
	;

externAliasDirectives
	: externAliasDirective+
	;

externAliasDirective
	: EXTERN ALIAS identifier ';'
	;

usingDirectives
	: usingDirective+
	;

usingDirective
	: USING identifier '=' namespaceOrTypeName ';'            #usingAliasDirective
	| USING namespaceOrTypeName ';'                           #usingNamespaceDirective
	// C# 6: https://msdn.microsoft.com/en-us/library/ms228593.aspx
	| USING STATIC namespaceOrTypeName ';'                    #usingStaticDirective
	;

namespaceMemberDeclarations
	: namespaceMemberDeclaration+
	;

namespaceMemberDeclaration
	: namespaceDeclaration
	| typeDeclaration
	;

typeDeclaration
	: attributes? allMemberModifiers?
      (classDefinition | structDefinition | interfaceDefinition | enumDefinition | delegateDefinition)
    ;

qualifiedAliasMember
	: identifier '::' identifier typeArgumentList?
	;

//B.2.7 Classes;
typeParameterList
	: '<' typeParameter (','  typeParameter)* '>'
	;

typeParameter
	: attributes? identifier
	;

classBase
	: ':' classType (','  namespaceOrTypeName)*
	;

interfaceTypeList
	: namespaceOrTypeName (','  namespaceOrTypeName)*
	;

typeParameterConstraintsClauses
	: typeParameterConstraintsClause+
	;

typeParameterConstraintsClause
	: WHERE identifier ':' typeParameterConstraints
	;

typeParameterConstraints
	: constructorConstraint
	| primaryConstraint (',' secondaryConstraints)? (',' constructorConstraint)?
	;

primaryConstraint
	: classType
	| CLASS '?'?
	| STRUCT
	| UNMANAGED
	;

// namespaceOrTypeName includes identifier
secondaryConstraints
	: namespaceOrTypeName (',' namespaceOrTypeName)*
	;

constructorConstraint
	: NEW OPEN_PARENS CLOSE_PARENS
	;

classBody
	: OPEN_BRACE classMemberDeclarations? CLOSE_BRACE
	;

classMemberDeclarations
	: classMemberDeclaration+
	;

classMemberDeclaration
	: attributes? allMemberModifiers? (commonMemberDeclaration | destructorDefinition)
	;

allMemberModifiers
	: allMemberModifier+
	;

allMemberModifier
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| READONLY
	| VOLATILE
	| VIRTUAL
	| SEALED
	| OVERRIDE
	| ABSTRACT
	| STATIC
	| UNSAFE
	| EXTERN
	| PARTIAL
	| ASYNC  // C# 5
	;

// represents the intersection of structMemberDeclaration and classMemberDeclaration
commonMemberDeclaration
	: constantDeclaration
	| typedMemberDeclaration
	| eventDeclaration
	| conversionOperatorDeclarator (body | rightArrow throwableExpression ';') // C# 6
	| constructorDeclaration
	| VOID methodDeclaration
	| classDefinition
	| structDefinition
	| interfaceDefinition
	| enumDefinition
	| delegateDefinition
	;

typedMemberDeclaration
	: (REF | READONLY REF | REF READONLY)? type
	  ( namespaceOrTypeName '.' indexerDeclaration
	  | methodDeclaration
	  | propertyDeclaration
	  | indexerDeclaration
	  | operatorDeclaration
	  | fieldDeclaration
	  )
	;

constantDeclarators
	: constantDeclarator (','  constantDeclarator)*
	;

constantDeclarator
	: identifier '=' expression
	;

variableDeclarators
	: variableDeclarator (','  variableDeclarator)*
	;

variableDeclarator
	: identifier ('=' variableInitializer)?
	;

variableInitializer
	: expression
	| arrayInitializer
	;

returnType
	: type
	| VOID
	;

memberName
	: namespaceOrTypeName
	;

methodBody
	: block
	| ';'
	;

formalParameterList
	: parameterArray
	| fixedParameters (',' parameterArray)?
	;

fixedParameters
	: fixedParameter ( ',' fixedParameter )*
	;

fixedParameter
	: attributes? parameterModifier? argDeclaration
	| ARGLIST
	;

parameterModifier
	: REF
	| OUT
	| IN
	| REF THIS
	| IN THIS
	| THIS
	;

parameterArray
	: attributes? PARAMS arrayType identifier
	;

accessorDeclarations
	: attrs=attributes? mods=accessorModifier?
	  (GET accessorBody setAccessorDeclaration? | SET accessorBody getAccessorDeclaration?)
	;

getAccessorDeclaration
	: attributes? accessorModifier? GET accessorBody
	;

setAccessorDeclaration
	: attributes? accessorModifier? SET accessorBody
	;

accessorModifier
	: PROTECTED
	| INTERNAL
	| PRIVATE
	| PROTECTED INTERNAL
	| INTERNAL PROTECTED
	;

accessorBody
	: block
	| ';'
	;

eventAccessorDeclarations
	: attributes? (ADD block removeAccessorDeclaration | REMOVE block addAccessorDeclaration)
	;

addAccessorDeclaration
	: attributes? ADD block
	;

removeAccessorDeclaration
	: attributes? REMOVE block
	;

overloadableOperator
	: '+'
	| '-'
	| BANG
	| '~'
	| '++'
	| '--'
	| TRUE
	| FALSE
	| '*'
	| '/'
	| '%'
	| '&'
	| '|'
	| '^'
	| '<<'
	| rightShift
	| OP_EQ
	| OP_NE
	| '>'
	| '<'
	| '>='
	| '<='
	;

conversionOperatorDeclarator
	: (IMPLICIT | EXPLICIT) OPERATOR type OPEN_PARENS argDeclaration CLOSE_PARENS
	;

constructorInitializer
	: ':' (BASE | THIS) OPEN_PARENS argumentList? CLOSE_PARENS
	;

body
	: block
	| ';'
	;

//B.2.8 Structs
structInterfaces
	: ':' interfaceTypeList
	;

structBody
	: OPEN_BRACE structMemberDeclaration* CLOSE_BRACE
	;

structMemberDeclaration
	: attributes? allMemberModifiers?
	  (commonMemberDeclaration | FIXED type fixedSizeBufferDeclarator+ ';')
	;

//B.2.9 Arrays
arrayType
	: baseType (('*' | '?')* rankSpecifier)+
	;

rankSpecifier
	: '[' ','* ']'
	;

arrayInitializer
	: OPEN_BRACE (variableInitializer (','  variableInitializer)* ','?)? CLOSE_BRACE
	;

//B.2.10 Interfaces
variantTypeParameterList
	: '<' variantTypeParameter (',' variantTypeParameter)* '>'
	;

variantTypeParameter
	: attributes? varianceAnnotation? identifier
	;

varianceAnnotation
	: IN | OUT
	;

interfaceBase
	: ':' interfaceTypeList
	;

interfaceBody // ignored in csharp 8
	: OPEN_BRACE interfaceMemberDeclaration* CLOSE_BRACE
	;

interfaceMemberDeclaration
	: attributes? NEW?
	  (UNSAFE? (REF | REF READONLY | READONLY REF)? type
	    ( identifier typeParameterList? OPEN_PARENS formalParameterList? CLOSE_PARENS typeParameterConstraintsClauses? ';'
	    | identifier OPEN_BRACE interfaceAccessors CLOSE_BRACE
	    | THIS '[' formalParameterList ']' OPEN_BRACE interfaceAccessors CLOSE_BRACE)
	  | UNSAFE? VOID identifier typeParameterList? OPEN_PARENS formalParameterList? CLOSE_PARENS typeParameterConstraintsClauses? ';'
	  | EVENT type identifier ';')
	;

interfaceAccessors
	: attributes? (GET ';' (attributes? SET ';')? | SET ';' (attributes? GET ';')?)
	;

//B.2.11 Enums
enumBase
	: ':' type
	;

enumBody
	: OPEN_BRACE (enumMemberDeclaration (','  enumMemberDeclaration)* ','?)? CLOSE_BRACE
	;

enumMemberDeclaration
	: attributes? identifier ('=' expression)?
	;

//B.2.12 Delegates

//B.2.13 Attributes
globalAttributeSection
	: '[' globalAttributeTarget ':' attributeList ','? ']'
	;

globalAttributeTarget
	: keyword
	| identifier
	;

attributes
	: attributeSection+
	;

attributeSection
	: '[' (attributeTarget ':')? attributeList ','? ']'
	;

attributeTarget
	: keyword
	| identifier
	;

attributeList
	: attribute (','  attribute)*
	;

attribute
	: namespaceOrTypeName (OPEN_PARENS (attributeArgument (','  attributeArgument)*)? CLOSE_PARENS)?
	;

attributeArgument
	: (identifier ':')? expression
	;

//B.3 Grammar extensions for unsafe code
pointerType
	: (simpleType | classType) (rankSpecifier | '?')* '*'
	| VOID '*'
	;

fixedPointerDeclarators
	: fixedPointerDeclarator (','  fixedPointerDeclarator)*
	;

fixedPointerDeclarator
	: identifier '=' fixedPointerInitializer
	;

fixedPointerInitializer
	: '&'? expression
	| stackallocInitializer
	;

fixedSizeBufferDeclarator
	: identifier '[' expression ']'
	;

stackallocInitializer
	: STACKALLOC type '[' expression ']'
	| STACKALLOC type? '[' expression? ']' OPEN_BRACE expression (',' expression)* ','? CLOSE_BRACE
	;

rightArrow
	: first='=' second='>' {$first.index + 1 == $second.index}? // Nothing between the tokens?
	;

rightShift
	: first='>' second='>' {$first.index + 1 == $second.index}? // Nothing between the tokens?
	;

rightShiftAssignment
	: first='>' second='>=' {$first.index + 1 == $second.index}? // Nothing between the tokens?
	;

literal
	: booleanLiteral
	| stringLiteral
	| INTEGER_LITERAL
	| HEX_INTEGER_LITERAL
	| BIN_INTEGER_LITERAL
	| REAL_LITERAL
	| CHARACTER_LITERAL
	| NULL
	;

booleanLiteral
	: TRUE
	| FALSE
	;

stringLiteral
	: interpolatedRegularString
	| interpolatedVerbatiumString
	| REGULAR_STRING
	| VERBATIUM_STRING
	;

interpolatedRegularString
	: INTERPOLATED_REGULAR_STRING_START interpolatedRegularStringPart* DOUBLE_QUOTE_INSIDE
	;


interpolatedVerbatiumString
	: INTERPOLATED_VERBATIUM_STRING_START interpolatedVerbatiumStringPart* DOUBLE_QUOTE_INSIDE
	;

interpolatedRegularStringPart
	: interpolatedStringExpression
	| DOUBLE_CURLY_INSIDE
	| REGULAR_CHAR_INSIDE
	| REGULAR_STRING_INSIDE
	;

interpolatedVerbatiumStringPart
	: interpolatedStringExpression
	| DOUBLE_CURLY_INSIDE
	| VERBATIUM_DOUBLE_QUOTE_INSIDE
	| VERBATIUM_INSIDE_STRING
	;

interpolatedStringExpression
	: expression (',' expression)* (':' FORMAT_STRING+)?
	;

//B.1.7 Keywords
keyword
	: ABSTRACT
	| AS
	| BASE
	| BOOL
	| BREAK
	| BYTE
	| CASE
	| CATCH
	| CHAR
	| CHECKED
	| CLASS
	| CONST
	| CONTINUE
	| DECIMAL
	| DEFAULT
	| DELEGATE
	| DO
	| DOUBLE
	| ELSE
	| ENUM
	| EVENT
	| EXPLICIT
	| EXTERN
	| FALSE
	| FINALLY
	| FIXED
	| FLOAT
	| FOR
	| FOREACH
	| GOTO
	| IF
	| IMPLICIT
	| IN
	| INT
	| INTERFACE
	| INTERNAL
	| IS
	| LOCK
	| LONG
	| NAMESPACE
	| NEW
	| NULL
	| OBJECT
	| OPERATOR
	| OUT
	| OVERRIDE
	| PARAMS
	| PRIVATE
	| PROTECTED
	| PUBLIC
	| READONLY
	| REF
	| RETURN
	| SBYTE
	| SEALED
	| SHORT
	| SIZEOF
	| STACKALLOC
	| STATIC
	| STRING
	| STRUCT
	| SWITCH
	| THIS
	| THROW
	| TRUE
	| TRY
	| TYPEOF
	| UINT
	| ULONG
	| UNCHECKED
	| UNMANAGED
	| UNSAFE
	| USHORT
	| USING
	| VIRTUAL
	| VOID
	| VOLATILE
	| WHILE
	;

// -------------------- extra rules for modularization --------------------------------

classDefinition
	: CLASS identifier typeParameterList? classBase? typeParameterConstraintsClauses?
	    classBody ';'?
	;

structDefinition
	: (READONLY | REF)? STRUCT identifier typeParameterList? structInterfaces? typeParameterConstraintsClauses?
	    structBody ';'?
	;

interfaceDefinition
	: INTERFACE identifier variantTypeParameterList? interfaceBase?
	    typeParameterConstraintsClauses? classBody ';'?
	;

enumDefinition
	: ENUM identifier enumBase? enumBody ';'?
	;

delegateDefinition
	: DELEGATE returnType identifier variantTypeParameterList?
	  OPEN_PARENS formalParameterList? CLOSE_PARENS typeParameterConstraintsClauses? ';'
	;

eventDeclaration
	: EVENT type (variableDeclarators ';' | memberName OPEN_BRACE eventAccessorDeclarations CLOSE_BRACE)
	;

fieldDeclaration
	: variableDeclarators ';'
	;

propertyDeclaration // Property initializer & lambda in properties C# 6
	: memberName (OPEN_BRACE accessorDeclarations CLOSE_BRACE ('=' variableInitializer ';')? | rightArrow throwableExpression ';')
	;

constantDeclaration
	: CONST type constantDeclarators ';'
	;

indexerDeclaration // lamdas from C# 6
	: THIS '[' formalParameterList ']' (OPEN_BRACE accessorDeclarations CLOSE_BRACE | rightArrow throwableExpression ';')
	;

destructorDefinition
	: '~' identifier OPEN_PARENS CLOSE_PARENS body
	;

constructorDeclaration
	: identifier OPEN_PARENS formalParameterList? CLOSE_PARENS constructorInitializer? body
	;

methodDeclaration // lamdas from C# 6
	: methodMemberName typeParameterList? OPEN_PARENS formalParameterList? CLOSE_PARENS
	    typeParameterConstraintsClauses? (methodBody | rightArrow throwableExpression ';')
	;

methodMemberName
	: (identifier | identifier '::' identifier) (typeArgumentList? '.' identifier)*
	;

operatorDeclaration // lamdas form C# 6
	: OPERATOR overloadableOperator OPEN_PARENS IN? argDeclaration
	       (',' IN? argDeclaration)? CLOSE_PARENS (body | rightArrow throwableExpression ';')
	;

argDeclaration
	: type identifier ('=' expression)?
	;

methodInvocation
	: OPEN_PARENS argumentList? CLOSE_PARENS
	;

objectCreationExpression
	: OPEN_PARENS argumentList? CLOSE_PARENS objectOrCollectionInitializer?
	;

identifier
	: IDENTIFIER
	| ADD
	| ALIAS
	| ARGLIST
	| ASCENDING
	| ASYNC
	| AWAIT
	| BY
	| DESCENDING
	| DYNAMIC
	| EQUALS
	| FROM
	| GET
	| GROUP
	| INTO
	| JOIN
	| LET
	| NAMEOF
	| ON
	| ORDERBY
	| PARTIAL
	| REMOVE
	| SELECT
	| SET
	| UNMANAGED
	| VAR
	| WHEN
	| WHERE
	| YIELD
	;