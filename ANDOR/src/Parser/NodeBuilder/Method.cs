using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ANDOR.src.Parser.NodeBuilder
{
	public class MethodNode : Node
	{
		public string Name { get; set; }
		public string Modifier { get; set; }
		public List<Node> Body { get; }
		public int Line { get; }
		public string path { get; }

		public MethodNode(string name, List<Node> body, string modifier, int line, string path)
		{
			Name = name;
			Body = body;
			Modifier = modifier;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.Write(new string(' ', indent) + $"MethodNode=Name: {Name}, Modifier: {Modifier}, ");
			Console.WriteLine("Body: [");
			foreach (var node in Body)
			{
				node.WriteNode(indent + 1);
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine("]");
		}
	}

	public class ParameterBuilder : Node
	{
		public string Name { get; }
		public List<Node> Body { get; }

		public ParameterBuilder(List<Node> args, string methodName, int line, string path)
		{
			Name = methodName;
			Body = args;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.Write(new string(' ', indent) + $"Arguments=Name: {Name}, Body: [\n");

			foreach (var node in Body)
			{
				node.WriteNode(indent + 1);
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine("]");
		}
	}

	public class AccessMethod : Node
	{
		public string Name { get; set; }
		public List<Node> Args { get; set; }
		public List<string> Locator { get; set; }
		public int Line { get; }
		public string path { get; }

		public AccessMethod(string name, List<Node> args, List<string> locator, int line, string path)
		{
			Name = name;
			Args = args;
			Locator = locator;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"CallMethod(Name: {Name}, Args: [");
			foreach (Node node in Args)
			{
				node.WriteNode(indent + 1);
			}
			Console.Write("], ");
			Console.WriteLine("Scope: [");
			foreach (string node in Locator)
			{
				Console.WriteLine(node + ", ");
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine("]");
		}
	}
}
