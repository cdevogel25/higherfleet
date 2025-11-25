using Godot;
using Builder.Components.External;

public partial class Component_Bridge : Component
{
	[Export]
	public NodePath ComponentPath;

	public override void _Ready()
	{
		ComponentPath = GetPath();
		_IsBeingDragged = false;
		_IsMouseOver = false;
		_CollectExternalSnapPoints();
	}

	public override void _Input(InputEvent @event)
	{
		if (!_IsMouseOver) return;

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				if (_IsBeingDragged)
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
}