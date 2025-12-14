namespace Kinetix.Core.Parsing;

internal static class ParserExtensions
{
	// Matches a string case-insensitively
	public static Parser<string> IgnoreCase(string s) =>
		s.Select(c => choice(ch(char.ToUpper(c)), ch(char.ToLower(c))))
			.Aggregate(result(""), (acc, p) => from x in acc from y in p select x + y);

	// Matches a symbol surrounded by spaces
	public static Parser<string> Op(string s) =>
		spaces.Bind(_ => str(s)).SelectMany(_ => spaces, (x, _) => x);

	// Matches a specific keyword or symbol, sorted by length
	public static Parser<string> Symbol(string canonical, params string[] aliases)
	{
		var all = aliases.Append(canonical).OrderByDescending(s => s.Length);
		return spaces.Bind(_ => choice(Seq(all.Select(str))))
			.SelectMany(_ => spaces, (_, _) => canonical);
	}
}