namespace Kinetix.Core.Nodes;

internal sealed record StringNode(string Value) : INode
{
	public override string ToString() => $"Str({Value})";
}