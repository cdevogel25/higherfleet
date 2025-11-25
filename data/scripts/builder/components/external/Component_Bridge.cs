using System.Collections;
using System.Collections.Generic;
using Godot;
using Builder.Components.External;

public partial class Component_Bridge : Component
{
	// public HashSet<SnapPoint_External> ExternalSnapPoints = new HashSet<SnapPoint_External>();
	public SnapPoint_Directional ExternalSnapPoints = new SnapPoint_Directional();

	public override void _Ready()
	{
		_IsBeingDragged = false;
		_IsMouseOver = false;
		_CollectSnapPoints();
	}

	public override void _Input(InputEvent @event)
	{
		if (!_IsMouseOver) return;

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				if(_IsBeingDragged)
				{
					_IsBeingDragged = false;
					return;
				} else
				{
					_IsBeingDragged = true;
					_FollowMouse();
					return;
				}
			}
		}

		if (@event is InputEventMouseMotion)
		{
			if (_IsBeingDragged)
			{
				_FollowMouse();
			}
		}
	}
	private void _CollectSnapPoints()
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