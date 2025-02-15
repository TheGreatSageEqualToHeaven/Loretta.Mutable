using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Loretta.CodeAnalysis.Lua.Mutable.SourceGenerator;
using Loretta.CodeAnalysis.Lua.Mutable.SourceGenerator.Syntaxxml;
using Microsoft.CodeAnalysis;

namespace SourceAnalysis.MutableLoretta.SourceGenerator;

[Generator]
public class SyntaxGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this generator.
    }

#pragma warning disable RS2008
    private static readonly DiagnosticDescriptor s_missingSyntaxXml = new(
	    "LSSG1001",

	    title: "Syntax.xml is missing",
        messageFormat: "The Syntax.xml file was not included in the project, so we are not generating source",
        category: "SyntaxGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_incorrectXml = new(
        "LSSG1002",
        title: "Syntax.xml is incorrect",
        messageFormat: "The Syntax.xml file was incorrectly made in the project, so we are not generating source",
        category: "SyntaxGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
#pragma warning restore RS2008

    public void Execute(GeneratorExecutionContext context)
    {
        var inputProvider = context.AdditionalFiles.FirstOrDefault(text => Path.GetFileName(text.Path) == "Syntax.xml");

        if (inputProvider is null)
        {
            Diagnostic.Create(s_incorrectXml, location: null);
            throw new Exception("No xml was provided!");
        }

        var inputText = inputProvider.GetText();

        Tree tree;
        var reader = XmlReader.Create(new SourceTextReader(inputText!),
            new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

        try
        {
            var serializer = new XmlSerializer(typeof(Tree));
            tree = (Tree)serializer.Deserialize(reader);
        }
        catch
        {
            Diagnostic.Create(s_incorrectXml, location: null);
            throw new Exception("Incorrectly parsed xml!");
        }

        var factoryStringBuilder = new StringBuilder().Append(@"
			namespace Loretta.CodeAnalysis.Lua.Mutable;

			public static partial class MutableFactory {

        ");
        var genericFactoryStringBuilder = new StringBuilder().Append(@"
			namespace Loretta.CodeAnalysis.Lua.Mutable;
			
			public static partial class MutableFactory {
				public static Result_Type ToImmutable(this Source_Type input) {
					MutableSyntaxNode castedInput = (MutableSyntaxNode)input;
					switch (castedInput) {
					
        ");
        var reverseFactoryStringBuilder = new StringBuilder().Append(@"
			namespace Loretta.CodeAnalysis.Lua.Mutable;
			
			public static partial class MutableFactory {
				public static Result_Type ToMutable(this Source_Type input) {
					SyntaxNode castedInput = (SyntaxNode)input;
					switch (castedInput) {
					
        ");
        var visitorStringBuilder = new StringBuilder().Append(@"
			using Loretta.CodeAnalysis.Lua.Mutable.Syntax;
			
			namespace Loretta.CodeAnalysis.Lua.Mutable;
			
			public abstract partial class MutableSyntaxWalker { 
				
			
        ");
	    var genericVisitorStringBuilder = new StringBuilder().Append(@"
			using Loretta.CodeAnalysis.Lua.Mutable.Syntax;
			
			namespace Loretta.CodeAnalysis.Lua.Mutable;
			
			public abstract partial class MutableSyntaxWalker { 
				public virtual void Visit(MutableSyntaxNode node) {
					switch (node) {
						
        ");
        var rewriterStringBuilder = new StringBuilder().Append(@"
			using Loretta.CodeAnalysis.Lua.Mutable.Syntax;
			
			namespace Loretta.CodeAnalysis.Lua.Mutable;
			
			public abstract partial class MutableSyntaxRewriter { 
				
			
        ");
	    var genericRewriterStringBuilder = new StringBuilder().Append(@"
			using Loretta.CodeAnalysis.Lua.Mutable.Syntax;
			
			namespace Loretta.CodeAnalysis.Lua.Mutable;
			
			public abstract partial class MutableSyntaxRewriter { 
				public virtual MutableSyntaxNode? Visit(MutableSyntaxNode? node) {
					switch (node) {
						
        ");

	    void CreateVisitor(Node node)
	    {
		    visitorStringBuilder.Append($@"
				public virtual void Visit{node.Name}({node.Name} node) {{
					
				
		    ");

		    rewriterStringBuilder.Append($@"
				public virtual MutableSyntaxNode? Visit{node.Name}({node.Name} node) {{
		    
		    ");

		    var listSize = 0;
		    
		    foreach (var field in node.SourceNode.Factories.Factories)
		    {
			    var baseField = node.Fields.Fields.First(other => other.Name == field.New);
			    
			    switch (field.Alt)
			    {
				    case "Node" or "PrefixNode":
				    {
						visitorStringBuilder.Append($"Visit(node.{field.New});");
						rewriterStringBuilder.Append($"node.{field.New} = Visit(node.{field.New}) as {baseField.Type};".Replace("?", ""));
					    break;
				    }				    
				    case "List":
				    {
					    visitorStringBuilder.Append($@"
							foreach (var child in node.{field.New}) {{
								Visit(child);
							}}
							
						");
						
					    var input = baseField.Type;
					    var pattern = @"<(.*?)>";

					    var match = Regex.Match(input, pattern);
					    
					    var baseType = match.Groups[1].Value;
					    
					    rewriterStringBuilder.Append($@"
							var list{listSize} = new {baseField.Type}();
							
							foreach (var child in node.{field.New}) {{
								var visited = Visit(child) as {baseType};
								
								if (visited is not null)
									list{listSize}.Add(visited);
							}}
							
							node.{field.New} = list{listSize};
					    ".Replace("?", ""));
					    ++listSize;
					    break;
				    }
			    }
				    
		    }

		    visitorStringBuilder.Append("\n}");
		    rewriterStringBuilder.Append("return node; \n}");

		    genericVisitorStringBuilder.Append($@"
				case {node.Name} casted: {{ 
					Visit{node.Name}(casted);
					break;
				}}
				
		    ");		    
		    genericRewriterStringBuilder.Append($@"
				case {node.Name} casted: {{ 
					return Visit{node.Name}(casted);
				}}
				
		    ");
	    }
            
        void CreateMutableFactory(Node node)
        {
            var conversions = new StringBuilder();

            var lists = 0;
            foreach (var field in node.SourceNode.Factories.Factories)
            {
                switch (field.Alt)
                {
                    case "List":
                    {
                        conversions.Append($@"
				var list_{lists} = node.{field.New};
				foreach (var item in sourceNode.{field.Original}) {{
					list_{lists}.Add(item.ToMutable());
				}}
				");
                        ++lists;
                        break;
                    }
                    case "Node":
                    {
                        conversions.Append(
                            $"node.{field.New} = sourceNode.{field.Original}.ToMutable();\n");
                        break;
                    }                    
                    case "NullableNode":
                    {
                        conversions.Append(
                            $"node.{field.New} = sourceNode.{field.Original}?.ToMutable();\n");
                        break;
                    }
                    default:
                    {
                        conversions.Append($"node.{field.New} = sourceNode.{field.Original};\n");
                        break;
                    }
                }
            }

            var source = $@"
	
	public static {node.Name} Mutable{node.Name}({node.SourceNode.Name} sourceNode) {{
		var node = new {node.Name}();
		node.SourceNode = sourceNode;
		
		{conversions}
		
		return node;
	}}
	
	";
            
            factoryStringBuilder.Append(source);
            factoryStringBuilder.Append($@"
				public static {node.Name} ToMutable(this {node.SourceNode.Name} sourceNode) {{
					return  Mutable{node.Name}(sourceNode);
				}}
            ");
            reverseFactoryStringBuilder.Append($@"
				case {node.SourceNode.Name} n: {{
					return (Result_Type)(MutableSyntaxNode)n.ToMutable();
				}}
				
            ");
        }

        void CreateSourceFactory(Node node)
        {
            var conversions = new StringBuilder();
            var args = new StringBuilder();

            var argsCount = 0;
			
			
            foreach (var field in node.SourceNode.SourceFactories!.Factories)
            {
                switch (field.Alt)
                {
	                case "Identifier":
	                {
		                if (field == node.SourceNode.SourceFactories.Factories.Last())
		                {
			                args.Append($"arg{argsCount}");
		                }
		                else
		                {
			                args.Append($"arg{argsCount},");
		                }

		                conversions.Append($"var arg{argsCount} = SyntaxFactory.Identifier(sourceNode.{field.Field});");
		                break;
	                }
	                case "SimpleFunctionName":
	                {
		                if (field == node.SourceNode.SourceFactories.Factories.Last())
		                {
			                args.Append($"arg{argsCount}");
		                }
		                else
		                {
			                args.Append($"arg{argsCount},");
		                }

		                conversions.Append($"var arg{argsCount} = SyntaxFactory.SimpleFunctionName(SyntaxFactory.Identifier(sourceNode.{field.Field}));");
		                break;
	                }
	                case "Literal":
	                {
		                if (field == node.SourceNode.SourceFactories.Factories.Last())
		                {
			                args.Append($"arg{argsCount}");
		                }
		                else
		                {
			                args.Append($"arg{argsCount},");
		                }

		                conversions.Append(
			                @$"
			                
			                SyntaxToken arg{argsCount};
			                
			                switch (sourceNode.SourceSyntaxKind) {{
								case SyntaxKind.NilLiteralExpression: 
								case SyntaxKind.TrueLiteralExpression: 
								case SyntaxKind.FalseLiteralExpression: 
								{{
									return SyntaxFactory.LiteralExpression(sourceNode.SourceSyntaxKind);
								}}
								case SyntaxKind.NumericalLiteralExpression: {{
									var data = sourceNode.{field.Field};
									switch (data) {{
										case long l: {{
											arg{argsCount} = SyntaxFactory.Literal(l);
											goto label_{argsCount};
										}}										
										case int i: {{
											arg{argsCount} = SyntaxFactory.Literal(i);
											goto label_{argsCount};
										}}										
										case double d: {{
											arg{argsCount} = SyntaxFactory.Literal(d);
											goto label_{argsCount};
										}}
									}}
									throw new Exception(""Unsupported number literal type!"");
								}}								
								case SyntaxKind.StringLiteralExpression: {{
									arg{argsCount} = SyntaxFactory.Literal((string)sourceNode.{field.Field});
									break;
								}}
								default: {{
									throw new Exception(""Unsupported literal syntax kind!"");
								}}
			                }}
			                
			                label_{argsCount}:;
								
			                ");
		                break;
	                }
                    case "List":
                    {
                        if (field == node.SourceNode.SourceFactories.Factories.Last())
                        {
                            args.Append($"arg{argsCount}");
                        }
                        else
                        {
                            args.Append($"arg{argsCount},");
                        }
                        
                        conversions.Append($@"
				var list{argsCount} = new List<{field.Type}>();
				foreach (var item in sourceNode.{field.Field}) {{
					list{argsCount}.Add(({field.Type})item.ToImmutable());
				}}
				var arg{argsCount} = SyntaxFactory.List<{field.Type}>(list{argsCount});
				");
                        ++argsCount;
                        break;
                    }                    
                    case "SeparatedList":
                    {
                        if (field == node.SourceNode.SourceFactories.Factories.Last())
                        {
                            args.Append($"arg{argsCount}");
                        }
                        else
                        {
                            args.Append($"arg{argsCount},");
                        }

                        conversions.Append($@"
				var list{argsCount} = new List<{field.Type}>();
				foreach (var item in sourceNode.{field.Field}) {{
					list{argsCount}.Add(({field.Type})item.ToImmutable());
				}}
				var arg{argsCount} = SyntaxFactory.SeparatedList<{field.Type}>(list{argsCount});
				");
                        ++argsCount;
                        break;
                    }
                    case "ParameterList":
                    {
	                    if (field == node.SourceNode.SourceFactories.Factories.Last())
	                    {
		                    args.Append($"arg{argsCount}");
	                    }
	                    else
	                    {
		                    args.Append($"arg{argsCount},");
	                    }
	                    
	                    conversions.Append($@"
				var list{argsCount} = new List<ParameterSyntax>();
				foreach (var item in sourceNode.{field.Field}) {{
					list{argsCount}.Add((ParameterSyntax)item.ToImmutable());
				}}
				var arg{argsCount} = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(list{argsCount}));
				");
	                    ++argsCount;
	                    break;
                    }
                    case "StatementList":
                    {
	                    if (field == node.SourceNode.SourceFactories.Factories.Last())
	                    {
		                    args.Append($"arg{argsCount}");
	                    }
	                    else
	                    {
		                    args.Append($"arg{argsCount},");
	                    }
	                    
	                    conversions.Append($@"
				var list{argsCount} = new List<StatementSyntax>();
				foreach (var item in sourceNode.{field.Field}.Statements) {{
					list{argsCount}.Add((StatementSyntax)item.ToImmutable());
				}}
				var arg{argsCount} = SyntaxFactory.StatementList(list{argsCount});
				".Replace(".RemoveDot", ""));
	                    ++argsCount;
	                    break;
                    }                    
                    case "Node":
                    {
                        if (field == node.SourceNode.SourceFactories.Factories.Last())
                        {
                            args.Append($"arg{argsCount}");
                        }
                        else
                        {
                            args.Append($"arg{argsCount},");
                        }

                        conversions.Append(
                            $"var arg{argsCount} = sourceNode.{field.Field}.ToImmutable();\n");
                        break;
                    }                                  
                    case "NullableNode":
                    {
                        if (field == node.SourceNode.SourceFactories.Factories.Last())
                        {
                            args.Append($"arg{argsCount}");
                        }
                        else
                        {
                            args.Append($"arg{argsCount},");
                        }

                        conversions.Append(
                            $"var arg{argsCount} = sourceNode.{field.Field}?.ToImmutable();\n");
                        break;
                    }                    
                    case "PrefixNode":
                    {
                        if (field == node.SourceNode.SourceFactories.Factories.Last())
                        {
                            args.Append($"arg{argsCount}");
                        }
                        else
                        {
                            args.Append($"arg{argsCount},");
                        }

                        conversions.Append(
                            $"var arg{argsCount} = (PrefixExpressionSyntax)sourceNode.{field.Field}.ToImmutable();\n");
                        break;
                    }
                    default:
                    {
                        if (field == node.SourceNode.SourceFactories.Factories.Last())
                        {
                            args.Append($"arg{argsCount}");
                        }
                        else
                        {
                            args.Append($"arg{argsCount},");
                        }

                        conversions.Append($"var arg{argsCount} = sourceNode.{field.Field};\n");
                        break;
                    }
                }

                ++argsCount;
            }

            var source = $@"
	
	public static {node.SourceNode.Name} Immutable{node.Name}({node.Name} sourceNode) {{
		{conversions}
		
		var node = SyntaxFactory.{node.SourceNode.SourceFactories.FactoryName}({args});

		return node;
	}}
	
	";

            factoryStringBuilder.Append(source);
            factoryStringBuilder.Append($@"
				public static {node.SourceNode.Name} ToImmutable(this {node.Name} sourceNode) {{
					return  Immutable{node.Name}(sourceNode);
				}}
            ");
            genericFactoryStringBuilder.Append($@"
				case {node.Name} n: {{
					return (Result_Type)(SyntaxNode)n.ToImmutable();
				}}
            ");
        }

        string CreateNodeClass(Node node)
        {
	        CreateVisitor(node);
	        if (node.SourceNode.NoImmutable is not "true")
				CreateMutableFactory(node);
            if (node.SourceNode.SourceFactories is not null)
                CreateSourceFactory(node);

            var fields = node.Fields;

            var fieldsString = new StringBuilder();

            foreach (var field in fields.Fields)
            {
                switch (field.Init)
                {
                    case "List":
                    {
                        fieldsString.Append($"public {field.Type} {field.Name} {{ get; set; }} = new();\n");
                        break;
                    }
                    default:
                    {
                        fieldsString.Append($"public {field.Type} {field.Name} {{ get; set; }}\n");
                        break;
                    }
                }
            }

            fieldsString.Append("public SyntaxNode SourceNode { get; set; }");

            var source = $$"""
                           
                           	public partial class {{node.Name}} : {{node.Base}}
                           	{
                           		public override MutableSyntaxKind Kind { get; set; } = MutableSyntaxKind.{{node.Name}};
                           		
                           		{{fieldsString}}
                           	}
                           	
                           """;

            return source;
        }

        foreach (var x in tree.Types)
        {
            context.AddSource($"{x.Name}.g.cs", 
                "namespace Loretta.CodeAnalysis.Lua.Mutable.Syntax;\nusing Loretta.CodeAnalysis.Lua.Syntax;\n" +
                CreateNodeClass(x));
        }

        factoryStringBuilder.Append('}');
        genericFactoryStringBuilder.Append(@" } 
        throw new Exception(""Out of bounds in generic function!"");
        } }");
        reverseFactoryStringBuilder.Append(@" } 
        throw new Exception(""Out of bounds in generic function!"");
        } }");        
        visitorStringBuilder.Append('}');
        genericVisitorStringBuilder.Append(@" } 
        } }");        
        rewriterStringBuilder.Append('}');
        genericRewriterStringBuilder.Append(@" } 
        return null;
        } }");


        context.AddSource("MutableFactory.g.cs", 
            "using Loretta.CodeAnalysis.Lua.Mutable.Syntax;\nusing Loretta.CodeAnalysis.Lua.Syntax;\n" + 
            factoryStringBuilder.ToString());
        
        var combos = new Dictionary<string, string>
        {
			{ "MutableStatement", "StatementSyntax" },
			{ "MutableExpression", "ExpressionSyntax" },
			{ "MutableFunctionArgument", "FunctionArgumentSyntax" },
			{ "MutableTableField", "TableFieldSyntax" },
			{ "MutableParameter", "ParameterSyntax" },
			{ "MutableFunctionName", "FunctionNameSyntax" },
        };

		context.AddSource("MutableSyntaxWalker.g.cs", visitorStringBuilder.ToString());
		context.AddSource("MutableSyntaxGenericWalker.g.cs", genericVisitorStringBuilder.ToString());
		context.AddSource("MutableSyntaxRewriter.g.cs", rewriterStringBuilder.ToString());
		context.AddSource("MutableSyntaxGenericRewriter.g.cs", genericRewriterStringBuilder.ToString());

		var addedFactories = new HashSet<string>();
		
        foreach (var pair in combos)
        {
	        if (!addedFactories.Contains(pair.Key))
	        {
		        context.AddSource($"MutableFactory{pair.Key}.g.cs",
			        "using Loretta.CodeAnalysis.Lua.Mutable.Syntax;\nusing Loretta.CodeAnalysis.Lua.Syntax;\n" +
			        genericFactoryStringBuilder.ToString().Replace("Source_Type", pair.Key).Replace("Result_Type", pair.Value));

		        addedFactories.Add(pair.Key);
	        }

	        if (!addedFactories.Contains(pair.Value))
	        {
		        context.AddSource($"MutableFactory{pair.Value}.g.cs", 
			        "using Loretta.CodeAnalysis.Lua.Mutable.Syntax;\nusing Loretta.CodeAnalysis.Lua.Syntax;\n" + 
			        reverseFactoryStringBuilder.ToString().Replace("Source_Type", pair.Value).Replace("Result_Type", pair.Key));

		        addedFactories.Add(pair.Value);
	        }
        }
        
        context.AddSource($"MutableFactoryPrefixExpressionSyntax.g.cs",
	        "using Loretta.CodeAnalysis.Lua.Mutable.Syntax;\nusing Loretta.CodeAnalysis.Lua.Syntax;\n" +
	        reverseFactoryStringBuilder.ToString().Replace("Source_Type", "PrefixExpressionSyntax").Replace("Result_Type", "MutableExpression"));
    }
}