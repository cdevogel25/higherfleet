using Godot;
using System;
using System.Collections.Generic;

public partial class ShipGraph
{
    // ok here's what needs to happen: a graph of all ship nodes and their connections needs to be maintained
    // each node is a BuilderObject_Placeable
    // when two nodes are connected, they need to be added to each other's neighbor lists
    // when two nodes are disconnected, they need to be removed from each other's neighbor lists and if a node is
    // removed from the ship it should be removed from ShipGraph entirely
    // at the same time, each connected component of the graph needs to be evaluated for connectivity to a root node
    public List<ShipGraphNode> Nodes = new List<ShipGraphNode>();
}