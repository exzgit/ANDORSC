using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Parser.NodeBuilder
{
	public class InstanceNode : Node
	{
		public string Name { get; set; }
		public List<Node> Args { get; set; }
		public List<string> Locator { get; set; }
		public int Line { get; }
		public string path { get; }

		public InstanceNode(string name, List<Node> args, List<string> locator, int line, string path)
		{
			Name = name;
			Args = args;
			Locator = locator;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.WriteLine(new string(' ', indent) + $"CallInstance(Name: {Name}, Args: [");
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
