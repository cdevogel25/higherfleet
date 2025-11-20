using Godot;
using System.Collections.Generic;
using System.Linq;

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
	// private bool _isOverlapAreaVisible = false;

	// graph node for this placeable object
	public ShipGraphNode GraphNode = null;

	public override void _Ready()
	{
		GraphNode = new ShipGraphNode(this, IsRoot);
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
			// is picked up and left mouse button is pressed
			if (IsBeingDragged && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				bool autoSnap = TryAutoSnap();
				GD.Print("AutoSnap result: " + autoSnap);
				if (autoSnap && !IsRoot)
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
				SetOverlapArea_Visible(false);
				IsBeingDragged = false;
				return;
				// GD.Print(Name + " placed at: " + Position);
			} else if (!IsBeingDragged && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				if (GetParent() == GetTree().Root)
				{
					IsSnapped = false;
				} else if (GetParent() == GetTree().Root.GetNode("Node2D/RootHull"))
				{
					IsSnapped = false;
					Reparent(GetTree().Root);
				}
				SetOverlapArea_Visible(true);
				IsBeingDragged = true;
				_FollowMouse();
				return;
			} else if (IsBeingDragged && mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				if (IsRoot)
				{
					IsBeingDragged = false;
					return;
				}
				QueueFree();
				// GD.Print(Name + " discarded.");
				return;
			}
		}

		if(@event is InputEventMouseMotion && IsBeingDragged)
		{
			_FollowMouse();
		}
	}

	public bool TryAutoSnap()
	{
		// logic to find nearest snap point on nearby placeable objects
		// check in order: north, east, south, west

		// first: get all neighbors
		// then: for the first (if any) neighbor, get its snap points
		// then: check distance from this object to each of neighbor's snap points
		// then: snap to the closest snap point and reparent.
		List <BuilderObject_Placeable> neighbors = new List<BuilderObject_Placeable>();
		neighbors = GetNeighbors();

		if (neighbors.Count > 0)
		{
			List<Marker2D> potentialSnapPoints = neighbors[0].SnapPoints;
			float distance = float.MaxValue;
			Marker2D nearestSnapTo = null;
			Marker2D nearestSnapFrom = null;

			foreach (Marker2D snapFrom in SnapPoints)
			{
				foreach (Marker2D snapTo in potentialSnapPoints)
				{
					float currentDistance = snapFrom.GlobalPosition.DistanceTo(snapTo.GlobalPosition);
					if (currentDistance < distance)
					{
						distance = currentDistance;
						nearestSnapTo = snapTo;
						nearestSnapFrom = snapFrom;
					}
				}
			}

			Vector2 offset = nearestSnapFrom.Position;
			if (!_WouldOverlap(neighbors[0], offset))
			{
				_snapPosition = nearestSnapTo.Position - offset + neighbors[0].RootOffset;

				if (!IsSnapped && !IsAncestorOf(neighbors[0]))
				{
					Reparent(GetTree().Root.GetNode("Node2D/RootHull"));
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

	// use something similar for neighbor detection
	private bool _WouldOverlap(BuilderObject_Placeable nearestObject, Vector2 offset)
	{
		// logic to check for overlap with other objects
		Rect2 collider = new Rect2(_snapPosition + offset, GetNode<CollisionShape2D>("ObjectCollisionShape").Shape.GetRect().Size * Scale);
		return nearestObject.GetNode<CollisionShape2D>("ObjectCollisionShape").Shape.GetRect().Intersects(collider);
	}

	// this needs some changes to be used in TryAutoSnap
	public List<BuilderObject_Placeable> GetNeighbors()
	{
		// logic to get neighboring placeable objects
		// first: get the neighbor checkers
		// THIS FIXES EVERYTHING, use this for snapping.
		List<Area2D> checkers = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("NeighborCheck")).ToList();
		List<BuilderObject_Placeable> neighbors = new List<BuilderObject_Placeable>();

		foreach (Area2D checker in checkers)
		{
			var overlappingAreas = checker.GetOverlappingAreas();
			foreach (var area in overlappingAreas)
			{
				if (area is BuilderObject_Placeable placeable && placeable != this)
				{
					neighbors.Add(placeable);
				}
			}
		}

		return neighbors;		
	}

	private void SetOverlapArea_Visible(bool isVisible)
	{
		List<Area2D> checkers = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("NeighborCheck")).ToList();
		foreach (Area2D checker in checkers)
		{
			var sprite = checker.GetNode<Sprite2D>("Sprite2D");
			if (sprite != null)
			{
				sprite.Visible = isVisible;
			}
		}
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
