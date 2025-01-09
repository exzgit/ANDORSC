using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Parser.NodeBuilder
{
	public class ClassNode : Node
	{
		public string Name { get; set; }

		public string Modifier { get; set; }
		public List<Node> Body { get; }
		public int Line { get; }
		public string path { get; }
		public ClassNode(string name, List<Node> body, string modifier, int line, string path)
		{
			Name = name;
			Body = body;
			Modifier = modifier;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.Write(new string(' ', indent) + $"ClassNode=Name: {Name}, Modifier: {Modifier}, ");
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
