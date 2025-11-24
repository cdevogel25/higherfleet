using System.Collections.Generic;
using Godot;

public partial class Component_Bridge : Component
{
    public HashSet<SnapPoint_External> ExternalSnapPoints = new HashSet<SnapPoint_External>();

    public override void _Ready()
    {
        _IsBeingDragged = true;
        _IsMouseOver = true;
        _CollectSnapPoints();
    }

    private void _CollectSnapPoints()
    {
        ExternalSnapPoints.Clear();
        foreach (var child in GetChildren())
        {
            if (child is SnapPoint_External snapPoint)
            {
                ExternalSnapPoints.Add(snapPoint);
            }
        }
    }
}