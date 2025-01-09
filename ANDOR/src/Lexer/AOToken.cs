using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Lexer
{
    public class AOToken
    {
        public enum TokenType
        {
            LParen,
            RParen,
            LCurl,
            RCurl,
            LCube,
            RCube,
            Pipe,
            Semi,
            Colon,
            Plus,
            Minus,
            Equal,
            Exclamation,
            Compound,
            Dolar,
            At,
            Quotation,
            OneQuotation,
            Star,
            Amper,
            Caret,
            Percent,
            Comma,
            Dot,
            Question,
            Slash,
            BSlash,
            Greater,
            Less,
            Keyword,
            Identifier,
            Number,
            String,
            Whitespace,
            Comment,
            BRLine,
            _UNK_,
            _EOS_,
            _ERROR_,
            _TRACE_
        }

        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public string Path { get; set; }

        public AOToken(TokenType type, string value, int line, string path)
        {
            Type = type;
            Value = value;
            Line = line;
            Path = path;
        }

        public override string ToString() => $"Type({Type}).Value({Value})[{Line}][{Path}]";
    }

    public class AOKeyword
    {
        public List<string> key = new List<string>
        {
            "public",
            "private",
            "function",
            "class",
            "print",
            "if",
            "else",
            "for",
            "while",
            "this",
            "switch",
            "case",
            "var",
            "null",
            "true",
            "false",
            "string",
			"float",
            "int",
            "bool",
            "list",
            "dict",
            "new",
            "type",
            "length",
            "get",
            "set",
            "idx",
            "max",
            "min",
            "return",
            "break",
            "include",
            "add",
            "rem",
            "lock",
            "mstack",
            "byte",
            "enum",
        };
    }
}
