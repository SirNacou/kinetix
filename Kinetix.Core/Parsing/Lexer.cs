using Kinetix.Core.Nodes;

namespace Kinetix.Core.Parsing;

internal static class Lexer
{
	internal static Parser<INode> Number =>
		from d in many1(digit)
		from dot in optional(ch('.'))
		from decimals in optional(many1(digit))
		let n = d.Append(dot.Map(Seq).IfNone(SeqEmpty.Default))
			.Append(decimals.IfNone(SeqEmpty.Default))
		select (INode)new NumberNode(decimal.Parse(string.Concat(n)));

	internal static Parser<INode> Identifier =>
		from x in letter
		from xs in many(alphaNum)
		select (INode)new VariableNode(x.Cons(xs).ToFullString(""));

	internal static Parser<INode> String =>
		ch('\'')
			.Bind(_ => many(satisfy(c => c != '\'')))
			.SelectMany(_ => ch('\''), INode (content, _) => new StringNode(content.ToFullString("")));
}