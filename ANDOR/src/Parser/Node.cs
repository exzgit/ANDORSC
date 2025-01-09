using ANDOR.src.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ANDOR.src.Parser
{
	public abstract class Node
	{
		public abstract void WriteNode(int indent = 0);
	}

	public class OveralNode : Node
	{
		public List<Node> Content;
		public OveralNode(List<Node> content)
		{
			Content = content;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine("Content[");
			foreach (var node in Content)
			{
				node.WriteNode(indent + 1);
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine("]");

		}
	}


	public class IntLiteral : Node
	{
		public int Value { get; }
		public string Type = "int";
		public int Line { get; }
		public string path { get; }

		public IntLiteral(int value, int line, string path)
		{
			Value = value;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"IntLiteral({Value.ToString()}) [{Line}]");
		}
	}



	public class FloatLiteral : Node
	{
		public float Value { get; }
		public string Type = "float";
		public int Line { get; }
		public string path { get; }

		public FloatLiteral(float value, int line, string path)
		{
			Value = value;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"FloatLiteral({Value.ToString()}) [{Line}]");
		}
	}

	public class BoolLiteral : Node
	{
		public bool Value { get; }
		public string Type = "bool";
		public int Line { get; }
		public string path { get; }

		public BoolLiteral(bool value, int line, string path)
		{
			Value = value;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"BoolLiteral({Value.ToString()}) [{Line}]");
		}
	}

	public class Nullable : Node
	{
		public string Value { get; }
		public string Type = "null";
		public int Line { get; }
		public string path { get; }

		public Nullable(string value, int line, string path)
		{
			Value = value;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"Nullable({Value.ToString()}) [{Line}]");
		}
	}

	public class StringLiteral : Node
	{
		public string Value { get; }
		public string Type = "string";
		public int Line { get; }
		public string path { get; }

		public StringLiteral(string value, int line, string path)
		{
			Value = value;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"StringLiteral({Value}) [{Line}]");
		}
	}
}