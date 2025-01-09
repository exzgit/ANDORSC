using ANDOR.src.Interpreter;
using ANDOR.src.Lexer;
using ANDOR.src.Parser.NodeBuilder;
using ANDOR.src.Sys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ANDOR.src.Parser
{
	public class AST
	{
		private readonly List<AOToken> Token = new List<AOToken>();
		private int _position = 0;
		private ScopeManager exception = new ScopeManager();
		private string _currentClass = "";

		public AST(List<AOToken> tokens)
		{
			Token = tokens;
			_position = 0;
		}

		public Node first_parser()
		{
			List<Node> body = new List<Node>();
			while (_position < Token.Count)
			{
				if (Match("Keyword", "class"))
				{
					body.Add(parse_class("local"));
				}else if (Match("Keyword", "public"))
				{
					if (Match("Keyword", "class"))
					{
						body.Add(parse_class("public"));
					}
				}
				else
				{
					Console.WriteLine($"TOKEN: {Token[_position].Value} - TYPE: {Token[_position].Type}");
					exception except = new exception($"An unexpected token was found at position {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
					return null;
				}
			}

			return new OveralNode(body);
		}

		private enum ParserType
		{
			Class,
			Method,
			Block,
		}

		private ParserType parserType;

		private List<Node> parse_body(ParserType _type, bool returns)
		{
			var body = new List<Node>();

			Consume("LCurl", "{");

			while (!Match("RCurl", "}"))
			{
				if (Check("Comment"))
				{
					continue;
				}

				if (_type == ParserType.Class)
				{
					if (Match("Keyword", "public"))
					{
						if (Match("Keyword", "string"))
						{
							body.Add(CreateVariable("string", "public", true));
							Consume("Semi", ";");
						}
						else if (Match("Keyword", "int"))
						{
							body.Add(CreateVariable("int", "public", true));
							Consume("Semi", ";");
						}
						else if (Match("Keyword", "float"))
						{
							body.Add(CreateVariable("float", "public", true));
							Consume("Semi", ";");
						}
						else if (Match("Keyword", "bool"))
						{
							body.Add(CreateVariable("bool", "public", true));
							Consume("Semi", ";");
						}
						else if (Match("Keyword", "function"))
						{
							body.Add(parse_function("public"));
						}
						else if (Match("Identifier", _currentClass))
						{
							body.Add(parse_construct("public"));
						}
					}
					else if (Match("Keyword", "function"))
					{
						body.Add(parse_function("local"));
					}
					if (Match("Keyword", "string"))
					{
						body.Add(CreateVariable("string", "local", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "int"))
					{
						body.Add(CreateVariable("int", "local", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "float"))
					{
						body.Add(CreateVariable("float", "local", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "bool"))
					{
						body.Add(CreateVariable("bool", "local", true));
						Consume("Semi", ";");
					}
					else if (Match("Identifier", _currentClass))
					{
						body.Add(parse_construct("local"));
					}
					else
					{
						exception except = new exception($"An unexpected token was found at position {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
						return null;
					}
				}
				else if (_type == ParserType.Method)
				{
					if (Match("Keyword", "string"))
					{
						body.Add(CreateVariable("string", "private", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "int"))
					{
						body.Add(CreateVariable("int", "private", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "float"))
					{
						body.Add(CreateVariable("float", "private", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "bool"))
					{
						body.Add(CreateVariable("bool", "private", true));
						Consume("Semi", ";");
					}
					else if (Match("Keyword", "print"))
					{
						body.Add(parse_print());

					}else if (Match("Identifier"))
					{
						body.Add(parse_callers());
						Consume("Semi", ";");
					}
					else if (Check("Keyword", "return"))
					{
						body.Add(parse_return());
						Consume("Semi", ";");
					}
					else
					{
						exception except = new exception($"An unexpected token was found at position {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
						return null;
					}
				}
			}

			return body;
		}

		private Node parse_return()
		{
			string pathf = Token[_position].Path;
			int lines = Token[_position].Line;

			Consume("Keyword", "return");
			exception.AddLoader(pathf, lines);

			var args = new List<Node>();
			Consume("LParen", "(");
			if (!Check("RParen", ")"))
			{
				args.Add(parse_factor());
				
				while (Match("Comma", ","))
				{
					args.Add(parse_factor());
				}
			}

			Consume("RParen", ")");
			return new ReturnNode(args, lines, pathf);

		}

		private Node parse_callers()
		{
			List<string> Locator = new List<string>();
			AOToken Name = Previous();
			Locator.Add(Name.Value);

			exception.AddLoader(Name.Path, Name.Line);

			while (Match("Dot", "."))
			{
				if (Match("Identifier"))
				{
					Name = Previous();
					Locator.Add(Name.Value);
				}
				else
				{
					exception except = new exception($"An unexpected token was found at position {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
					return null;
				}
			}

			if (Check("LParen", "("))
			{
				List<Node> arguments = new List<Node>();
				if (Token[_position + 1].Value != ")")
				{
					arguments = Parameter("Invoker");
				}
				else
				{
					Consume("LParen", "(");
					Consume("RParen", ")");
				}

				return new AccessMethod(Name.Value, arguments, Locator, Name.Line, Name.Path);
			}

			return new AccessVariable(Name.Value, Locator, Name.Line, Name.Path);
		}

		private Node parse_print()
		{
			int line = Token[_position].Line;
			string path = Token[_position].Path;
			exception.AddLoader(path, line);

			var args = new List<Node>();
			Consume("LParen", "(");
			if (!Check("RParen", ")"))
			{
				args.Add(parse_factor());
				while (Match("Comma", ","))
				{
					args.Add(parse_factor());
				}
			}

			Consume("RParen", ")");
			Consume("Semi", ";");
			return new PrintNode(args, line, path);
		}

		private Node parse_class(string modifier)
		{
			string pathf = Token[_position].Path;
			int lines = Token[_position].Line;


			if (!Match("Identifier"))
			{
				exception except = new exception($"Expected an identifier token but got a token instead {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
				return null;
			}

			AOToken ClassName = Previous();
			exception.AddLoader(pathf, lines);

			_currentClass = ClassName.Value;

			var body = parse_body(ParserType.Class, false);

			_currentClass = "";
			return new ClassNode(ClassName.Value, body, modifier, lines, pathf);
		}

		private Node parse_construct(string modifier)
		{
			

			AOToken ConstructName = Previous();
			exception.AddLoader(ConstructName.Path, ConstructName.Line);

			List<Node> body = new List<Node>();
			var args = new ParameterBuilder(Parameter("function"), ConstructName.Value, ConstructName.Line, ConstructName.Path);
			body = parse_body(ParserType.Method, true);
			body.Add(args);
			return new ConstructorNode(ConstructName.Value, body, ConstructName.Line, ConstructName.Path);
		}

		private Node parse_function(string modifier)
		{
			if (!Match("Identifier"))
			{
				exception except = new exception($"Expected an identifier token but got a token instead {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
			}

			AOToken FunctionName = Previous();

			exception.AddLoader(FunctionName.Path, FunctionName.Line);

			List<Node> body = new List<Node>();
			var args = new ParameterBuilder(Parameter("function"), FunctionName.Value, FunctionName.Line, FunctionName.Path);
			body = parse_body(ParserType.Method, true);
			body.Add(args);
			return new MethodNode(FunctionName.Value, body, modifier, FunctionName.Line, FunctionName.Path);
		}

		private List<Node> Parameter(string argsType)
		{
			Consume("LParen", "(");
			var args = new List<Node>();
			if (!string.IsNullOrEmpty(argsType))
			{
				if (argsType == "function")
				{
					if (Match("Keyword", "string")){
						args.Add(CreateVariable("string", "private", true));
					}
					else if(Match("Keyword", "int"))
					{
						args.Add(CreateVariable("int", "private", true));
					}
					else if (Match("Keyword", "float"))
					{
						args.Add(CreateVariable("float", "private", true));
					}
					else if (Match("Keyword", "bool"))
					{
						args.Add(CreateVariable("bool", "private", true));
					}

					while (Check("Comma"))
					{
						Consume("Comma");
						if (Match("Keyword", "string"))
						{
							args.Add(CreateVariable("string", "private", true));
						}
						else if (Match("Keyword", "int"))
						{
							args.Add(CreateVariable("int", "private", true));
						}
						else if (Match("Keyword", "float"))
						{
							args.Add(CreateVariable("float", "private", true));
						}
						else if (Match("Keyword", "bool"))
						{
							args.Add(CreateVariable("bool", "private", true));
						}
					}
				}
				else
				{
					if (Match("Keyword", "string"))
					{
						args.Add(CreateVariable("string", "private", false));
					}
					else if (Match("Keyword", "int"))
					{
						args.Add(CreateVariable("int", "private", false));
					}
					else if (Match("Keyword", "float"))
					{
						args.Add(CreateVariable("float", "private", false));
					}
					else if (Match("Keyword", "bool"))
					{
						args.Add(CreateVariable("bool", "private", false));
					}
					else
					{
						args.Add(parse_factor());
					}

					while (Check("Comma"))
					{
						Consume("Comma");
						if (Match("Keyword", "string"))
						{
							args.Add(CreateVariable("string", "private", false));
						}
						else if (Match("Keyword", "int"))
						{
							args.Add(CreateVariable("int", "private", false));
						}
						else if (Match("Keyword", "float"))
						{
							args.Add(CreateVariable("float", "private", false));
						}
						else if (Match("Keyword", "bool"))
						{
							args.Add(CreateVariable("bool", "private", true));
						}
						else
						{
							args.Add(parse_factor());
						}
					}
				}
			}
			Consume("RParen", ")");
			return args;
		}

		private Node CreateVariable(string type, string modifier, bool useDeclare)
		{
			if (!Match("Identifier"))
			{
				exception except = new exception($"Expected an identifier token but got a token instead {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
				return null;
			}

			AOToken VariableName = Previous();

			exception.AddLoader(VariableName.Path, VariableName.Line);

			if (Match("Equal", "="))
			{
				if (!string.IsNullOrEmpty(type))
				{
					if (type == "string" && (Check("String", Token[_position].Value) || Check("Keyword", "null")))
					{
						var val = parse_factor();
						if (AOToken.TokenType.String != Previous().Type)
						{
							exception except = new exception($"Expected type {type} but gor {Previous().Type}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.TypeError);
						}

						return new VariableDefinitionNode(VariableName.Value, val, type, modifier, VariableName.Line, VariableName.Path);
					}
					else if (type == "int" && (Check("Number", Token[_position].Value) || Check("Keyword", "null")))
					{
						var val = parse_factor();
						if (AOToken.TokenType.Number != Previous().Type)
						{
							exception except = new exception($"Expected type {type} but gor {Previous().Type}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.TypeError);
						}

						return new VariableDefinitionNode(VariableName.Value, val, type, modifier, VariableName.Line, VariableName.Path);
					}
					else if (type == "float" && (Check("Number", Token[_position].Value) || Check("Keyword", "null")))
					{
						var val = parse_factor();
						if (AOToken.TokenType.Number != Previous().Type)
						{
							exception except = new exception($"Expected type {type} but gor {Previous().Type}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.TypeError);
						}

						return new VariableDefinitionNode(VariableName.Value, val, type, modifier, VariableName.Line, VariableName.Path);
					}
					else if (type == "bool" && (Check("Keyword", "true") || Check("Keyword", "false") || Check("Keyword", "null")))
					{
						var val = parse_factor();
						if ("true" != Previous().Value || "false" != Previous().Value)
						{
							exception except = new exception($"Expected type {type} but gor {Previous().Type}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.TypeError);
						}

						return new VariableDefinitionNode(VariableName.Value, val, type, modifier, VariableName.Line, VariableName.Path);
					}
					else if (Check("Identifier", Token[_position].Value))
					{
						var val = parse_factor();
						return new VariableDefinitionNode(VariableName.Value, val, type, modifier, VariableName.Line, VariableName.Path);
					}
					else
					{
						exception except = new exception($"Unexpected value {type}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.ValueError);
						return null;
					}
				}
				else
				{
					exception except = new exception($"Unrecognized type {type}, detection from CreateVariable(); methods.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.TypeError);
					return null;
				}
			}
			else if (Check("Semi", ";") && useDeclare == true)
			{
				Consume("Semi", ";");
				return new VariableDeclarationNode(VariableName.Value, type, null, modifier, VariableName.Line, VariableName.Path);
			}
			else if (Check("RParen", ")") && useDeclare == true)
			{
				return new VariableDeclarationNode(VariableName.Value, type, null, modifier, VariableName.Line, VariableName.Path);
			}
			else
			{
				exception except = new exception($" Unexpected tokens found in position {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.SyntaxError);
				return null;
			}
		}

		private Node parse_factor()
		{
			if (Match("Number", Token[_position].Value)){
				AOToken Number = Previous();
				if (Number.Value.Any(c => c == '.'))
				{
					return new FloatLiteral(float.Parse(Number.Value), Number.Line, Number.Path);
				}
				else
				{
					return new IntLiteral(int.Parse(Number.Value), Number.Line, Number.Path);
				}
			}
			if (Match("String", Token[_position].Value)) return new StringLiteral(Previous().Value, Token[_position - 1].Line, Token[_position - 1].Path);
			if (Match("Keyword", "true")) return new BoolLiteral(true, Token[_position - 1].Line, Token[_position - 1].Path);
			if (Match("Keyword", "false")) return new BoolLiteral(false, Token[_position - 1].Line, Token[_position - 1].Path);
			if (Match("Keyword", "null")) return new Nullable("null", Token[_position - 1].Line, Token[_position - 1].Path);
			if (Match("Identifier"))
			{
				return parse_callers();
			}
			exception except = new exception($"Unexpected tokens found in position {Token[_position].Value}.", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.ValueError);
			return null;
		}

		private AOToken Consume(string type, string value = "\0")
		{
			try
			{
				if (Check(type, value))
				{
					return Advance();
				}

				Console.WriteLine($"Expected {type} {value}, found {Peek().Type} {Peek().Value}");
				while (true)
				{
					continue;
				}
			}
			catch (Exception e)
			{
				exception except = new exception($"Sorry, the program got stuck in Consume(); when forming {Token[_position].Value}.\nDetails -> {e.Message}", Token[_position].Line, Token[_position].Path, Sys.exception.HandleType.Error, Sys.exception.ErrorType.ProgramCrash);
				return null;
			}

		}

		private bool Match(string type, params string[] values)
		{
			if (Check(type, values))
			{
				Advance();
				return true;
			}
			return false;
		}

		private bool Check(string type, params string[] values)
		{
			if (IsAtEnd()) return false;

			AOToken tokens = Peek();
			if (tokens.Type.ToString() != type) return false;

			return values.Length == 0 || Array.Exists(values, value => value == tokens.Value);
		}

		private AOToken Advance()
		{
			if (!IsAtEnd()) _position++;
			return Previous();
		}

		private void SkipWhitespace()
		{
			while (Check("Whitespace"))
			{
				Advance();
			}
		}

		private bool IsAtEnd() => _position >= Token.Count; 

		private AOToken Peek() => Token[_position];

		private AOToken Previous() => Token[_position - 1];
	}
}
