namespace Kinetix.Core.Nodes;

internal sealed record BinaryNode(INode Left, INode Right, string Op) : INode
{
	public override string ToString() => $"({Left} {Op} {Right})";
}