namespace Loretta.CodeAnalysis.Lua.Mutable.Syntax;

public enum MutableSyntaxKind
{
	None = 0,
	
	// Binding 1 -> 10
	IdentifierName = 1,
	WrappedIdentifierName = 2,
	LocalDeclarationName = 3,
	StatementList = 4,
	CompilationUnit = 5,
	
	// Expressions 11 -> 100
	LiteralExpression = 100,
	FunctionCallExpression = 101,
	MethodCallExpression = 102,
	MemberAccessExpression = 103,
	ElementAccessExpression = 104,
	UnaryExpression = 105,
	BinaryExpression = 106,
	TableConstructorExpression = 107,
	AnonymousFunctionExpression = 108,
	ParenthesizedExpression = 109,
	VarArgExpression = 110,
	
	// Function Argument 101 -> 200
	ExpressionListFunctionArgument = 101,
	StringFunctionArgument = 102,
	TableConstructorFunctionArgument = 103,
	
	// Table Field 201 -> 300
	UnkeyedTableField = 201,
	IdentifierKeyedTableField = 202,
	ExpressionKeyedTableField = 203,
	
	// Paramerter 301 -> 400
	NamedParameter = 301,
	VarArgParameter = 302,
	
	// Statements 1001 -> 10000
	IfStatement = 1001,
	ElseIfClause = 1002,
	ElseClause = 1003,
	WhileStatement = 1004,
	RepeatUntilStatement = 1005,
	NumericForStatement = 1006,
	GenericForStatement = 1007,
	DoStatement = 1008,
	LocalVariableDeclarationStatement = 1009,
	AssignmentStatement = 1010,
	EqualsValuesClause = 1011,
	ExpressionStatement = 1012,
	GotoStatement = 1013,
	GotoLabelStatement = 1014,
	BreakStatement = 1015,
	ContinueStatement = 1016,
	ReturnStatement = 1017,
	FunctionDeclarationStatement = 1018,
	SimpleFunctionName = 1019,
	MethodFunctionName = 1020,
	MemberFunctionName = 1021,
	LocalFunctionDeclarationStatement = 1022,
}