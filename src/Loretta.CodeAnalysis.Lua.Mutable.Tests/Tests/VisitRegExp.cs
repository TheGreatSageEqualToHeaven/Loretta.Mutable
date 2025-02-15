using Loretta.CodeAnalysis.Lua.Mutable;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;

namespace SourceAnalysis.MutableLoretta.Tests.Tests;

public class VisitRegExp : MutableSyntaxWalker
{
	public int Value = -1;
	
	/*
	public override void VisitRegisterExpression(RegisterExpression node)
	{
		Value = node.Register;
		base.VisitRegisterExpression(node);
	}
	*/
}