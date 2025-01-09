using ANDOR.src.Lexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Parser.NodeBuilder
{
	public class VariableDefinitionNode : Node
	{
		public string Modifier { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public Node Value { get; set; }
		public int Line { get; }
		public string path { get; }
		public VariableDefinitionNode(string name, Node value, string type, string modifier, int line, string path)
		{
			Type = type;
			Name = name;
			Value = value;
			Modifier = modifier;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"VariableDefinition(Name: {Name}, Type: {Type}, Modifier: {Modifier}, Value: {Value})");
		}
	}

	public class AccessVariable : Node
	{
		public string Name { get; set; }
		public List<string> Locator { get; set; }
		public int Line { get; }
		public string path { get; }
		public AccessVariable(string name, List<string> locator, int line, string path)
		{
			Name = name;
			Locator = locator;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.Write(new string(' ', indent) + $"CallVariable(Name: {Name}, ");
			Console.WriteLine("Scope: [");
			foreach (string node in Locator)
			{
				Console.WriteLine(node + ", ");
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine("]");
		}
	}

	public class VariableDeclarationNode : Node
	{
		public string Modifier { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public Node Value { get; set; }
		public int Line { get; }
		public string path { get; }
		public VariableDeclarationNode(string name, string type, Node value, string modifier, int line, string path)
		{
			Type = type;
			Name = name;
			Value = value;
			Modifier = modifier;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"VariableDeclaration(Name: {Name}, Type: {Type}, Modifier: {Modifier})");
		}
	}
}
