using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Mutable;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;

namespace SourceAnalysis.MutableLoretta.Tests.Tests;

public class WalkTreeAndRewrite : MutableSyntaxWalker
{
	public override void VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatement node)
	{
		if (node.Body.Statements[0] is ReturnStatement ret
		    && ret.Expressions[0] is LiteralExpression literal)
		{
			literal.SourceSyntaxKind = SyntaxKind.StringLiteralExpression;
			literal.LiteralData = "Hello World!";
		}
		
		base.VisitLocalFunctionDeclarationStatement(node);
	}

	public override void VisitFunctionCallExpression(FunctionCallExpression node)
	{
		if (node.Expression is IdentifierName { Name: "assert_true" } ident)
		{
			ident.Name = "assert_false";
		}
		
		base.VisitFunctionCallExpression(node);
	}
}