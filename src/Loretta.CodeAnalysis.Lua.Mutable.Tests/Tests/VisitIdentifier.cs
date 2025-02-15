using Loretta.CodeAnalysis.Lua.Mutable;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;

namespace SourceAnalysis.MutableLoretta.Tests.Tests;

public class WalkIdentifier : MutableSyntaxWalker
{
	public string Name; 
	public MutableExpression MutableNode; 
	
	public override void VisitIdentifierName(IdentifierName node)
	{
		Name = node.Name;
		MutableNode = node;
		
		base.VisitIdentifierName(node);
	}
}

public class RewriteIdentifier : MutableSyntaxRewriter
{
	public override MutableSyntaxNode? VisitIdentifierName(IdentifierName node)
	{
		return new IdentifierName
		{
			Name = "Rewritten"
		};
	}
}

