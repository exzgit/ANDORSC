using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Parser.NodeBuilder
{
	public class PrintNode : Node
	{
		public List<Node> Args;
		public int Line { get; }
		public string path { get; }
		public PrintNode(List<Node> args, int line, string path)
		{
			Args = args;
			Line = line;
			this.path = path;
		}

		public override void WriteNode(int indent = 0)
		{
			Console.Write(new string(' ', indent));
			Console.WriteLine($"PrintNode(");
			foreach (Node arg in Args)
			{
				arg.WriteNode(indent + 1);
			}
			Console.Write(new string(' ', indent));
			Console.WriteLine($")");
		}
	}
}
