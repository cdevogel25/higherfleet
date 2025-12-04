using System;
using System.Collections.Generic;
using System.Linq;
using Builder.Components.External;
using Godot;

public partial class Component_Structural_S : Component_Structural
{
    protected override void _CollectExternalSnapPoints()
    {
        foreach (var child in GetChildren().OfType<SnapPoint_External>())
        {
            switch (child.Name)
            {
                case "SnapPoint_External_North" : ExternalSnapPoints.North = child; break;
                case "SnapPoint_External_South" : ExternalSnapPoints.South = child; break;
                case "SnapPoint_External_East" : ExternalSnapPoints.East = child; break;
                case "SnapPoint_External_West" : ExternalSnapPoints.West = child; break;
            }
        }
    }

    protected override bool _TrySnap_Structural()
    {
        List<Component_Structural> nearbyStructurals = GetNearbyStructurals();
        float distance = float.MaxValue;
        SnapPoint_External bestSnapFrom = null;
        SnapPoint_External bestSnapTo = null;

        Component_Bridge nearbyBridge = GetNearbyBridge();

        if (nearbyBridge != null)
        {
            foreach (Face face in new Face[]
            {
               Face.North,
               Face.South,
               Face.East,
               Face.West })
            {
                var overlapArea = OverlapAreas.GetFace(face);

                if (nearbyBridge.ExternalSnapPoints.TryGet(ExternalSnapPoints.Opposite_SmallA(face).Value, out SnapPoint_External externalSnapTo) &&
                    overlapArea.GetOverlappingAreas().Contains(externalSnapTo) &&
                    !externalSnapTo.IsOccupied &&
                    ExternalSnapPoints.TryGet(face, out SnapPoint_External externalSnapFrom) &&
                    !externalSnapFrom.IsOccupied)
                {
                    // check distance between snap points
                    float currentDistance = externalSnapFrom.GlobalPosition.DistanceTo(externalSnapTo.GlobalPosition);
                    GD.Print("From: " + externalSnapFrom.Name + " To: " + externalSnapTo.Name + " Distance: " + currentDistance);
                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                        bestSnapFrom = externalSnapFrom;
                        bestSnapTo = externalSnapTo;
                    }
                } else if (nearbyBridge.ExternalSnapPoints.TryGet(ExternalSnapPoints.Opposite_SmallB(face).Value, out SnapPoint_External externalSnapToB) &&
                    overlapArea.GetOverlappingAreas().Contains(externalSnapToB) &&
                    !externalSnapToB.IsOccupied &&
                    ExternalSnapPoints.TryGet(face, out SnapPoint_External externalSnapFromB) &&
                    !externalSnapFromB.IsOccupied)
                {
                    // check distance between snap points
                    float currentDistance = externalSnapFromB.GlobalPosition.DistanceTo(externalSnapToB.GlobalPosition);
                    GD.Print("From: " + externalSnapFromB.Name + " To: " + externalSnapToB.Name + " Distance: " + currentDistance);
                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                        bestSnapFrom = externalSnapFromB;
                        bestSnapTo = externalSnapToB;
                    }
                }
            }
        }

        if (bestSnapFrom == null || bestSnapTo == null)
        {
            return false;
        }

        var offset = bestSnapFrom.Position;
        _snapPosition = (bestSnapTo.GetParent<Component>() is Component_Structural) ?
            bestSnapTo.GetParent<Component>().Position + bestSnapTo.Position - offset :
            bestSnapTo.Position - offset;
        
        if (!_WouldOverlap(_snapPosition))
        {
            Position = _snapPosition;
            bestSnapFrom.SetIsOccupied();
            bestSnapTo.SetIsOccupied();
            _IsSnapped = true;
            return true;
        }

        return false;
    }
}