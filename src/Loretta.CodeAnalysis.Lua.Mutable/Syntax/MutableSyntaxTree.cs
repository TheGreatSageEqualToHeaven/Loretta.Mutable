using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Mutable.Syntax;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace Loretta.CodeAnalysis.Lua.Mutable;

public class MutableSyntaxTree
{
	public LuaSyntaxOptions SyntaxOptions { get; set; }
		
	public CompilationUnit Root { get; set; }
		
	public static MutableSyntaxTree Create(LuaSyntaxTree syntaxTree)
	{
		var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
			
		var mutable = MutableFactory.MutableCompilationUnit(root);
			
		return new MutableSyntaxTree
		{
			SyntaxOptions = syntaxTree.Options.SyntaxOptions,
			Root = mutable
		};
	}	
	
	public static MutableSyntaxTree Create(SyntaxTree syntaxTree)
	{
		return Create((LuaSyntaxTree)syntaxTree);
	}
}