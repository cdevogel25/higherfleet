using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

public partial class BuilderObject_Placeable : Area2D
{
	// what do other objects need to know about a placeable object?
	// is it snapped in place?
	public bool IsSnapped = false;
	// is it connected to the root object?
	public bool IsConnectedToRoot = false;
	// is it being dragged?
	public bool IsBeingDragged = false;
	// is it dropped?
	public bool IsDropped = false;
	public bool IsRoot = false;
	public Vector2 RootOffset = Vector2.Zero;
	
	// ignore spawn click
	private bool _ignoreInitialLeftClick = true;
	private bool _isMouseOver = false;
	private float _snapDistance = 32.0f;
	public List<Marker2D> SnapPoints = new List<Marker2D>();
	private Vector2 _snapPosition = Vector2.Zero;

	public override void _Ready()
	{
		foreach (Node2D c in GetChildren())
		{
			if (c is Marker2D marker && marker.Name.ToString().StartsWith("Snap"))
			{
				SnapPoints.Add(marker);
			}
		}
		IsBeingDragged = true;
		_isMouseOver = true;
		_FollowMouse();
	}

	/* Every placeable object should have:
	*  - Snapping (TryAutoSnap)
	*  - Input handling for drag/drop
	*  - Root finding and connection logic
	*  - Visual feedback for snapping and connection status
	*  - Edge detection (if the tile is on the edge of the structure and another
	*    tile is picked up, available snap points should be highlighted)
	*  - overlap detection
	*/
	
	// method skeletons
	public override void _Input(InputEvent @event)
	{
		// left mouse will pick up a single tile (if not the root tile)
		// right mouse will cancel placement of a currently-dragged tile
		// right mouse will pick up a tile that is already placed and all its connected tiles
		// right mouse will also pick up the root tile and connected tiles if any exist

		// ok now is the time to fix input
		if (!_isMouseOver) return;
		if (@event is InputEventMouseButton mouseEvent)
		{
			// this will prevent pickup once objects are snapped [[TEMP!]]
			// if (GetParent() != GetTree().Root)
			// {
			// 	return;
			// }

			// do you want place-on-press or place-on-release?
			if (!IsBeingDragged && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				if (GetParent() == GetTree().Root)
				{
					IsSnapped = false;
				} else if (GetParent() == GetTree().Root.GetNode("Node2D/RootHull"))
				{
					IsSnapped = false;
					Reparent(GetTree().Root);
				}
				IsBeingDragged = true;
				_FollowMouse();
			} else if (IsBeingDragged && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				if (!IsRoot && _isMouseOver && TryAutoSnap())
				{
					
					Position = _snapPosition;
					GD.Print("Position relative to parent after snap: " + Position);
					RootOffset = Position;
					IsSnapped = true;
				}
				else
				{
					Position = GetGlobalMousePosition();
				}
				IsBeingDragged = false;
				// GD.Print(Name + " placed at: " + Position);
			} else if (IsBeingDragged && mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				if (IsRoot)
				{
					IsBeingDragged = false;
					return;
				}
				QueueFree();
				// GD.Print(Name + " discarded.");
			}
		}

		if(@event is InputEventMouseMotion && IsBeingDragged)
		{
			_FollowMouse();
		}
	}

	public bool TryAutoSnap()
	{
		// you need new snapping logic, cuz what you have
		// in Hull.cs is not good
		var allTiles = GetTree().GetNodesInGroup("BuilderObjectsPlaceable");

		BuilderObject_Placeable nearestObject = null;
		Marker2D nearestSnapFrom = null;
		Marker2D nearestSnapTo = null;
		List<Marker2D> snapToPoints;

		float nearestDistance = float.MaxValue;

		foreach (BuilderObject_Placeable tile in allTiles)
		{
			if (tile == this) continue;
			snapToPoints = tile.SnapPoints;

			foreach (Marker2D snapFrom in SnapPoints)
			{
				foreach (Marker2D snapTo in snapToPoints)
				{
					float distance = snapFrom.GlobalPosition.DistanceTo(snapTo.GlobalPosition);

					if (distance < nearestDistance && distance <= _snapDistance)
					{
						nearestDistance = distance;
						nearestObject = tile;

						nearestSnapFrom = snapFrom;
						nearestSnapTo = snapTo;
					}
				}
			}
		}
		GD.Print("NearestSnapTo: " + nearestSnapTo + " NearestSnapFrom: " + nearestSnapFrom + " NearestDistance: " + nearestDistance);

		if (nearestObject != null && nearestDistance <= _snapDistance)
		{
			Vector2 offset = nearestSnapFrom.Position;
			if (!_WouldOverlap(nearestObject, offset))
			{
				_snapPosition = nearestSnapTo.Position - offset + nearestObject.RootOffset;

				if (!IsSnapped && !IsAncestorOf(nearestObject)) {
					Reparent(GetTree().Root.GetNode("/root/Node2D/RootHull"));
				}
				return true;
			}
		}

		return false;
	}



	private void _FindRoot()
	{
		// the root object will inherit from this class
		if (IsRoot) return;
		// logic to find and connect to root object
	}

	public void EdgeDetection()
	{
		// logic for edge detection and highlighting snap points
		// check all snap points to see if anything is connected
	}


	private void _FollowMouse()
	{
		if (IsBeingDragged)
		{
			Position = GetGlobalMousePosition();
		}
	}

	private bool _WouldOverlap(BuilderObject_Placeable nearestObject, Vector2 offset)
	{
		// logic to check for overlap with other objects
		Rect2 collider = new Rect2(_snapPosition + offset, GetNode<CollisionShape2D>("ObjectCollisionShape").Shape.GetRect().Size * Scale);
		return nearestObject.GetNode<CollisionShape2D>("ObjectCollisionShape").Shape.GetRect().Intersects(collider);
	}

	// this is a hacky and bad way to do this but it has to work for now
	private void OnMouseEntered()
	{
		_isMouseOver = true;
	}

	private void OnMouseExited()
	{
		_isMouseOver = false;
	}
}
