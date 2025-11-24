using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BuilderObject_Placeable : Area2D
{
	// what do other objects need to know about a placeable object?
	public bool IsSnapped = false;
	public bool IsBeingDragged = false;
	public bool IsDropped = false;
	public bool IsRoot = false;
	public Vector2 RootOffset = Vector2.Zero;
	
	private bool _isMouseOver = false;
	public List<Marker2D> SnapPoints = new List<Marker2D>();
	private Vector2 _snapPosition = Vector2.Zero;
	// private bool _isOverlapAreaVisible = false;

	// graph node for this placeable object
	public ShipGraphNode GraphNode = null;

	public override void _Ready()
	{
		GraphNode = new ShipGraphNode(this);
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
		// [TODO]
		// left mouse will pick up a single tile (if not the root tile)
		// right mouse will cancel placement of a currently-dragged tile
		// right mouse will pick up a tile that is already placed and all its connected tiles
		// right mouse will also pick up the root tile and connected tiles if any exist

		if (!_isMouseOver) return;

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				if (IsBeingDragged)
				{
					// bool autoSnap = TryAutoSnap();
					if (TryAutoSnap() && !IsRoot)
					{
						Position = _snapPosition;
						RootOffset = Position;
						IsSnapped = true;
						_DrawRootLine();
					} else
					{
						Position = GetGlobalMousePosition();
					}
					SetOverlapArea_Visible(false);
					IsBeingDragged = false;
					return;
				} else
				{
					if (GetParent() == GetTree().Root)
					{
						IsSnapped = false;
					} else if (GetParent() == GetTree().Root.GetNode("Node2D/RootHull"))
					{
						IsSnapped = false;
						Reparent(GetTree().Root.GetNode("Node2D"));
					}
					_EraseRootLine();
					RootOffset = Vector2.Zero;
					SetOverlapArea_Visible(true);
					IsBeingDragged = true;
					_FollowMouse();
					return;
				}
			} else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				if (IsBeingDragged)
				{
					if (IsRoot)
					{
						IsBeingDragged = false;
						return;
					}
					QueueFree();
					return;
				}
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
		List <BuilderObject_Placeable> neighbors = GetNeighbors();

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

			BuilderObject_Placeable snapToParent = nearestSnapTo.GetParent<BuilderObject_Placeable>();
			Node2D root = _FindRoot(snapToParent);

			// offset is the distance from the center of this tile to the selected snap-from point
			// snapPosition should be the position of the snap-to point minus the offset, plus the snap-to object's root offset
			var offset = nearestSnapFrom.Position;
			_snapPosition = nearestSnapTo.Position - offset + nearestSnapTo.GetParent<BuilderObject_Placeable>().RootOffset; // is this neighbors[0].RootOffset? Try both.
			// GD.Print(_WouldOverlap(snapToParent, _snapPosition));
			if(!_WouldOverlap(snapToParent, _snapPosition))
			{
				// snap and reparent
				if(!IsAncestorOf(snapToParent))
				{
					Position = _snapPosition;
					Reparent(snapToParent);
					return true;
				}
				Position = _snapPosition;
				Reparent(root);
				return true;
			} else
			{
				// i dont think this is right
				// afaik doesn't do anything, but let's see
				if (!IsSnapped && !IsAncestorOf(snapToParent))
				{
					Reparent(snapToParent);
					GD.Print("Reparented to: " + GetTree().Root.GetNode("Node2D/RootHull"));
				}
				return true;
			}
		}

		return false;
	}

	private Node2D _FindRoot(Node2D obj)
	{
		if (obj is RootHull)
		{
			return obj;
		} else if (obj.GetParent() == GetTree().Root)
		{
			return GetTree().Root.GetNode("Node2D") as Node2D;
		} else
		{
			return _FindRoot(obj.GetParent<Node2D>());
		}
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

	// _WouldOverlap is not working correctly. Why?
	// origin of rect2 is the top left stupid (so you need to move it by half size to center it)
	// but also why is it like that
	private bool _WouldOverlap(BuilderObject_Placeable nearestObject, Vector2 snapTo)
	{
		Area2D tempArea = new Area2D();
		tempArea.AddChild(GetNode<CollisionShape2D>("ObjectCollisionShape").Duplicate());
		tempArea.Position = snapTo;
		var overlappingAreas = tempArea.GetOverlappingAreas();
		foreach (var area in overlappingAreas)
		{
			if (area is BuilderObject_Placeable placeable && placeable != this)
			{
				return true;
			}
		}
		return false;
	}

	public List<BuilderObject_Placeable> GetNeighbors()
	{
		List<Area2D> checkers = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();

		List<BuilderObject_Placeable> neighbors = new List<BuilderObject_Placeable>();

		foreach (Area2D checker in checkers)
		{
			var overlappingAreas = checker.GetOverlappingAreas();
			foreach (var area in overlappingAreas)
			{
				if (area is BuilderObject_Placeable placeable && placeable != this)
				{
					GraphNode.AddNeighbor(checker.Name.ToString()[6..], placeable.GraphNode);
					neighbors.Add(placeable);
				}
			}
		}
		GD.Print("Found " + neighbors.Count + " neighbor(s).");
		return neighbors;		
	}

	private void SetOverlapArea_Visible(bool isVisible)
	{
		List<Area2D> checkers = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();
		foreach (Area2D checker in checkers)
		{
			var sprite = checker.GetNode<Sprite2D>("Sprite2D");
			if (sprite != null)
			{
				sprite.Visible = isVisible;
			}
		}
	}

	private void OnMouseEntered()
	{
		_isMouseOver = true;
	}

	private void OnMouseExited()
	{
		_isMouseOver = false;
	}

	private void _DrawRootLine()
	{
		if (IsRoot)
		{
			return;
		}
		Line2D line = new Line2D();
		line.Position = Vector2.Zero;
		line.DefaultColor = Colors.Red;
		line.Width = 10.0f;
		line.AddPoint(-RootOffset);
		line.AddPoint(Vector2.Zero);
		line.SetVisibilityLayerBit(1, true);
		AddChild(line);
	}

	private void _EraseRootLine()
	{
		var lines = GetChildren().OfType<Line2D>().ToList();
		foreach (var line in lines)
		{
			line.QueueFree();
		}
	}
}
