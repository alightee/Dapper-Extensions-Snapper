using Dapper.Extensions.Snapper.Helpers.ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Dapper.Extensions.Snapper.Helpers.SqlCompiler
{
	public static class WhereClauseCompiler
	{

		public const string ParameterName = "__P";
		public const int ParameterLimit = 2000;
		public const Language DefaultLanguage = Language.TSQL;

		public static (string Sql, IDictionary<string, object> Parameters) ToSql<T>(Expression<Func<T, bool>> expression, string parameterName = null, int? parameterLimit = null, Language? language = null)
		{
			var paramName = string.IsNullOrWhiteSpace(parameterName) ? ParameterName : parameterName;
			var paramLimit = parameterLimit ?? ParameterLimit;
			Language lang = language ?? DefaultLanguage;

			return new WhereClauseCompilerContext<T>(paramName, paramLimit, lang).BuildWhereClause(expression);
		}

		private class WhereClauseCompilerContext<T>
		{
			private Dictionary<string, object> QueryParameters { get; }
			private string ParameterName { get; }
			private int ParameterLimit { get; }
			private int CurrentParameterIndex { get; set; } = 0;
			private Language SelectedLanguage { get; set; }

			public WhereClauseCompilerContext(string parameterName, int parameterLimit, Language language)
			{
				QueryParameters = new Dictionary<string, object>();
				ParameterName = parameterName;
				ParameterLimit = parameterLimit;
				SelectedLanguage = language;
			}

			public (string Sql, IDictionary<string, object> Parameters) BuildWhereClause(Expression<Func<T, bool>> expression)
			{
				var query = Recurse(expression.Body, true);
				return (query, QueryParameters);
			}

			private string Recurse(Expression expression, bool isUnary = false, bool expectTypePropertySelector = false, ExpressionType? parentExpressionNodeType = null)
			{
				if (expression is UnaryExpression)
				{
					var unary = (UnaryExpression)expression;
					if (UnaryExpressionShouldBeCompiled(unary))
					{
						return CreateParameter(unary.CompileAndGetValue());
						
					}
					var right = Recurse(unary.Operand, true);
					return "(" + unary.NodeType.ToSqlOperand(right == "NULL") + " " + right + ")";
				}

				if (expression is BinaryExpression)
				{
					var body = (BinaryExpression)expression;

					// determine if we should add paranthesis or not
					bool paranthesis = true;
					if (parentExpressionNodeType.HasValue)
					{
						if (parentExpressionNodeType == ExpressionType.AndAlso)
						{
							if (body.NodeType == ExpressionType.AndAlso)
								paranthesis = false;
						}

						if (parentExpressionNodeType == ExpressionType.OrElse)
						{
							if (body.NodeType == ExpressionType.AndAlso)
								paranthesis = false;
							if (body.NodeType == ExpressionType.OrElse)
								paranthesis = false;
						}
					}

					var right = Recurse(body.Right, parentExpressionNodeType: body.NodeType);
					var left = Recurse(body.Left, parentExpressionNodeType: body.NodeType);

					// switch so that null value is allways to the right
					if (left == "NULL" && right != "NULL")
					{
						left = right;
						right = "NULL";
					}

					return string.Format("{0}{1} {2} {3}{4}",
						paranthesis ? "(" : string.Empty,
						left,
						body.NodeType.ToSqlOperand(right == "NULL"),
						right,
						paranthesis ? ")" : string.Empty);
				}

				if (expression is ConstantExpression)
				{
					var constant = (ConstantExpression)expression;
					return CreateParameter(constant.Value);
				}

				if (expression is MemberExpression)
				{
					var member = (MemberExpression)expression;

					if (member.Member is PropertyInfo)
					{
						if (IsPropertyOfModel(member))
						{
							var columnSql = string.Format("{0}{1}{2}",
									SelectedLanguage == Language.TSQL ? "[" : "`",
									((PropertyInfo)member.Member).Name,
									SelectedLanguage == Language.TSQL ? "]" : "`"
								);

							if (isUnary && member.Type == typeof(bool))
							{
								return "(" + columnSql + " = 1)";
							}
							return columnSql;
						}
						else
						{
							if (expectTypePropertySelector)
								throw new NotSupportedException($"Not supported. The selected property wasn't from the {typeof(T).Name} type.");

							//property of a different type means that we expect the expression to compile to a constant value
							return CreateParameter(member.CompileAndGetValue());
						}
					}

					if (member.Member is FieldInfo)
					{
						return CreateParameter(member.CompileAndGetValue());
					}

					throw new NotSupportedException($"Expression does not refer to a property or field: {expression}");
				}

				if (expression is MethodCallExpression)
				{
					var methodCall = (MethodCallExpression)expression;
					// LIKE queries:
					if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
					{
						return "(" + Recurse(methodCall.Object) + " LIKE CONCAT('%'," + Recurse(methodCall.Arguments[0]) + ",'%'))";
					}
					if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
					{
						return "(" + Recurse(methodCall.Object) + " LIKE CONCAT(" + Recurse(methodCall.Arguments[0]) + ",'%'))";
					}
					if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
					{
						return "(" + Recurse(methodCall.Object) + " LIKE CONCAT('%'," + Recurse(methodCall.Arguments[0]) + "))";
					}
					// IN queries:
					if (methodCall.Method.Name == "Contains")
					{
						Expression collection;
						Expression property;
						if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
						{
							collection = methodCall.Arguments[0];
							property = methodCall.Arguments[1];
						}
						else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
						{
							collection = methodCall.Object;
							property = methodCall.Arguments[0];
						}
						else
						{
							throw new NotSupportedException("Unsupported method call: " + methodCall.Method.Name);
						}

						var values = collection.CompileAndGetValue();
						if (!(values is IEnumerable))
						{
							throw new NotSupportedException("Unsupported method call: " + methodCall.Method.Name);
						}

						var listParameters = new List<string>();
						foreach (var e in (IEnumerable)values)
						{
							listParameters.Add(CreateParameter(e));
						}

						if (!listParameters.Any())
						{
							return "(1=0)"; // Value is inside empty list? = No
						}

						return "(" + Recurse(property, expectTypePropertySelector: true) + " IN (" + string.Join(", ", listParameters) + "))";
					}
					throw new NotSupportedException("Unsupported method call: " + methodCall.Method.Name);
				}

				throw new NotSupportedException("Unsupported expression: " + expression.GetType().Name);
			}

			private string CreateParameter(object value)
			{
				if (value == null)
					return "NULL";

				//add 1 because indexes start at 0
				if (CurrentParameterIndex + 1 > ParameterLimit)
					throw new IndexOutOfRangeException($"Exceeded {ParameterLimit} parameters");

				if (QueryParameters.TryAdd($"{ParameterName}{CurrentParameterIndex}", value))
				{
					CurrentParameterIndex++;
				}
				else
				{
					// this exception should never happen - we have tests so it's okay
					throw new Exception("Couldn't create dynamic parameter.");
				}

				return $"@{ParameterName}{CurrentParameterIndex - 1}";
			}

			private bool UnaryExpressionShouldBeCompiled(UnaryExpression unaryExpression)
			{
				var shouldBeCompiled = false;

				switch (unaryExpression.NodeType)
				{
					case ExpressionType.ArrayLength:
					case ExpressionType.Convert:
					case ExpressionType.ConvertChecked:
					case ExpressionType.Negate:
					case ExpressionType.NegateChecked:
					case ExpressionType.UnaryPlus:
					case ExpressionType.Not:
						shouldBeCompiled = true;
						break;
				}

				//check operand is constant / variable
				if (shouldBeCompiled)
				{
					var operand = unaryExpression.Operand;

					//example: convert is called on a constant expression (which is also unary) so there's a unary expression inside a unary expression
					if (operand is UnaryExpression)
						shouldBeCompiled = UnaryExpressionShouldBeCompiled((UnaryExpression)operand);
					else
					{
						if (!(operand is ConstantExpression || operand is MemberExpression))
						{
							shouldBeCompiled = false;
						}
						else if (operand is MemberExpression)
						{
							shouldBeCompiled = !IsPropertyOfModel((MemberExpression)operand);
						}
					}

				}

				return shouldBeCompiled;
			}

			/// <summary>
			/// Returns true if the member expression is a property slector from the model
			/// <para>Throws <see cref="NotSupportedException"/> if the property is marked with an "ignore" attribute.</para>
			/// </summary>
			/// <param name="expression"></param>
			/// <returns></returns>
			private bool IsPropertyOfModel(MemberExpression expression)
			{
				if (expression.Member is PropertyInfo && expression.Expression is ParameterExpression)
				{
					var property = (PropertyInfo)expression.Member;
					CheckModelProperty(property);
					return true;
				}
				return false;
			}

			/// <summary>
			/// Check dapper "Write(false)" attribute
			/// </summary>
			/// <param name="modelProperty"></param>
			private void CheckModelProperty(PropertyInfo modelProperty)
			{
				var propertyAttributes = modelProperty.GetCustomAttributes(true);
				foreach (var attr in propertyAttributes)
				{
					var attrType = attr.GetType();
					if (attrType.FullName != "Dapper.Contrib.Extensions.WriteAttribute")
						continue;

					var attrProperty = attrType.GetProperty("Write");
					if (attrProperty != null)
					{
						var attrPropertyValue = attrProperty.GetValue(attr);
						if (attrPropertyValue.GetType() == typeof(bool) && !(bool)attrPropertyValue)
						{
							throw new NotSupportedException("Must select a property that is mapped to one of the table columns.");
						}
					}
				}
			}
		}

		public enum Language
		{
			TSQL,
			MYSQL
		}
	}
}