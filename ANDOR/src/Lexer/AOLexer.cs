using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using static ANDOR.src.Lexer.AOToken;
using static ANDOR.src.Lexer.AOKeyword;
using static ANDOR.src.Sys.exception;
using System.Diagnostics;
using ANDOR.src.Sys;

namespace ANDOR.src.Lexer
{
    public class AOLexer
    {
        private string _input = "";
        private string WorkingDirectory = Directory.GetCurrentDirectory();
        private int _position = 0;
        private int _line = 1;
		private string _path = "";

		public AOLexer(string input, string path)
        {
            _input = input;
            _position = 0;
            _line = 1;
			_path = path;
        }


        private AOToken GetNumberToken()
        {
            int startPos = _position;
            bool C_Minus = true;
            bool C_Float = true;
            string value = "";

            if (_input[_position] == '-' && C_Minus == true)
            {
                value += _input[_position];

				_position++;
				C_Minus = false;
            }

            while (_position < _input.Length)
            {
                if (_input[_position] == '.' && C_Float == true)
                {
					value += _input[_position];
					_position++;
					C_Float = false;
                }
                else if (char.IsLetter(_input[_position]))
                {
                    string msg = "Unexpected " + _input[_position];
                    return new AOToken(TokenType._ERROR_, msg, _line, _path);
				}
                else if (char.IsDigit(_input[_position]))
                {
					value += _input[_position];
					_position++;
                }
                else
                {
                    break;
                }
            }

            return new AOToken(TokenType.Number, _input.Substring(startPos, _position - startPos), _line, _path);
        }

        private AOToken GetIdentifierToken()
        {
            int startPos = _position;
            while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            {
                _position++;
            }
            string identifier = _input.Substring(startPos, _position - startPos);
            AOKeyword keyword = new AOKeyword();
            if (keyword.key.Contains(identifier))
            {
                return new AOToken(TokenType.Keyword, identifier, _line, _path);
            }
            return new AOToken(TokenType.Identifier, identifier, _line, _path);
        }

        private AOToken GetSymbolToken()
        {
            char c = _input[_position];

            switch (c)
            {
                case '+':
                    _position++;
                    return new AOToken(TokenType.Plus, c.ToString(), _line, _path);
                case '-':
                    _position++;
                    return new AOToken(TokenType.Minus, c.ToString(), _line, _path);
                case '*':
                    _position++;
                    return new AOToken(TokenType.Star, c.ToString(), _line, _path);
                case '/':
                    _position++;
                    if (_input[_position] == '/')
                    {
						_position++;
                        string value = "";

						while (_input[_position] != '\n' || _position >= _input.Length)
                        {
                            value += _input[_position];
                            _position++;

                            if (_position >= _input.Length)
                            {
                                return new AOToken(TokenType._ERROR_, "The error is caused by an infinity loop that occurs when parsing comment tokens that do not have an ending such as a newline or */.", _line, _path);
                            }
                        }

                        return new AOToken(TokenType.Comment, value, _line, _path);
                    }else if (_input[_position] == '*')
                    {
						_position++;
						string value = "";

						while (true || _position >= _input.Length)
						{
							value += _input[_position];
                            if (_input[_position] == '*')
                            {
								if (_position < _input.Length)
                                {
									_position++;
                                }
                                else
                                {
									return new AOToken(TokenType._ERROR_, "The error is caused by an infinity loop that occurs when parsing comment tokens that do not have an ending such as a newline or */.", _line, _path);
								}

								if (_input[_position] == '/')
                                {
                                    break;
                                }

                                value += _input[_position];
							}
							_position++;
                                

							if (_position >= _input.Length)
							{
								return new AOToken(TokenType._ERROR_, "The error is caused by an infinity loop that occurs when parsing comment tokens that do not have an ending such as a newline or */.", _line, _path);
							}
						}

                        _position++;
						return new AOToken(TokenType.Comment, value, _line, _path);
					}
                    return new AOToken(TokenType.Slash, c.ToString(), _line, _path);
                case '=':
                    _position++;
                    return new AOToken(TokenType.Equal, c.ToString(), _line, _path);
                case '<':
                    _position++;
                    return new AOToken(TokenType.Less, c.ToString(), _line, _path);
                case '>':
                    _position++;
                    return new AOToken(TokenType.Greater, c.ToString(), _line, _path);
                case '!':
                    _position++;
                    return new AOToken(TokenType.Exclamation, c.ToString(), _line, _path);
                case '@':
                    _position++;
                    return new AOToken(TokenType.At, c.ToString(), _line, _path);
                case '#':
                    _position++;
                    return new AOToken(TokenType.Compound, c.ToString(), _line, _path);
                case '$':
                    _position++;
                    return new AOToken(TokenType.Dolar, c.ToString(), _line, _path);
                case '%':
                    _position++;
                    return new AOToken(TokenType.Percent, c.ToString(), _line, _path);
                case '^':
                    _position++;
                    return new AOToken(TokenType.Caret, c.ToString(), _line, _path);
                case '&':
                    _position++;
                    return new AOToken(TokenType.Amper, c.ToString(), _line, _path);
                case '(':
                    _position++;
                    return new AOToken(TokenType.LParen, c.ToString(), _line, _path);
                case ')':
                    _position++;
                    return new AOToken(TokenType.RParen, c.ToString(), _line, _path);
                case '"':
                    _position++;
                    return new AOToken(TokenType.Quotation, c.ToString(), _line, _path);
                case '\'':
                    _position++;
                    return new AOToken(TokenType.OneQuotation, c.ToString(), _line, _path);
                case '{':
                    _position++;
                    return new AOToken(TokenType.LCurl, c.ToString(), _line, _path);
                case '}':
                    _position++;
                    return new AOToken(TokenType.RCurl, c.ToString(), _line, _path);
                case '[':
                    _position++;
                    return new AOToken(TokenType.LCube, c.ToString(), _line, _path);
                case ']':
                    _position++;
                    return new AOToken(TokenType.RCube, c.ToString(), _line, _path);
                case '|':
                    _position++;
                    return new AOToken(TokenType.Pipe, c.ToString(), _line, _path);
                case '\\':
                    _position++;
                    return new AOToken(TokenType.BSlash, c.ToString(), _line, _path);
                case ':':
                    _position++;
                    return new AOToken(TokenType.Colon, c.ToString(), _line, _path);
                case ';':
                    _position++;
                    return new AOToken(TokenType.Semi, c.ToString(), _line, _path);
                case '.':
                    _position++;
                    return new AOToken(TokenType.Dot, c.ToString(), _line, _path);
                case ',':
                    _position++;
                    return new AOToken(TokenType.Comma, c.ToString(), _line, _path);
                case '?':
                    _position++;
                    return new AOToken(TokenType.Question, c.ToString(), _line, _path);
                default:
                    break;
            }
            return new AOToken(TokenType.Identifier, c.ToString(), _line, _path);
        }

        private AOToken GetStringToken()
        {
            int startPos = _position;
            char punctuation = _input[_position];
            string value = "";

            _position++;

            while (_position < _input.Length && _input[_position] != punctuation)
            {
                if (_position != _input.Length - 1)
                {
                    value += _input[_position];
                    _position++;
                }
                else if (_position >= _input.Length && _input[_position] != punctuation)
                {
					return new AOToken(TokenType._ERROR_, "Expected " + punctuation + " but got " + _input[_position], _line, _path);
				}
				else
                {
                    if (_input[_position] == punctuation)
                    {
                        break;
                    }
                    else
                    {
                        return new AOToken(TokenType._ERROR_, "Expected " + punctuation + " but got " + _input[_position], _line, _path);
					}
                }
            }

            _position++;
            value += "\0";

            return new AOToken(TokenType.String, value, _line, _path);
        }

        public AOToken NextToken()
        {
            if (_position >= _input.Length)
            {
                return new AOToken(TokenType._EOS_, "\0", _line, _path);
            }

            char currentChar = _input[_position];

            if (char.IsWhiteSpace(currentChar))
            {
                if (currentChar == '\n')
                {
                    _line++;
                }
                _position++;
                return new AOToken(TokenType.Whitespace, currentChar.ToString(), _line, _path);
            }

            if (char.IsDigit(currentChar) || currentChar == '-' && char.IsDigit(_input[_position + 1]))
            {
                return GetNumberToken();
            }

            if (char.IsPunctuation(currentChar) && (currentChar == '"' || currentChar == '\''))
            {
                return GetStringToken();
            }

            if (char.IsLetter(currentChar) || char.IsSymbol(currentChar) && currentChar == '_')
            {
                return GetIdentifierToken();
            }

            if ((char.IsPunctuation(currentChar) || char.IsSymbol(currentChar)) && currentChar != '_')
            {
                return GetSymbolToken();
            }

            return new AOToken(TokenType._UNK_, $"Unrecognize syntax {currentChar.ToString()}.", _line, _path);
        }
    }
}