using System.Collections.Generic;
using System.Linq;
using Godot;
using Builder.Components.External;

public partial class Component_Structural : Component
{
	// for structural components (pretty much just hull tiles,
	// but I want to keep them separate from things like electronics, weapons, storage components, etc.)
	public OverlapArea_Directional OverlapAreas = new OverlapArea_Directional();
	private Vector2 _snapPosition = Vector2.Zero;
	private List<Area2D> _overlapDetectors = new List<Area2D>();
	public override void _Ready()
	{
		_IsBeingDragged = true;
		_IsMouseOver = true;
		_CollectExternalSnapPoints();
		_CollectOverlapAreas();
		_FollowMouse();
	}

	public override void _Input(InputEvent @event)
	{
		if (!_IsMouseOver) return;

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				//try autosnap (make a new autosnap)
				// otherwise drop at mouse position
				if (_IsBeingDragged)
				{
					if (_TrySnap_Structural())
					{
						Position = _snapPosition;
						_IsSnapped = true;
					}
					SetOverlapArea_Visible(false);
					_IsBeingDragged = false;
					return;
				} else
				{
					SetOverlapArea_Visible(true);
					_IsBeingDragged = true;
					_FollowMouse();
					return;
				}
			} else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				if (_IsBeingDragged)
				{
					QueueFree();
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

	private bool _TrySnap_Structural()
	{
		List<Component_Structural> nearbyStructurals = GetNearbyStructurals();
		float distance = float.MaxValue;
		SnapPoint_External bestSnapFrom = null;
		SnapPoint_External bestSnapTo = null;
		// get nearest unoccupied external snap point
		Component_Bridge nearbyBridge = GetNearbyBridge();

		GD.Print("Nearby bridge: " + nearbyBridge);
		GD.Print("Nearby structurals count: " + nearbyStructurals.Count);

		if (nearbyBridge != null)
		{
			// only check the opposing snap points for connection (e.g. if snapping from this north, only check south on the other)
			foreach (Face face in new Face[] { Face.North, Face.South, Face.East, Face.West })
			{
				if (ExternalSnapPoints.TryGet(face, out SnapPoint_External externalSnapFrom) &&
					!externalSnapFrom.IsOccupied &&
					nearbyBridge.ExternalSnapPoints.TryGet(ExternalSnapPoints.Opposite(face).Value, out SnapPoint_External externalSnapTo) &&
					!externalSnapTo.IsOccupied)
				{
					// check distance between snap points
					float currentDistance = externalSnapFrom.GlobalPosition.DistanceTo(externalSnapTo.GlobalPosition);
					if (currentDistance < distance)
					{
						distance = currentDistance;
						bestSnapFrom = externalSnapFrom;
						bestSnapTo = externalSnapTo;
					}
				}	
			}
		} else
		{
			foreach (Component_Structural structural in nearbyStructurals)
			{
				foreach (Face face in new Face[] { Face.North, Face.South, Face.East, Face.West })
				{
					if (ExternalSnapPoints.TryGet(face, out SnapPoint_External externalSnapFrom) &&
						!externalSnapFrom.IsOccupied &&
						structural.ExternalSnapPoints.TryGet(structural.ExternalSnapPoints.Opposite(face).Value, out SnapPoint_External externalSnapTo) &&
						!externalSnapTo.IsOccupied)
					{
						// check distance between snap points
						float currentDistance = externalSnapFrom.GlobalPosition.DistanceTo(externalSnapTo.GlobalPosition);
						if (currentDistance < distance)
						{
							distance = currentDistance;
							bestSnapFrom = externalSnapFrom;
							bestSnapTo = externalSnapTo;
						}
					}
				}
			}
		}
		
		if (bestSnapFrom == null || bestSnapTo == null)
		{
			return false; // no available snap points found
		}

		// you have found the nearest snap point. now check for overlap and do the positioning math
		// the new position 

		var offset = bestSnapFrom.Position; 

		if (bestSnapTo.GetParent<Component>() is Component_Structural)
		{
			_snapPosition = bestSnapTo.GetParent<Component>().Position + bestSnapTo.Position - offset;
		} else
		{
			_snapPosition = bestSnapTo.Position - offset;
		}


		if (!_WouldOverlap(_snapPosition))
		{
			Position = _snapPosition;
			bestSnapFrom.SetIsOccupied();
			bestSnapTo.SetIsOccupied();
			_IsSnapped = true;
			return true;
		}


		return false;
	}

	// this might not be necessary anymore
	private bool _WouldOverlap(Vector2 snapTo)
	{
		Area2D tempArea = new Area2D();
		tempArea.AddChild(GetNode<CollisionShape2D>("ObjectCollisionShape").Duplicate());
		tempArea.Position = snapTo;
		GetParent().AddChild(tempArea);
		var overlappingBodies = tempArea.GetOverlappingAreas();
		bool result = false;
		foreach (var body in overlappingBodies)
		{
			if (body is Component_Structural structural && structural != this)
			{
				GD.Print("Would overlap with structural: " + structural);
				result = true;
			} else if (body is Component_Bridge bridge)
			{
				GD.Print("Would overlap with bridge: " + bridge);
				result = true;
			}
		}
		GetParent().RemoveChild(tempArea);
		tempArea.QueueFree();
		return result;
	}

	public Component_Bridge GetNearbyBridge()
	{
		// these have to be either Component_Structural or Component_Bridge
		// how to do that? for now just structural
		foreach (Area2D overlap in _overlapDetectors)
		{
			var overlappingAreas = overlap.GetOverlappingAreas();
			foreach (var area in overlappingAreas)
			{
				if (area is Component_Bridge bridge)
				{
					GD.Print("Found nearby bridge: " + bridge);
					return bridge;
				}
			}
		}
		return null;
	}

	public List<Component_Structural> GetNearbyStructurals()
	{
		// these have to be either Component_Structural or Component_Bridge
		// how to do that? for now just structural
		List<Component_Structural> nearbyStructurals = new List<Component_Structural>();

		foreach (Area2D overlap in _overlapDetectors)
		{
			var overlappingAreas = overlap.GetOverlappingAreas();
			foreach (var area in overlappingAreas)
			{
				if (area is Component_Structural structural && structural != this)
				{
					nearbyStructurals.Add(structural);
				}
			}
		}
		return nearbyStructurals;
	}

	private void _CollectOverlapAreas()
	{
		_overlapDetectors = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();
		// foreach (var child in GetChildren())
		// {
		// 	if (child is Area2D area && area.Name.ToString().StartsWith("Check"))
		// 	{
		// 		switch (area.Name)
		// 		{
		// 			case "Check_North": OverlapAreas.North = area; break;
		// 			case "Check_South": OverlapAreas.South = area; break;
		// 			case "Check_East": OverlapAreas.East = area; break;
		// 			case "Check_West": OverlapAreas.West = area; break;
		// 		}
		// 	}
		// }
	}

	private void SetOverlapArea_Visible(bool isVisible)
	{
		foreach (Area2D overlap in _overlapDetectors)
		{
			var sprite = overlap.GetNode<Sprite2D>("Sprite2D");
			if (sprite != null)
			{
				sprite.Visible = isVisible;
			}
		}
	}

	private void _OnPickup()
	{
		// when picked up, free up any occupied snap points
		foreach (var snap in ExternalSnapPoints.All)
		{
			if (snap.IsOccupied)
			{
				snap.SetIsUnoccupied();
			}
		}
		// free neighboring snap points that this was snapped to
		// do I need to change the collision layer of snap points and overlap detectors?
		List<Component_Structural> nearbyStructurals = GetNearbyStructurals();
		GD.Print("Nearby structurals count on pickup: " + nearbyStructurals.Count);
		HashSet<SnapPoint_External> nearbyBridgeSnapPoints = null;
		Component_Bridge nearbyBridge = GetNearbyBridge();
		if (nearbyBridge != null)
		{
			nearbyBridgeSnapPoints = nearbyBridge.ExternalSnapPoints.All.ToHashSet();
		}
		List<Area2D> overlapDetectors = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();

		if (nearbyStructurals.Count == 0 && nearbyBridgeSnapPoints == null)
		{
			return;
		}

		foreach (Component_Structural structural in nearbyStructurals)
		{
			foreach (var snap in structural.ExternalSnapPoints.All)
			{
				foreach (Area2D detector in overlapDetectors)
				{
					var overlappingAreas = detector.GetOverlappingAreas();
					foreach (var area in overlappingAreas)
					{
						if (area is SnapPoint_External otherSnap && otherSnap == snap && snap.IsOccupied)
						{
							snap.SetIsUnoccupied();
						}
					}
				}
			}
		}
	}
}
