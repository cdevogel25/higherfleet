using System.Collections.Generic;
using Godot;

public class ShipGraphNode
{
    public bool IsRoot = false;
    public BuilderObject_Placeable PlaceableObject;
    // maybe this should be a dictionary?
    public HashSet<ShipGraphNode> Neighbors = new HashSet<ShipGraphNode>();
    // public Dictionary<int, ShipGraphNode> Neighbors = new Dictionary<int, ShipGraphNode>();

    public ShipGraphNode(BuilderObject_Placeable placeableObject, bool isRoot = false)
    {
        PlaceableObject = placeableObject;
        IsRoot = isRoot;
    }

    public void AddNeighbor(ShipGraphNode neighbor)
    {
        Neighbors.Add(neighbor);
    }

    public void RemoveNeighbor(ShipGraphNode neighbor)
    {
        Neighbors.Remove(neighbor);
    }

    public void OnRemove()
    {
        foreach (var neighbor in Neighbors)
        {
            neighbor.RemoveNeighbor(this);
        }
        Neighbors.Clear();
    }

    public int GetNeighborDirection(ShipGraphNode neighbor)
    {
        Vector2 direction = neighbor.PlaceableObject.Position - PlaceableObject.Position;
        // this is not right
        return (int)direction.Angle();
        // return Mathf.RoundToInt(direction.Angle());
    }
}