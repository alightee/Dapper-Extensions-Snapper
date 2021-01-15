using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Snapper.Helpers.ExtensionMethods
{
	public static class ExpressionExtensionMethods
	{
		public static object ToSqlOperand(this ExpressionType nodeType, bool rightIsNull)
		{
			switch (nodeType)
			{
				case ExpressionType.Add:
					return "+";
				case ExpressionType.And:
					return "&";
				case ExpressionType.AndAlso:
					return "AND";
				case ExpressionType.Divide:
					return "/";
				case ExpressionType.Equal:
					return rightIsNull ? "IS" : "=";
				case ExpressionType.ExclusiveOr:
					return "^";
				case ExpressionType.GreaterThan:
					return ">";
				case ExpressionType.GreaterThanOrEqual:
					return ">=";
				case ExpressionType.LessThan:
					return "<";
				case ExpressionType.LessThanOrEqual:
					return "<=";
				case ExpressionType.Modulo:
					return "%";
				case ExpressionType.Multiply:
					return "*";
				case ExpressionType.Negate:
					return "-";
				case ExpressionType.Not:
					return "NOT";
				case ExpressionType.NotEqual:
					return "<>";
				case ExpressionType.Or:
					return "|";
				case ExpressionType.OrElse:
					return "OR";
				case ExpressionType.Subtract:
					return "-";
			}
			throw new Exception($"Unsupported node type: {nodeType}");
		}

		public static object CompileAndGetValue(this Expression member)
		{
			var objectMember = Expression.Convert(member, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();
			return getter();
		}
	}
}
