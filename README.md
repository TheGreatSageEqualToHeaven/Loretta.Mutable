# Loretta.Mutable
A mutable extension for the Loretta library

## Usage

Include `Loretta.CodeAnalysis.Lua.Mutable` in your project. All functions and classes match their immutable counterparts minus a "Mutable" prefix.  
  
`SyntaxKind` -> `MutableSyntaxKind`  
`SyntaxFactory` -> `MutableFactory`  
`SyntaxNode`/`LuaSyntaxNode` -> `MutableSyntaxNode`  
`ExpressionSyntax` -> `MutableExpression`  
`StatementSyntax` -> `MutableStatement`  
`FunctionArgumentSyntax` -> `MutableFunctionArgument`  
`TableFieldSyntax` -> `MutableTableField`  
`ParameterSyntax` -> `MutableParameter`  
`FunctionNameSyntax` -> `MutableFunctionName`  
`SyntaxTree`/`LuaSyntaxTree` -> `MutableSyntaxTree`  
  
## Examples

Parsing a `LuaSyntaxTree` and creating a `MutableSyntaxTree` from it.

```cs 
		var parsedTree = LuaSyntaxTree.ParseText(@"
			local function foo() 
				return 10
			end
			
			assert_true(foo() == 10)
		");

		var mutableTree = MutableSyntaxTree.Create(parsedTree);
		
		new WalkTreeAndRewrite().Visit(mutableTree.Root);

		var compilationUnit = MutableFactory.ImmutableCompilationUnit(mutableTree.Root);
		
		Console.WriteLine(compilationUnit.NormalizeWhitespace().ToFullString());
```

An example `MutableSyntaxWalker` that changes a function identifier and type of the first returned constant to a string `"Hello World"`.  

```cs 
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
```

An example `MutableSyntaxRewriter` that replaces a `LocalVariableDeclarationStatement` with an `AssignmentStatement Replaced = ...` 

```cs 
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
```  
  
## MutableFactory   
  
MutableFactory serves as a way to convert immutable nodes to mutable nodes and vice versa. You are able to create mutable nodes directly by calling `new` unlike their immutable counterparts.  
  
`MutableFactory.ImmutableCompilationUnit(CompilationUnit)` to create an immutable `CompilationUnitSyntax`  
`MutableFactory.MutableCompilationUnit(CompilationUnitSyntax)` to create a mutable `CompilationUnit`  
   
`MutableFactory.ImmutableFunctionDeclarationStatement(FunctionDeclarationStatement)` to create an immutable `FunctionDeclarationStatementSyntax`  
`MutableFactory.MutableFunctionDeclarationStatement(FunctionDeclarationStatementSyntax)` to create a mutable `FunctionDeclarationStatement`  
   
Including `MutableFactory` also includes the `ToMutable` method for immutable nodes and `ToImmutable` for mutable nodes.  
