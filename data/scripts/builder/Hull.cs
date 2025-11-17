using Godot;
using System;
using System.Collections.Generic;

public partial class Hull : Area2D
{
	public bool _isDragging = true;
	private Tween _tween;
	private bool _isMouseOver = true;
	private float SnapDistance = 32.0f;
	private List<Marker2D> snapPoints = new List<Marker2D>();
	public Vector2 SnapPosition = Vector2.Zero;
    private PhysicsPointQueryParameters2D query;
	public override void _Ready()
	{
		foreach (Node2D c in GetChildren())
		{
			if (c is Marker2D marker && marker.Name.ToString().StartsWith("Snap"))
			{
				snapPoints.Add(marker);
			}
		}
		Position = GetGlobalMousePosition();
		GD.Print(GetTree().GetNodesInGroup("HullTiles"));
	}

	public override void _Input(InputEvent @event)
	{
		if (!_isMouseOver) return;
		if (@event is InputEventMouseButton mouseEvent)
		{
            query = new PhysicsPointQueryParameters2D();
            query.Position =  mouseEvent.Position;
            var objectsClicked = GetWorld2D().GetDirectSpaceState().IntersectPoint(query);
            GD.Print("Objects clicked: " + objectsClicked);
            
			if (_isDragging && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				// Position = GetGlobalMousePosition();
				if (TryAutoSnap())
				{
					Position = SnapPosition;
				} else
				{
					Position = GetGlobalMousePosition();
				}
				_isDragging = false;
				GD.Print(Name + " placed at: " + Position);
			} else if (_isDragging && mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				QueueFree();
				GD.Print(Name + " placement cancelled.");
			} else if (!_isDragging && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				_isDragging = true;
				FollowMouse();
				
			}
		}

		if (@event is InputEventMouseMotion)
		{
			FollowMouse();      
		}
	}

	public List<Marker2D> GetGlobalSnapPoints()
	{
		List<Marker2D> globalPoints = [.. snapPoints];
		return globalPoints;
	}

	public bool TryAutoSnap()
	{
		var allHulls = GetTree().GetNodesInGroup("HullTiles");
		Hull nearestHull = null;
		float nearestDist = float.MaxValue;
		Marker2D nearestSnapFrom = null;
		Marker2D nearestSnapTo = null;
		List<Marker2D> snapFromPoints;
		List<Marker2D> snapToPoints;

		snapFromPoints = GetGlobalSnapPoints();

		foreach (Hull tile in allHulls)
		{
			if (tile == this)
			{
				GD.Print("Skipping self");
				continue;
			}
			snapToPoints = tile.GetGlobalSnapPoints();

			foreach (var from in snapFromPoints)
			{
				foreach (var to in snapToPoints)
				{
					float dist = from.GlobalPosition.DistanceTo(to.GlobalPosition);
					if (dist < nearestDist && dist <= SnapDistance)
					{
						nearestDist = dist;
						nearestHull = tile;
						nearestSnapFrom = from;
						nearestSnapTo = to;
						GD.Print(from.Name + " to " + to.Name + " distance: " + dist);
					}
				}
			}
		}

		if (nearestHull == null)
			return false;
		
		GD.Print("Nearest hull found: " + nearestHull.Name);
		
		if (nearestDist > SnapDistance)
			return false;

		// hull should snap to the nearest snap point offset by the distance between the center (Position) and the snap point (nearestSnapFrom)
		Vector2 offset = Position - nearestSnapFrom.GlobalPosition;
		SnapPosition = nearestSnapTo.GlobalPosition + offset;

		if (WouldOverlap(nearestHull, SnapPosition)) {
			GD.Print("Would overlap with " + nearestHull.Name);
			return false;
		}
	
		return true;
	}

	private bool WouldOverlap(Hull other, Vector2 SnapPosition)
	{
		// if the bounding boxes of this hull at SnapPosition and the other hull overlap, return true
		Vector2 thisSize = GetNode<CollisionShape2D>("HullCollision").Shape.GetRect().Size;
		var overlapRect = new Rect2(SnapPosition, thisSize * Scale);
		Rect2 otherPosition = other.GetNode<CollisionShape2D>("HullCollision").Shape.GetRect();
        otherPosition = new Rect2(other.GlobalPosition, otherPosition.Size * Scale);
        GD.Print("from position " + overlapRect.Position + " to position " + otherPosition.Position);
		return overlapRect.Intersects(otherPosition);
	}

	private void FollowMouse()
	{
		if (!_isDragging) return;

		Visible = true;
		Vector2 mousePos = GetGlobalMousePosition();
		GlobalPosition = mousePos;
	}

	private void OnMouseEntered()
	{
		_isMouseOver = true;
	}

	private void OnMouseExited()
	{
		_isMouseOver = false;
	}
}
