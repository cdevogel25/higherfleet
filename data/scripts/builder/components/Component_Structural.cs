using System.Collections.Generic;
using Godot;

public partial class Component_Structural : Component
{
    // for structural components (pretty much just hull tiles,
    // but I want to keep them separate from things like electronics, weapons, storage componets, etc.)
    public HashSet<SnapPoint_External> ExternalSnapPoints = new HashSet<SnapPoint_External>();
    public HashSet<SnapPoint_Internal> InternalSnapPoints = new HashSet<SnapPoint_Internal>();
    public override void _Ready()
    {
        _IsBeingDragged = true;
        _IsMouseOver = true;
        _FollowMouse();
        _CollectSnapPoints();
    }

    public override void _Input(InputEvent @event)
    {
        if (!_IsMouseOver) return;

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (_IsBeingDragged)
            {
                //try autosnap (make a new autosnap)
                // otherwise drop at mouse position
            }
        }
    }

    private bool _TrySnap_Structural()
    {
        List<Component_Structural> nearbyStructurals = GetNearbyStructurals();
        
        if (nearbyStructurals.Count > 0)
        {
            // float distance = float.MaxValue;
        }
        return false;
    }

    public List<Component_Structural> GetNearbyStructurals()
    {
        return null;
    }

    private void _CollectSnapPoints()
    {
        foreach (Node2D c in GetChildren())
        {
            if (c is SnapPoint_External externalSnap)
            {
                ExternalSnapPoints.Add(externalSnap);
            }
            else if (c is SnapPoint_Internal internalSnap)
            {
                InternalSnapPoints.Add(internalSnap);
            }
        }
    }
}