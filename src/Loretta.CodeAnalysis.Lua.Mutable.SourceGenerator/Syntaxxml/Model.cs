using System.Collections.Generic;
using System.Xml.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Loretta.CodeAnalysis.Lua.Mutable.SourceGenerator.Syntaxxml;

[XmlRoot("Tree")]
public class Tree
{
	[XmlAttribute("Root")]
	public string Root;

	[XmlElement("Node")]
	public List<Node> Types;
}

public class Node
{
	[XmlAttribute("Name")]
	public string Name;

	[XmlAttribute("Base")]
	public string Base;

	[XmlElement("Kind")]
	public Kind Kind;

	[XmlElement("Fields")]
	public FieldsContainer Fields;	
	
	[XmlElement("SourceNode")]
	public SourceNode SourceNode;
}

public class Kind
{
	[XmlAttribute("Name")]
	public string Name;
}

public class FieldsContainer
{
	[XmlElement("Field")]
	public List<Field> Fields;
}

public class Field
{
	[XmlAttribute("Name")]
	public string Name;

	[XmlAttribute("Type")]
	public string Type;
	
	[XmlAttribute("Init")]
	public string? Init;
}

public class SourceNode
{
	[XmlAttribute("Name")]
	public string Name;	
	
	[XmlAttribute("NoImmutable")]
	public string? NoImmutable;
	
	[XmlElement("Factories")]
	public FactoryContainer Factories;		
	
	[XmlElement("SourceFactories")]
	public SourceFactoryContainer? SourceFactories;	
}

public class FactoryContainer
{
	[XmlElement("Factory")]
	public List<Factory> Factories;
}

public class Factory
{
	[XmlAttribute("Original")]
	public string Original;

	[XmlAttribute("New")]
	public string New;	
	
	[XmlAttribute("Alt")]
	public string? Alt;
}

public class SourceFactoryContainer
{
	[XmlAttribute("FactoryName")]
	public string FactoryName;	
	
	[XmlElement("Factory")]
	public List<SourceFactory> Factories;
}

public class SourceFactory
{
	[XmlAttribute("Field")]
	public string Field;
	
	[XmlAttribute("Alt")]
	public string? Alt;	
	
	[XmlAttribute("Type")]
	public string? Type;	
	
	[XmlAttribute("SourceType")]
	public string? SourceType;
}