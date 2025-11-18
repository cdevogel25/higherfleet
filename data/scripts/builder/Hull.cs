using Godot;
using System;
using System.Collections.Generic;

public partial class Hull : BuilderObject_Placeable
{
	public bool _isDragging = true;
	private Tween _tween;
	private bool _isMouseOver = true;
	private float SnapDistance = 32.0f;
	private List<Marker2D> snapPoints = new List<Marker2D>();
    private PhysicsPointQueryParameters2D query;
}
