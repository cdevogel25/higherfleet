using System;
using System.Collections.Generic;
using Godot;

public partial class Component : Area2D
{
	// possibly not needed, we'll see
	public GraphNode GraphNode = null;
	protected bool _IsMouseOver = false;
	protected bool _IsBeingDragged = false;
	protected bool _IsSnapped = false;

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
}
