namespace Kinetix.Core.Nodes;

internal sealed record FunctionNode(string Name, List<INode> Arguments) : INode
{
	public override string ToString() => $"{Name}({string.Join(", ", Arguments.Select(a => a.ToString()))})";
}