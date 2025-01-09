using ANDOR.src.Lexer;
using ANDOR.src.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ANDOR.src.Interpreter
{
	public class ScopeManager
	{
		private readonly Stack<Dictionary<string, object>> StackScope = new Stack<Dictionary<string, object>>();
		private static readonly Stack<string> StackLoader = new Stack<string>();

		public static Stack<string> Instance
		{
			get { return StackLoader; }
		}

		public ScopeManager()
		{
			StackScope.Push(new Dictionary<string, object>());
		}

		public void EnterScope()
		{
			StackScope.Push(new Dictionary<string, object>());
		}

		public void ExitScope()
		{
			if (StackScope.Count > 1)
			{
				StackScope.Pop();
			}
			else
			{
				string Message = $"Scope Error -> Unexpected to exit from global scope";
				throw new Exception(Message);
			}
		}

		public void SetVariable(string name, object value)
		{
			StackScope.Peek()[name] = value;
		}

		public object GetVariable(string name)
		{
			foreach (var scope in StackScope)
			{
				if (scope.ContainsKey(name))
				{
					return scope[name];
				}
			}
			string Message = $"Scope Error -> Variable modifier is null or empty {name}";
			return Message;
		}

		public bool VariableExists(string name)
		{
			return StackScope.Peek().ContainsKey(name);
		}

		public void AddLoader(string path, int line)
		{
			_AddLoader(path, line);
		}

		public static void _AddLoader(string path, int line)
		{
			string combine = $"Loader::{"0x" + RuntimeHelpers.GetHashCode(path + line).ToString("X16")}\n\tPacket: {path}, Line: {line}";
			StackLoader.Push(combine);

			if (StackLoader.Count > 8)
			{
				StackLoader.Pop();
			}
		}

		public static List<string> GetDebugInfo()
		{
			return StackLoader.ToList();
		}

	}
}
