namespace Kinetix.Core.Nodes;

internal sealed record VariableNode(string Name) : INode
{
	public override string ToString() => $"Var({Name})";
}