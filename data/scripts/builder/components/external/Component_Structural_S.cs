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
                if (OverlapAreas.TryGet(face, out Area2D overlapArea) &&
                   (overlapArea.GetOverlappingAreas().Contains(nearbyBridge.ExternalSnapPoints[ExternalSnapPoints.Opposite_SmallA(face).Value]) ||
                    overlapArea.GetOverlappingAreas().Contains(nearbyBridge.ExternalSnapPoints[ExternalSnapPoints.Opposite_SmallB(face).Value])) &&
                    ExternalSnapPoints.TryGet(face, out SnapPoint_External externalSnapFrom) &&
                    !externalSnapFrom.IsOccupied)
                {
                    // help me what even is this if statement
                }
            }
        }
        return false;
    }
}