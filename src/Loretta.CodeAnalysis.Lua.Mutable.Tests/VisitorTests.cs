using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Lua.Mutable;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;

using SourceAnalysis.MutableLoretta.Tests.Tests;

namespace SourceAnalysis.MutableLoretta.Tests;

public class VisitorTests
{
	[Test]
	public void IdentifierTest()
	{
		var immutableIdentifier = SyntaxFactory.IdentifierName("Sample");
		var mutableIdentifier = MutableFactory.MutableIdentifierName(immutableIdentifier);
		
		Assert.That(mutableIdentifier.Name, Is.EqualTo("Sample"));

		var walker = new WalkIdentifier();
		walker.Visit(mutableIdentifier);
		
        Assert.Multiple(() =>
        {
            Assert.That(walker.Name, Is.EqualTo("Sample"));
            Assert.That(walker.MutableNode, Is.EqualTo(mutableIdentifier));
        });		
        
        var rewriter = new RewriteIdentifier();
		var rewritten = rewriter.Visit(mutableIdentifier);
		
		Assert.That(rewritten is IdentifierName { Name: "Rewritten" }, Is.True);

		var immutableAgain = MutableFactory.ImmutableIdentifierName((IdentifierName)rewritten!);
		
		Assert.That(immutableAgain.ToFullString(), Is.EqualTo("Rewritten"));
	}

	[Test]
	public void StatementListTest()
	{
		var immutableIdentifier = SyntaxFactory.ParseCompilationUnit(@"
			local a = 1
		").Statements;

		var mutableStatements = MutableFactory.MutableStatementList(immutableIdentifier);

		((LocalVariableDeclarationStatement)mutableStatements.Statements[0]).Names[0].Name = "Changed";

		var rewriter = new RewriteStatementList();
		var value = (StatementList)rewriter.Visit(mutableStatements)!;
		
		Assert.That(rewriter.Name, Is.EqualTo("Changed"));
		
		var list = MutableFactory.ImmutableStatementList(value);
		
		Assert.That(list.NormalizeWhitespace().ToFullString(), Is.EqualTo("Replaced = ..."));
	}	
	
	[Test]
	public void TableTest()
	{
		var immutableTable = (TableConstructorExpressionSyntax) SyntaxFactory.ParseExpression(@"{ 1, 2, 3, [4] = true, Identifier = nil }");
		var mutableTable = MutableFactory.MutableTableConstructorExpression(immutableTable);

		foreach (var field in mutableTable.Fields)
		{
			switch (field)
			{
				case UnkeyedTableField f:
				{
					Assert.That(MutableFactory.ImmutableUnkeyedTableField(f).IsKind(SyntaxKind.UnkeyedTableField));
					break;
				}
				case ExpressionKeyedTableField f:
				{
					Assert.That(MutableFactory.ImmutableExpressionKeyedTableField(f).IsKind(SyntaxKind.ExpressionKeyedTableField));
					break;
				}
				case IdentifierKeyedTableField f:
				{
					Assert.That(MutableFactory.ImmutableIdentifierKeyedTableField(f).IsKind(SyntaxKind.IdentifierKeyedTableField));
					break;
				}
			}
		}

		var walker = new WalkTable();
		walker.Visit(mutableTable);
		
		Assert.That(walker.Name, Is.EqualTo("Identifier"));

		var rewriter = new RewriteTable();
		var rewritten = (TableConstructorExpression)rewriter.Visit(mutableTable);
		
		var immutableAgain = MutableFactory.ImmutableTableConstructorExpression(rewritten);
		
		Console.WriteLine(immutableAgain.NormalizeWhitespace().ToFullString());
		
		Assert.That(immutableAgain.NormalizeWhitespace().ToFullString().Contains("[10] = true"));
	}

	[Test]
	public void TreeTest()
	{
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
	}	
	
	/*
		[Test]
		public void RegisterExpressionTest()
		{
			var parsedTree = LuaSyntaxTree.ParseText("");
			var mutableTree = MutableSyntaxTree.Create(parsedTree);

			mutableTree.Root = new CompilationUnit
			{
				Statements = [ new ExpressionStatement{ Expression = new RegisterExpression{ Register = 5 } } ]
			};
			
			var visitor = new VisitRegExp();
			visitor.Visit(mutableTree.Root);

			Assert.That(visitor.Value, Is.EqualTo(5));
		}
	 */
}