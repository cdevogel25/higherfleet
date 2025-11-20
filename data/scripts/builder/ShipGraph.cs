using System.Collections.Generic;
using Godot;

public class ShipGraph
{
    public HashSet<ShipGraphNode> Nodes = new HashSet<ShipGraphNode>();
    public ShipGraphNode RootNode = null;

    public ShipGraph(ShipGraphNode rootNode)
    {
        RootNode = rootNode;
        Nodes.Add(rootNode);
    }

    public void AddNode(ShipGraphNode node)
    {
        Nodes.Add(node);
    }

    public void RemoveNode(ShipGraphNode node)
    {
        node.OnRemove();
        Nodes.Remove(node);
    }
}