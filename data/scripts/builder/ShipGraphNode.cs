using Godot;
using System;
using System.Collections.Generic;

public class ShipGraphNode
{
    // this is the placeable object this node represents
    public BuilderObject_Placeable PlaceableObject;
    public bool IsRoot = false;
    public bool IsConnectedToRoot = false;
    // Dictionary of neighbor nodes and the direction from which they are connected.
    // at most can contain 4 neighbors (N, S, E, W)
    public Dictionary<string, ShipGraphNode> Neighbors = new Dictionary<string, ShipGraphNode>();
}