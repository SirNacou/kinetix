using Kinetix.Core.Nodes;

namespace Kinetix.Core.Parsing;

internal static class Grammar
{
	private static Parser<string> symbol(string canonical, params string[] aliases) =>
		spaces.Bind(_ => choice(Seq(aliases.Append(canonical).OrderByDescending(x => x.Length).Select(ignoreCase))))
			.SelectMany(_ => spaces, (_, _) => canonical);

	private static Parser<INode> functionCall =>
		Lexer.Identifier.SelectMany(_ => spaces, (name, _) => name)
			.SelectMany(_ => ch('('), (name, _) => name)
			.SelectMany(_ => sepBy(lazyp(() => expression), op(",")), (name, args) => new { name, args })
			.SelectMany(_ => ch(')'), INode (x, _) => new FunctionNode(((VariableNode)x.name).Name, x.args.ToList()));

	private static Parser<INode> factor =>
		choice(
			Lexer.Number,
			Lexer.String,
			attempt(functionCall),
			Lexer.Identifier,
			between(ch('('), ch(')'), lazyp(() => expression)));

	private static Parser<INode> term =>
		chainl1(factor,
			choice(symbol("*"), symbol("/")).Map(o => (Func<INode, INode, INode>)((l, r) => new BinaryNode(l, r, o)))
		);

	private static Parser<INode> math => chainl1(
		term,
		choice(symbol("+"), symbol("-")).Map(o => (Func<INode, INode, INode>)((l, r) => new BinaryNode(l, r, o)))
	);

	private static Parser<INode> comparison => chainl1(
		math,
		choice(
				symbol(">"),
				symbol("<"),
				symbol(">="),
				symbol("<="),
				symbol("==", "="),
				symbol("!=", "<>"))
			.Map(o => (Func<INode, INode, INode>)((l, r) => new BinaryNode(l, r, o)))
	);

	private static Parser<INode> expression =>
		chainl1(
			comparison,
			choice(
					wordOp("AND", "&&"),
					wordOp("OR", "||")
				)
				.Map(o => (Func<INode, INode, INode>)((l, r) => new BinaryNode(l, r, o)))
		);

	private static Parser<string> wordOp(string canonical, params string[] aliases)
	{
		var all = aliases.Append(canonical).OrderByDescending(s => s.Length);
		return spaces.Bind(_ => choice(Seq(all.Select(ignoreCase))))
			.SelectMany(_ => spaces, (_, _) => canonical);
	}

	private static Parser<string> ignoreCase(string s)
	{
		var charParsers = s.Select(c => choice(ch(char.ToUpper(c)), ch(char.ToLower(c))));
		return charParsers.Aggregate(
			result(""),
			(acc, p) => from x in acc from y in p select x + y
		);
	}

	private static Parser<string> op(string s) =>
		spaces.Bind(_ => str(s))
			.SelectMany(_ => spaces, (str, _) => str);

	internal static Fin<INode> Parse(string input)
	{
		var result = parse(expression, input);

		return result.ToEither().Match(
			Right: Fin.Succ,
			Left: Fin.Fail<INode>
		);
	}
}