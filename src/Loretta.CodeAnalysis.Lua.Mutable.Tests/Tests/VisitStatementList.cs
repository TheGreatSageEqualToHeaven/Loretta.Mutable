using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Mutable;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;

namespace SourceAnalysis.MutableLoretta.Tests.Tests;

public class RewriteStatementList : MutableSyntaxRewriter
{
	public string Name;

	public override MutableSyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatement node)
	{
		Name = node.Names[0].Name;

		return new AssignmentStatement
		{
			Variables = [ new IdentifierName{ Name = "Replaced" } ],
			EqualsValues = new EqualsValuesClause { Values = [ new VarArgExpression() ] }
		};
	}
}