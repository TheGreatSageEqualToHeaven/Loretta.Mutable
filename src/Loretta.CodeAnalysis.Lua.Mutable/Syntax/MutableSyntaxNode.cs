namespace Loretta.CodeAnalysis.Lua.Mutable.Syntax;

public abstract class MutableSyntaxNode
{
	public abstract MutableSyntaxKind Kind { get; set; }
	
	public string ToFullString()
	{
		return base.ToString();
	}
}

public abstract class MutableExpression : MutableSyntaxNode
{
	public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.None;
}

public abstract class MutableStatement : MutableSyntaxNode
{
	public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.None;
}

public abstract class MutableFunctionArgument : MutableSyntaxNode
{
	public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.None;
}

public abstract class MutableTableField : MutableSyntaxNode
{
	public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.None;
}

public abstract class MutableParameter : MutableSyntaxNode
{
	public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.None;
}

public abstract class MutableFunctionName : MutableSyntaxNode
{
	public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.None;
}