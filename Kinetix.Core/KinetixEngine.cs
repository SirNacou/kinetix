using Kinetix.Core.Compiler;

namespace Kinetix.Core;

public class KinetixEngine
{
	// Generic Execute method
	public Fin<TResult> Execute<TContext, TResult>(string formula, TContext context)
	{
		// 1. Parse
		var astResult = Parsing.Grammar.Parse(formula);

		return astResult.Bind(node =>
			{
				// 2. Compile
				var compiler = new ExpressionCompiler();
				var compileResult = compiler.Compile<TContext, TResult>(node);

				return compileResult.Map(func => func(context));
			}
		);
	}
}