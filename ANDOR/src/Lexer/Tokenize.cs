using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ANDOR.src.Lexer.AOToken;

namespace ANDOR.src.Lexer
{
	public class Tokenize
	{
		public List<AOToken> Tokenizer(string code)
		{
			var Token = new List<AOToken>();

			AOLexer lexer = new AOLexer(code, "\0");
			AOToken token;
			while ((token = lexer.NextToken()).Type != TokenType._EOS_)
			{
				Token.Add(token);
			}

			return Token;
		}
	}
}
