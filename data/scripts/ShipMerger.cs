using Godot;
using System;
using System.Collections.Generic;

public static class ShipMerger
{
    // merges tileA and tileB into a single ShipTile?
    public static void MergeTiles (ShipTile tileA, ShipTile tileB)
    {
        Node parent = tileA.GetParent();
        if (parent == null)
            parent = tileB.GetParent();

        var merged = new ShipTile();
        merged.Name = "MergedShipTile";
        Rect2 aabbA = tileA.GetApproximateAABB();
        Rect2 aabbB = tileB.GetApproximateAABB();
        Rect2 combined = aabbA.Merge(aabbB);
        Vector2 center = combined.Position + combined.Size / 2.0f;
        merged.GlobalPosition = center;
        // merged.Mass = tileA.Mass + tileB.Mass;
        // merged.GlobalPosition = ComputeCenter(tileA, tileB);

        CopyCollisionShapes(tileA, merged);
        CopyCollisionShapes(tileB, merged);

        parent.AddChild(merged);
        merged.Owner = parent.GetOwner();

        TransferChildrenToBody(tileA, merged);
        TransferChildrenToBody(tileB, merged);

        tileA.QueueFree();
        tileB.QueueFree();
    }

    private static void CopyCollisionShapes(ShipTile from, ShipTile to)
    {
        foreach (Node child in from.GetChildren())
        {
            if (child is CollisionShape2D cs)
            {
                var newShape = cs.Shape.Duplicate() as Shape2D;
                var newCs = new CollisionShape2D();
                newCs.Shape = newShape;

                var globalXform = cs.GlobalTransform;
                to.AddChild(newCs);
                newCs.Owner = to.GetOwner();
                newCs.GlobalTransform = globalXform;
            }
        }
    }

    private static Vector2 ComputeCenter(ShipTile a, ShipTile b)
    {
        // center of mass will be center of tile, so no need to include masses (for now)
        return (a.GlobalPosition + b.GlobalPosition) / 2.0f;
    }

    private static void TransferChildrenToBody(Node from, Node toBody)
    {
        var move = new List<Node>();
        foreach (Node child in from.GetChildren())
        {
            if (child != toBody)
                move.Add(child);
        }

        foreach (var c in move)
        {
            c.GetParent().RemoveChild(c);

            if (c is Node2D n2d)
            {
                var globalXform = n2d.GlobalTransform;
                toBody.AddChild(n2d);
                n2d.Owner = toBody.GetOwner();
                n2d.GlobalTransform = globalXform;
            }
            else
            {
                toBody.AddChild(c);
                c.Owner = toBody.GetOwner();
            }
        }
    }
}