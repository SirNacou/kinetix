using System.Linq.Expressions;
using Kinetix.Core.Nodes;

namespace Kinetix.Core.Compiler;

internal class ExpressionCompiler
{
	internal Fin<Func<TContext, TResult>> Compile<TContext, TResult>(INode root)
	{
		return Try.lift(() =>
		{
			var param = Expression.Parameter(typeof(TContext), "ctx");

			var body = Visit(root, param);

			if (body.Type != typeof(TResult))
			{
				body = Expression.Convert(body, typeof(TResult));
			}

			var compiledFunc = Expression.Lambda<Func<TContext, TResult>>(body, param).Compile();

			return compiledFunc;
		}).Run();
	}

	private Expression Visit(INode node, ParameterExpression param)
	{
		switch (node)
		{
			case NumberNode n:
				return Expression.Constant(n.Value, typeof(decimal));
			case StringNode s:
				return Expression.Constant(s.Value, typeof(string));
			case VariableNode v:
				var prop = Expression.Property(param, v.Name);

				if (prop.Type == typeof(int) || prop.Type == typeof(double))
				{
					return Expression.Convert(prop, typeof(decimal));
				}

				return prop;
			case BinaryNode b:
				var left = Visit(b.Left, param);
				var right = Visit(b.Right, param);

				if (left.Type != right.Type)
				{
					if (left.Type != typeof(decimal)) left = Expression.Convert(left, typeof(decimal));
					if (right.Type != typeof(decimal)) right = Expression.Convert(right, typeof(decimal));
				}

				return b.Op switch
				{
					"+" => Expression.Add(left, right),
					"-" => Expression.Subtract(left, right),
					"*" => Expression.Multiply(left, right),
					"/" => Expression.Divide(left, right),
					">" => Expression.GreaterThan(left, right),
					"<" => Expression.LessThan(left, right),
					">=" => Expression.GreaterThanOrEqual(left, right),
					"<=" => Expression.LessThanOrEqual(left, right),
					"==" => Expression.Equal(left, right),
					"!=" => Expression.NotEqual(left, right),
					"AND" => Expression.And(left, right),
					"OR" => Expression.Or(left, right),
					_ => throw new NotSupportedException($"Op '{b.Op}' not supported")
				};
			case FunctionNode f:
				var args = f.Arguments.Select(a => Visit(a, param)).ToArray();

				return f.Name.ToUpper() switch
				{
					"MIN" => Expression.Call(
						typeof(Math).GetMethod(nameof(Math.Min), [typeof(decimal), typeof(decimal)])!,
						args[0], args[1]),
					"MAX" => Expression.Call(
						typeof(Math).GetMethod(nameof(Math.Max), [typeof(decimal), typeof(decimal)])!,
						args[0], args[1]),
					"ABS" => Expression.Call(
						typeof(Math).GetMethod(nameof(Math.Abs), [typeof(decimal), typeof(decimal)])!,
						args[0], args[1]),
					"IF" => Expression.Condition(args[0], args[1], args[2]),
					_ => throw new NotSupportedException($"Function '{f.Name}' not supported")
				};
			default:
				throw new NotSupportedException($"Node '{node.GetType().Name}' not supported");
		}
	}
}