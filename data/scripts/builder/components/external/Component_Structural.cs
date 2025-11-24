using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Godot;

public partial class Component_Structural : Component
{
	// for structural components (pretty much just hull tiles,
	// but I want to keep them separate from things like electronics, weapons, storage componets, etc.)
	public HashSet<SnapPoint_External> ExternalSnapPoints = new HashSet<SnapPoint_External>();
	public HashSet<SnapPoint_Internal> InternalSnapPoints = new HashSet<SnapPoint_Internal>();
	private Vector2 _snapPosition = Vector2.Zero;  
	public override void _Ready()
	{
		_IsBeingDragged = true;
		_IsMouseOver = true;
		_CollectSnapPoints();
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
			foreach (SnapPoint_External externalSnapTo in nearbyBridge.ExternalSnapPoints)
			{
				if (!externalSnapTo.IsOccupied)
				{
					foreach (SnapPoint_External externalSnapFrom in ExternalSnapPoints)
					{
						if (!externalSnapFrom.IsOccupied)
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
		} else
		{
			foreach (Component_Structural structural in nearbyStructurals)
			{
				foreach (SnapPoint_External externalSnapTo in structural.ExternalSnapPoints)
				{
					if (!externalSnapTo.IsOccupied)
					{
						foreach (SnapPoint_External externalSnapFrom in ExternalSnapPoints)
						{
							if (!externalSnapFrom.IsOccupied)
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
			}
		}
		
		if (bestSnapFrom == null || bestSnapTo == null)
		{
			return false; // no available snap points found
		} else if (bestSnapFrom.Name.ToString().EndsWith('N'))
		{
			if (!bestSnapTo.Name.ToString().EndsWith('S'))
			{
				return false;
			}
		} else if (bestSnapFrom.Name.ToString().EndsWith('S'))
		{
			if (!bestSnapTo.Name.ToString().EndsWith('N'))
			{
				return false;
			}
		} else if (bestSnapFrom.Name.ToString().EndsWith('E'))
		{
			if (!bestSnapTo.Name.ToString().EndsWith('W'))
			{
				return false;
			}
		} else if (bestSnapFrom.Name.ToString().EndsWith('W'))
		{
			if (!bestSnapTo.Name.ToString().EndsWith('E'))
			{
				return false;
			}
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

	private bool _WouldOverlap(Vector2 snapTo)
	{
		Area2D tempArea = new Area2D();
		tempArea.AddChild(GetNode<CollisionShape2D>("ObjectCollisionShape").Duplicate());
		tempArea.Position = snapTo;
		var overlappingBodies = tempArea.GetOverlappingAreas();
		foreach (var body in overlappingBodies)
		{
			if (body is Component_Structural structural && structural != this)
			{
				GD.Print("Would overlap with structural: " + structural);
				return true;
			} else if (body is Component_Bridge bridge)
			{
				GD.Print("Would overlap with bridge: " + bridge);
				return true;
			}
		}
		return false;
	}

	public Component_Bridge GetNearbyBridge()
	{
		List<Area2D> overlapChecks = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();
		// these have to be either Component_Structural or Component_Bridge
		// how to do that? for now just structural
		foreach (Area2D check in overlapChecks)
		{
			var overlappingAreas = check.GetOverlappingAreas();
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
		List<Area2D> overlapChecks = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();
		// these have to be either Component_Structural or Component_Bridge
		// how to do that? for now just structural
		List<Component_Structural> nearbyStructurals = new List<Component_Structural>();

		foreach (Area2D check in overlapChecks)
		{
			var overlappingAreas = check.GetOverlappingAreas();
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

	private void _CollectSnapPoints()
	{
		foreach (Node2D c in GetChildren())
		{
			if (c is SnapPoint_External externalSnap)
			{
				ExternalSnapPoints.Add(externalSnap);
			}
			else if (c is SnapPoint_Internal internalSnap)
			{
				InternalSnapPoints.Add(internalSnap);
			}
		}
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

	private void _OnPickup()
	{
		// when picked up, free up any occupied snap points
		foreach (var snap in ExternalSnapPoints)
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
			nearbyBridgeSnapPoints = nearbyBridge.ExternalSnapPoints;
		}
		List<Area2D> overlapDetectors = GetChildren().OfType<Area2D>().Where(a => a.Name.ToString().StartsWith("Check")).ToList();

		if (nearbyStructurals.Count == 0 && nearbyBridgeSnapPoints == null)
		{
			return;
		}

		// foreach (SnapPoint_External snap in nearbyBridgeSnapPoints)
		// {
		// 	foreach (Area2D detector in overlapDetectors)
		// 	{
		// 		if (detector.OverlapsArea(snap))
		// 		{
		// 			GD.Print("Freeing snap point on bridge: " + snap);
		// 			if (snap.IsOccupied)
		// 			{
		// 				snap.SetIsUnoccupied();
		// 			}
		// 		}
		// 	}
		// }
		// use signals for this but how?

		foreach (Component_Structural structural in nearbyStructurals)
		{
			foreach (var snap in structural.ExternalSnapPoints)
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
