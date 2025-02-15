using Loretta.CodeAnalysis.Lua.Mutable;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;

namespace SourceAnalysis.MutableLoretta.Tests.Tests;

public class WalkTable : MutableSyntaxWalker
{
	public string Name;

	public override void VisitIdentifierKeyedTableField(IdentifierKeyedTableField node)
	{
		Name = node.Name;
		base.VisitIdentifierKeyedTableField(node);
	}
}

public class RewriteTable : MutableSyntaxRewriter
{
	public override MutableSyntaxNode? VisitExpressionKeyedTableField(ExpressionKeyedTableField node)
	{
		if (node.Key is LiteralExpression literal)
		{
			literal.LiteralData = 10;
		}
		
		return base.VisitExpressionKeyedTableField(node);
	}
}
