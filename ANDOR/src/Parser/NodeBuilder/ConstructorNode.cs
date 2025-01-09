using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Parser.NodeBuilder
{
	public class ConstructorNode : Node
	{
		public string Name { get; set; }
		public List<Node> Body { get; }
		public int Line { get; }
		public string path { get; }

		public ConstructorNode(string name, List<Node> body, int line, string path)
		{
			Name = name;
			Body = body;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.Write(new string(' ', indent) + $"ConstructNode=Name: {Name}, ");
			Console.WriteLine("Body: [");
			foreach (var node in Body)
			{
				node.WriteNode(indent + 1);
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine("]");
		}
	}
}
