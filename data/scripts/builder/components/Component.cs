using Builder.Components.External;
using Godot;

public partial class Component : Area2D
{
	// possibly not needed, we'll see
	public GraphNode GraphNode = null;
	protected bool _IsMouseOver = false;
	protected bool _IsBeingDragged = false;
	protected bool _IsSnapped = false;
	public SnapPoint_Directional ExternalSnapPoints = new SnapPoint_Directional();

	protected void _OnMouseEntered()
	{
		_IsMouseOver = true;
	}

	protected void _OnMouseExited()
	{
		_IsMouseOver = false;
	}

	protected void _FollowMouse()
	{
		if (_IsBeingDragged)
		{
			GlobalPosition = GetGlobalMousePosition();
		}
	}

	protected void _CollectExternalSnapPoints()
    {
        foreach (var child in GetChildren())
		{
			if (child is SnapPoint_External snapPoint)
			{
				switch (snapPoint.Name)
				{
					case "SnapPoint_External_North": ExternalSnapPoints.North = snapPoint; break;
					case "SnapPoint_External_South": ExternalSnapPoints.South = snapPoint; break;
					case "SnapPoint_External_East": ExternalSnapPoints.East = snapPoint; break;
					case "SnapPoint_External_West": ExternalSnapPoints.West = snapPoint; break;
				}
			}
		}
    }
}
