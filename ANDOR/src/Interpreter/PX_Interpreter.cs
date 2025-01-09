using ANDOR.src.Parser;
using ANDOR.src.Parser.NodeBuilder;
using ANDOR.src.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ANDOR.src.Interpreter
{
	public class PX_Interpreter
	{
		private readonly ScopeManager scopeManager = new ScopeManager();
		private readonly Node root;

		public PX_Interpreter(Node root)
		{
			this.root = root;
		}

		public void Execute()
		{
			ExecuteNode(root);
		}

		private void ExecuteNode(Node node)
		{
			switch (node)
			{
				case OveralNode overalNode:
					ExecuteOveralNode(overalNode);
					break;
				case ClassNode classNode:
					ExecuteClass(classNode);
					break;
				default: break;
			}
		}

		private void ExecuteOveralNode(OveralNode overalNode)
		{
			foreach (var content in overalNode.Content)
			{
				ExecuteNode(content);
			}
		}

		private void ExecuteClass(ClassNode classNode)
		{
			scopeManager.AddLoader(classNode.path, classNode.Line);
			var value = classNode.Body;
			scopeManager.SetVariable(classNode.Name, value);
			scopeManager.EnterScope();
			ExecuteClassBody(classNode.Body);
			scopeManager.ExitScope();
		}

		private void ExecuteClassBody(List<Node> body)
		{
			foreach (var bodyNode in body)
			{
				switch (bodyNode)
				{
					case MethodNode methodNode:
						SaveMethod(methodNode);
						break;

					case VariableDeclarationNode variableNode:
						SaveVariable(variableNode);
						break;

					case ConstructorNode constructorNode:
						ExecuteConstructor(constructorNode);
						break;

					default:
						exception except = new exception($"Statement not recognized {bodyNode.GetType().Name}.", 0, "\0", exception.HandleType.Error, exception.ErrorType.NotImplemented);
						break;
				}
			}
		}

		private void SaveVariable(Node variableNode)
		{
			switch (variableNode)
			{
				case VariableDeclarationNode varNode:
					scopeManager.SetVariable(varNode.Name, ExecuteTerminal(varNode.Value));
					break;
				case VariableDefinitionNode varNode:
					scopeManager.SetVariable(varNode.Name, ExecuteTerminal(varNode.Value));
					break;
				default:
					exception except = new exception($"Assignment type not recognized {variableNode.GetType().Name}.", 0, "\0", exception.HandleType.Error, exception.ErrorType.NotImplemented);
					break;
			}
		}

		private void SaveMethod(MethodNode method)
		{
			scopeManager.SetVariable(method.Name, method.Body);
		}

		private void ExecuteConstructor(ConstructorNode node)
		{
			scopeManager.AddLoader(node.path, node.Line);
			List<Node> body = node.Body;
			scopeManager.SetVariable(node.Name, body);
			scopeManager.EnterScope();

			foreach (var arg in body)
			{
				switch (arg)
				{
					case ParameterBuilder args:
						ExecuteParameter(args.Body);
						break;
					default:
						break;
				}
			}

			ExecuteBody(body);

			scopeManager.ExitScope();
		}

		private void ExecuteParameter(List<Node> Args)
		{
			foreach (Node arg in Args)
			{
				switch (arg)
				{
					case VariableDeclarationNode declare:
						scopeManager.SetVariable(declare.Name, ExecuteTerminal(declare.Value));
						break;
					case VariableDefinitionNode define:
						scopeManager.SetVariable(define.Name, ExecuteTerminal(define.Value));
						break;
					default: break;
				}
			}
		}

		private void ExecuteVariableDefinition(VariableDefinitionNode node)
		{

		}


		private object ExecuteBody(List<Node> node)
		{
			object result = null;
			foreach (var arg in node)
			{
				switch (arg)
				{
					case VariableDefinitionNode define:
						scopeManager.AddLoader(define.path, define.Line);
						scopeManager.SetVariable(define.Name, ExecuteTerminal(define.Value));
						break;
					case VariableDeclarationNode declare:
						scopeManager.AddLoader(declare.path, declare.Line);
						scopeManager.SetVariable(declare.Name, ExecuteTerminal(declare.Value));
						break;
					case PrintNode print:
						ExecutePrint(print);
						break;
					case ReturnNode returns:
						return ExecuteReturn(returns);
					case AccessMethod accessMethods:
						scopeManager.AddLoader(accessMethods.path, accessMethods.Line);
						result = ExecuteCallMethod(accessMethods);
						break;
					default:
						break;
				}
			}

			return result;
		}

		private object ExecuteCallMethod(AccessMethod access)
		{
			string name = access.Name;
			List<string> locator = access.Locator;
			List<Node> parameters = access.Args;
			var methodBody = new List<Node>();

			if (locator.Count == 1)
			{
				// Ambil metode dari kelas saat ini
				methodBody = scopeManager.GetVariable(name) as List<Node>;
				if (methodBody == null)
				{
					exception except = new exception($"The expected object {name} does not exist, possibly undefined.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.RuntimeError);
					return null;
				}

				List<Node> args = new List<Node>();

				// Eksekusi body metode
				foreach (Node node in methodBody)
				{
					switch (node)
					{
						case ParameterBuilder parameter:
							if (parameter.Name == name)
							{
								args = parameter.Body;
							}
							break;
						default: break;
					}
				}

				List<Node> NewParameter = new List<Node>();

				if (access.Args.Count <= args.Count)
				{
					for (int i = 0; i < args.Count; i++)
					{
						var arg = args[i];
						if (i < access.Args.Count)
						{
							if (arg is VariableDeclarationNode declare)
							{
								var newArgValue = access.Args[i];
								switch (newArgValue)
								{
									case VariableDefinitionNode kwargs:
										if (kwargs.Type == declare.Type)
										{
											declare.Value = kwargs.Value;
											NewParameter.Add(declare);
										}
										else
										{
											exception except = new exception($"There is a data type mismatch between {declare.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
										}
										break;
									case StringLiteral kwargs:
										if (kwargs.Type == declare.Type)
										{
											declare.Value = kwargs;
											NewParameter.Add(declare);
										}
										else
										{
											exception except = new exception($"There is a data type mismatch between {declare.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
										}
										break;
									case IntLiteral kwargs:
										if (kwargs.Type == declare.Type)
										{
											declare.Value = kwargs;
											NewParameter.Add(declare);
										}
										else
										{
											exception except = new exception($"There is a data type mismatch between {declare.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
										}
										break;
									case FloatLiteral kwargs:
										if (kwargs.Type == declare.Type)
										{
											declare.Value = kwargs;
											NewParameter.Add(declare);
										}
										else
										{
											exception except = new exception($"There is a data type mismatch between {declare.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
										}
										break;
									case BoolLiteral kwargs:
										if (kwargs.Type == declare.Type)
										{
											declare.Value = kwargs;
											NewParameter.Add(declare);
										}
										else
										{
											exception except = new exception($"There is a data type mismatch between {declare.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
										}
										break;
									case Parser.Nullable kwargs:
										if (kwargs.Type == declare.Type)
										{
											declare.Value = kwargs;
											NewParameter.Add(declare);
										}
										else
										{
											exception except = new exception($"There is a data type mismatch between {declare.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
										}
										break;
									case AccessVariable accessVariable:
										var value = ExecuteCallVariable(accessVariable);
										switch (value)
										{
											case StringLiteral literal:
												if (literal.Type == declare.Type)
												{
													NewParameter.Add((Node)literal);
												}
												else
												{
													exception except = new exception($"There is a data type mismatch between {declare.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
													return null;
												}
												break;
											case Parser.Nullable literal:
												if (literal.Type == declare.Type)
												{
													NewParameter.Add((Node)literal);
												}
												else
												{
													exception except = new exception($"There is a data type mismatch between {declare.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
													return null;
												}
												break;
											case FloatLiteral literal:
												if (literal.Type == declare.Type)
												{
													NewParameter.Add((Node)literal);
												}
												else
												{
													exception except = new exception($"There is a data type mismatch between {declare.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
													return null;
												}
												break;
											case IntLiteral literal:
												if (literal.Type == declare.Type)
												{
													NewParameter.Add((Node)literal);
												}
												else
												{
													exception except = new exception($"There is a data type mismatch between {declare.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
													return null;
												}
												break;
											case BoolLiteral literal:
												if (literal.Type == declare.Type)
												{
													NewParameter.Add((Node)literal);
												}
												else
												{
													exception except = new exception($"There is a data type mismatch between {declare.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
													return null;
												}
												break;
											default:
												exception exceptDefault = new exception($"Unsupported value type for {declare.Type}", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
										}
										break;
									default:
										exception exceptUnhandled = new exception($"Arguments is not implemented {newArgValue.GetType().Name}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
										return null;
								}
							}
								else if (arg is VariableDefinitionNode define)
								{
									var newDefArgs = access.Args[i];
									switch (newDefArgs)
									{
										case VariableDefinitionNode kwargs:
											if (kwargs.Type == define.Type)
											{
												define.Value = kwargs.Value;
												NewParameter.Add(define);
											}
											else
											{
												exception except = new exception($"There is a data type mismatch between {define.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
											}
											break;
										case StringLiteral kwargs:
											if (kwargs.Type == define.Type)
											{
												define.Value = kwargs;
												NewParameter.Add(define);
											}
											else
											{
												exception except = new exception($"There is a data type mismatch between {define.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
											}
											break;
										case IntLiteral kwargs:
											if (kwargs.Type == define.Type)
											{
												define.Value = kwargs;
												NewParameter.Add(define);
											}
											else
											{
												exception except = new exception($"There is a data type mismatch between {define.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
											}
											break;
										case FloatLiteral kwargs:
											if (kwargs.Type == define.Type)
											{
												define.Value = kwargs;
												NewParameter.Add(define);
											}
											else
											{
												exception except = new exception($"There is a data type mismatch between {define.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
											}
											break;
										case BoolLiteral kwargs:
											if (kwargs.Type == define.Type)
											{
												define.Value = kwargs;
												NewParameter.Add(define);
											}
											else
											{
												exception except = new exception($"There is a data type mismatch between {define.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
											}
											break;
										case Parser.Nullable kwargs:
											if (kwargs.Type == define.Type)
											{
												define.Value = kwargs;
												NewParameter.Add(define);
											}
											else
											{
												exception except = new exception($"There is a data type mismatch between {define.Type} and {kwargs.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
												return null;
											}
											break;
										case AccessVariable accessVariable:
											var value = ExecuteCallVariable(accessVariable);
											switch (value)
											{
												case StringLiteral literal:
													if (literal.Type == define.Type)
													{
														NewParameter.Add((Node)literal);
													}
													else
													{
														exception except = new exception($"There is a data type mismatch between {define.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
														return null;
													}
													break;
												case Parser.Nullable literal:
													if (literal.Type == define.Type)
													{
														NewParameter.Add((Node)literal);
													}
													else
													{
														exception except = new exception($"There is a data type mismatch between {define.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
														return null;
													}
													break;
												case FloatLiteral literal:
													if (literal.Type == define.Type)
													{
														NewParameter.Add((Node)literal);
													}
													else
													{
														exception except = new exception($"There is a data type mismatch between {define.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
														return null;
													}
													break;
												case IntLiteral literal:
													if (literal.Type == define.Type)
													{
														NewParameter.Add((Node)literal);
													}
													else
													{
														exception except = new exception($"There is a data type mismatch between {define.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
														return null;
													}
													break;
												case BoolLiteral literal:
													if (literal.Type == define.Type)
													{
														NewParameter.Add((Node)literal);
													}
													else
													{
														exception except = new exception($"There is a data type mismatch between {define.Type} and {literal.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
														return null;
													}
													break;
												default:
													exception exceptDefault = new exception($"Unsupported value type for {define.Type}", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
													return null;
											}
											break;
										default:
											exception exceptUnhandled = new exception($"Arguments is not implemented {newDefArgs.GetType().Name}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.TypeError);
											return null;
									}
								}
						}
						else
						{
							if (arg is VariableDeclarationNode declare)
							{
								exception except = new exception($"The {declare.Name} argument requires a value of type {declare.Type}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.ValueError);
								return null;
							}
							else if (arg is VariableDefinitionNode define)
							{
								NewParameter.Add(define);
							}
						}
					}
				}
				else
				{
					exception except = new exception($"Too many arguments passed, Expected argument length {args.Count} but got {parameters.Count}.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.RuntimeError);
					return null;
				}

				parameters = NewParameter;
			}
			else
			{
				exception except = new exception("Locator refers to an external class, handling not implemented.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.NotImplemented);
				return null;
			}

			List<Node> body = new List<Node>();

			scopeManager.SetVariable(name, body);
			scopeManager.EnterScope();

			ExecuteParameter(parameters);

			foreach (var child in methodBody)
			{
				switch (child)
				{
					case not ParameterBuilder:
						body.Add(child);
						break;
					default: break;
				}
			}

			object result = ExecuteBody(body);
			scopeManager.ExitScope();

			return result;
		}


		private object ExecuteCallVariable(AccessVariable access)
		{
			string name = access.Name;
			List<string> locator = access.Locator;

			if (locator.Count == 1)
			{
				// Ambil metode dari kelas saat ini
				Node variable = scopeManager.GetVariable(name) as Node;
				if (variable == null)
				{
					exception except = new exception($"Unrecognize {name} in current.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.ValueError);
					return null;
				}
				else
				{
					return ExecuteTerminal(variable);
				}
			}
			else
			{
				exception except = new exception("Locator refers to an interal class, handling not implemented.", access.Line, access.path, exception.HandleType.Error, exception.ErrorType.NotImplemented);
				return null;
			}
		}

		private object ExecuteReturn(ReturnNode returnNode)
		{
			var result = new List<Node>();
			scopeManager.AddLoader(returnNode.path, returnNode.Line);
			foreach (var arg in returnNode.Args)
			{
				result.Add(ExecuteTerminal(arg));
			}

			returnNode.Args = result;

			return returnNode;
		}

		private void ExecutePrint(PrintNode print)
		{
			scopeManager.AddLoader(print.path, print.Line);
			foreach (Node arg in print.Args)
			{
				Console.Write(GetValue(ExecuteTerminal(arg)).ToString());
			}

			Console.WriteLine("");
		}

		private object GetValue(object term)
		{
			switch (term)
			{
				case StringLiteral literal:
					return literal.Value;
				case FloatLiteral literal:
					return literal.Value;
				case IntLiteral literal:
					return literal.Value;
				case BoolLiteral literal:
					return literal.Value;
				case Parser.Nullable literal:
					return literal.Value;
				case AccessMethod literal:
					return ExecuteCallMethod(literal);
				case AccessVariable literal:
					return ExecuteCallVariable(literal);
				case ReturnNode returns:

					if (returns.Args.Count <= 1)
					{
						return GetValue(returns.Args[0]);
					}

					string result = "[";

					if ("string" == GetValueType(returns.Args[0]))
					{
						result += $"'{GetValue(returns.Args[0])}'";
					}
					else
					{
						result += $"{GetValue(returns.Args[0])}";
					}


					for (int i = 1; i < returns.Args.Count; i++)
					{
						if ("string" == GetValueType(returns.Args[i]))
						{
							result += $", '{GetValue(returns.Args[i])}'";
						}
						else
						{
							result += $", {GetValue(returns.Args[i])}";
						}
					}

					return result + "]";
				default: return term;
			}
		}

		private string GetValueType(object term)
		{
			switch (term)
			{
				case StringLiteral literal:
					return literal.Type;
				case FloatLiteral literal:
					return literal.Type;
				case IntLiteral literal:
					return literal.Type;
				case BoolLiteral literal:
					return literal.Type;
				case Parser.Nullable literal:
					return literal.Type;
				case ReturnNode returns:
					return "List";
				case List<Task> terms:
					return "List";
				default: return term.GetType().Name;
			}
		}

		private Node ExecuteTerminal(Node terminal)
		{
			switch (terminal)
			{
				case StringLiteral strings:
					return (Node)strings;
				case IntLiteral integer:
					return (Node)integer;
				case FloatLiteral floats:
					return (Node)floats;
				case BoolLiteral bools:
					return (Node)bools;
				case Parser.Nullable nulls:
					return (Node)nulls;
				case AccessMethod accessMethod:
					return (Node)ExecuteCallMethod(accessMethod);
				case AccessVariable accessVariable:
					return (Node)ExecuteCallVariable(accessVariable);
				case ReturnNode returns:
					if (returns.Args.Count == 1)
					{
						return (Node)returns.Args[0];
					}
					return returns;
				default:
					return terminal;
			}
		}

		private void loops()
		{
			while (true){}
		}
	}
}