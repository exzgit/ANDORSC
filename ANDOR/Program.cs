using ANDOR.src.Interpreter;
using ANDOR.src.Lexer;
using ANDOR.src.Parser;
using ANDOR.src.Sys;
using System;
using System.Collections.Generic;
using static ANDOR.src.Lexer.AOToken;
using System.IO;

public class Program
{
	private static readonly string examplepath = @".\CodeTest\example.px";

	static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("No file path provided.");
			return;
		}

		string filePath = args[0];

		// Menangani --example atau --ex
		if (filePath == "--example" || filePath == "--ex")
		{
			Console.WriteLine($"Displaying content of example file: {examplepath}");
			if (File.Exists(examplepath))
			{
				string content = File.ReadAllText(examplepath);
				Console.WriteLine(content);
			}
			else
			{
				Console.WriteLine($"Example file not found: {examplepath}");
			}
			return;
		}else if (filePath == "--help" || filePath == "-h")
		{
			Console.WriteLine("Command List:");
			Console.WriteLine("	--example or --ex ~> Menampilkan contoh kode.");
			Console.WriteLine("	--run or -r ~> Menjalankan file program.\n\t(Contoh: ANDOR.exe --run path\\to\\file.px)");
			Console.WriteLine();
		}
		else if (filePath == "--run" || filePath == "-r")
		{
			if (args.Length > 2)
			{
				RunInterpreter(args[1]);
				return;
			}
			else
			{
				Console.WriteLine("No file path provided.");
			}
		}

		if (!File.Exists(filePath))
		{
			Console.WriteLine($"File not found: {filePath}");
			return;
		}

		RunInterpreter(filePath);
	}

	static void RunInterpreter(string filePath)
	{
		string content = File.ReadAllText(filePath);

		AOLexer lexer = new AOLexer(content, filePath);
		AOToken token;

		List<AOToken> LToken = new List<AOToken>();

		while ((token = lexer.NextToken()).Type != TokenType._EOS_)
		{
			if (token.Type != TokenType.Whitespace && token.Type != TokenType.Comment
				 && token.Type != TokenType._ERROR_ && token.Type != TokenType._UNK_)
			{
				LToken.Add(token);
			}
			else if (token.Type == TokenType._ERROR_)
			{
				exception except = new exception(token.Value, token.Line, token.Path, exception.HandleType.Error, exception.ErrorType.SyntaxError);
			}
			else if (token.Type == TokenType._UNK_)
			{
				exception except = new exception(token.Value, token.Line, token.Path, exception.HandleType.Error, exception.ErrorType.SyntaxError);
			}
			else if (token.Type == TokenType.Whitespace)
			{
				continue;
			}
		}

		AST Nodes = new AST(LToken);
		Node Ast = Nodes.first_parser();

		//Ast.WriteNode();

		PX_Interpreter exctr = new PX_Interpreter(Ast);
		exctr.Execute();
	}
}
