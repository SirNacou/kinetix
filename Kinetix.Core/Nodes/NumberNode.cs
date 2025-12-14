namespace Kinetix.Core.Nodes;

internal sealed record NumberNode(decimal Value) : INode
{
	public override string ToString() => $"Num({Value})";
}