using System.Collections.Generic;
using Godot;

// when a placeable object is added to the ship structure, 1) check for neighbors and 2) create graph node connections
public class ShipGraphNode
{
    public BuilderObject_Placeable PlaceableObject;
    public Dictionary<string, ShipGraphNode> Neighbors = new Dictionary<string, ShipGraphNode>();

    public ShipGraphNode(BuilderObject_Placeable obj)
    {
        PlaceableObject = obj;
    }

    public void AddNeighbor(string direction, ShipGraphNode neighbor)
    {
        if (!Neighbors.ContainsKey(direction))
        {
            Neighbors[direction] = neighbor;
        }
    }
}