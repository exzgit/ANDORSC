using ANDOR.src.Interpreter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ANDOR.src.Sys
{
	//public class ThisException : Exception
	//{
	//	public string Path { get; }
	//	public int Line { get; }

	//	// Konstruktor yang menerima pesan, path, dan line
	//	public ThisException(string message, string path, int line)
	//		: base(message)
	//	{
            
 //           //Console.WriteLine("Press the CTRL + C keys to exit the program...");
	//		Path = path;
	//		Line = line;
 //           try
 //           {
	//			Console.WriteLine(message);
	//		}catch (Exception e)
 //           {
	//			Console.WriteLine(message);
	//		}
	//	}
	//}

	public class exception
    {
        public enum HandleType
        {
            Warning,
            Error,
            Message,
            Success,
            Default,
        }

        public enum ErrorType
        {
            SystemError,
            SyntaxError,
            ValueError,
            StatementError,
            RuntimeError,
            TypeError,
            LiteralError,
            ReaderError,
            ZeroDivisionError,
            OperandError,
            DefaultError,
            NotImplemented,
            ProgramCrash,
		}

        public HandleType _handleType = HandleType.Default;
        public ErrorType _errorType = ErrorType.DefaultError;

        private string MESSAGE = "";
        private int ERRLINE = 0;
        private string ERRPATH = "";

		public exception(string msg, int line, string path, HandleType handleType, ErrorType errorType)
        {
            MESSAGE = msg;
			ERRLINE = line;
			ERRPATH = path;
            _handleType = handleType;
            _errorType = errorType;

            if (_handleType == HandleType.Warning)
            {
                Warning();
            }
            else if (_handleType == HandleType.Message)
            {
                Message();
            }
            else if (_handleType == HandleType.Default)
            {
                Default();
            }
            else if (_handleType == HandleType.Success)
            {
                Success();
            }
            else if (_handleType == HandleType.Error)
            {
                if (_errorType == ErrorType.SyntaxError)
                {
                    SyntaxError();
                }
                else if (_errorType == ErrorType.TypeError)
                {
                    TypeError();
                }
                else if (_errorType == ErrorType.RuntimeError)
                {
                    RuntimeError();
                }
                else if (_errorType == ErrorType.OperandError)
                {
                    OperandError();
                }
                else if (_errorType == ErrorType.LiteralError)
                {
                    LiteralError();
                }
                else if (_errorType == ErrorType.ReaderError)
                {
                    ReaderError();
                }
                else if (_errorType == ErrorType.SystemError)
                {
                    SystemError();
                }
                else if (_errorType == ErrorType.ValueError)
                {
                    ValueError();
                }
                else if (_errorType == ErrorType.ZeroDivisionError)
                {
                    ZeroDivisionError();
                }
				else if (_errorType == ErrorType.NotImplemented)
				{
					NotImplemented();
				}
				else if (_errorType == ErrorType.ProgramCrash)
				{
					ProgramCrash();
				}
				else
                {
                    DefaultError();
                }
            }
        }

		private void Log(string messageType, string message, ConsoleColor color = ConsoleColor.White)
        {
            List<string> loader = new List<string>();
            loader = ScopeManager.GetDebugInfo();

			string info = "Backtracking Loader:\n";
            foreach (string arg in loader.ToArray().Reverse())
            {
                info += arg + "\n";
            }

            Console.ResetColor();

            info += $"\n{ERRPATH}, line {ERRLINE}\n {messageType} -> {message}";
            //throw new ThisException(info, _path, _line);
            Console.WriteLine(info);
            return;
		}

        public void Success()
        {
            Log("Success", MESSAGE, ConsoleColor.Green);
        }

        public void Warning()
        {
            Log("Warning", MESSAGE, ConsoleColor.Yellow);
        }

        public void Default()
        {
            Log("Default", MESSAGE, ConsoleColor.White);
        }

		public void ProgramCrash()
		{
			Log("Program Crash", MESSAGE, ConsoleColor.White);
		}

		public void Message()
        {
            Log("Message", MESSAGE, ConsoleColor.White);
        }

        public void DefaultError()
        {
            Log("Default Error", MESSAGE, ConsoleColor.Red);
        }

		public void NotImplemented()
		{
			Log("Default Error", MESSAGE, ConsoleColor.Red);
		}
		public void SyntaxError()
        {
            Log("Not Implemented Error", MESSAGE, ConsoleColor.Red);
        }

        public void ValueError()
        {
            Log("Value Error", MESSAGE, ConsoleColor.Red);
        }

        public void StatementError()
        {
            Log("Statement Error", MESSAGE, ConsoleColor.Red);
        }

        public void RuntimeError()
        {
            Log("Runtime Error", MESSAGE, ConsoleColor.Red);
        }

        public void ZeroDivisionError()
        {
            Log("Zero Division Error", MESSAGE, ConsoleColor.Red);
        }

        public void TypeError()
        {
            Log("Type Error", MESSAGE, ConsoleColor.Red);
        }

        public void ReaderError()
        {
            Log("Reader Error", MESSAGE, ConsoleColor.Red);
        }

        public void SystemError()
        {
            Log("System Error", MESSAGE, ConsoleColor.Red);
        }

        public void LiteralError()
        {
            Log("Literal Error", MESSAGE, ConsoleColor.Red);
        }

        public void OperandError()
        {
            Log("Operand Error", MESSAGE, ConsoleColor.Red);
        }
    }
}
