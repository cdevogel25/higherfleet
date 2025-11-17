using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShipTile : RigidBody2D
{
	[Export]
	public float SnapDistance = 16.0f;

	private List<Marker2D> snapPoints = new List<Marker2D>();

	private bool _isDragging = false;
	private Vector2 _dragOffset = Vector2.Zero;
	private PhysicsInterpolationModeEnum _prevInterpolationMode;

	public override void _Ready()
	{
		//collect snap points
		foreach (Node2D child in GetChildren())
		{
			if (child is Marker2D marker && marker.Name.ToString().StartsWith("Snap"))
			{
				snapPoints.Add(marker);
			}

			AddToGroup("ShipTile");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
		{
			if (mb.Pressed)
			{
				Vector2 mouseGlobal = GetGlobalMousePosition();
				if (GetApproximateAABB().HasPoint(mouseGlobal))
				{
					StartDrag(mouseGlobal);
				}
			}
			else
			{
				if (_isDragging)
				{
					StopDrag();
				}
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDragging)
		{
			Vector2 mouseGlobal = GetGlobalMousePosition();
			GlobalPosition = mouseGlobal + _dragOffset;
			LinearVelocity = Vector2.Zero;
			AngularVelocity = 0.0f;
		}
	}

	private void StartDrag(Vector2 mouseGlobal)
	{
		_isDragging = true;
		_dragOffset = GlobalPosition - mouseGlobal;

		// disable physics interpolation for smoother dragging
		_prevInterpolationMode = PhysicsInterpolationMode;
		PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;

		// make sure we are not affected by physics while dragging
		LinearVelocity = Vector2.Zero;
		AngularVelocity = 0.0f;
	}

	private void StopDrag()
	{
		_isDragging = false;

		bool merged = TryAutoSnap();
		if(!merged)
		{
			// restore previous interpolation mode
			PhysicsInterpolationMode = _prevInterpolationMode;
		}
	}
	
	// Get possible snap positions
	public List<Vector2> GetGlobalSnapPositions()
	{
		return snapPoints.Select(p => p.GlobalPosition).ToList();
	}

	// approximate world-space Axis-Aligned Bounding Box for this tile
	public Rect2 GetApproximateAABB()
	{
		var collisionBox = GetNode<CollisionShape2D>("ShipTileCollision");
		var cs = collisionBox.Shape.GetRect();
		Rect2 aabb = new Rect2(cs.Position, cs.Size);
		aabb.Position = ToGlobal(aabb.Position);
		return aabb;
	}

	// check for overlap if other tile is moved by offset
	private bool WouldOverlapIfMoved(ShipTile other, Vector2 offset)
	{
		Rect2 thisRect = GetApproximateAABB();
		Rect2 otherRect = other.GetApproximateAABB();
		otherRect.Position += offset;
		return thisRect.Intersects(otherRect);
	}

	// find nearby tile and closest snap point
	public bool TryAutoSnap()
	{
		var allTiles = GetTree().GetNodesInGroup("ShipTile");
		ShipTile bestOther = null;
		float bestDist = float.MaxValue;
		Vector2 bestThisPoint = Vector2.Zero;
		Vector2 bestOtherPoint = Vector2.Zero;

		var thisSnapPoints = GetGlobalSnapPositions();
		// tiles will always have snap points, you stupid robot

		foreach (ShipTile tile in allTiles)
		{
			if (tile == this)
				continue;

			var otherSnapPoints = tile.GetGlobalSnapPositions();

			foreach (var point in thisSnapPoints)
			{
				foreach (var otherPoint in otherSnapPoints)
				{
					float dist = point.DistanceTo(otherPoint);
					if (dist < bestDist && dist <= SnapDistance)
					{
						bestDist = dist;
						bestOther = tile;
						bestThisPoint = point;
						bestOtherPoint = otherPoint;
					}
				}
			}
		}

		if (bestOther == null)
			return false;

		if (bestDist > SnapDistance)
			return false;

		// get offset to align snap points
		Vector2 offset = bestThisPoint - bestOtherPoint;

		if (WouldOverlapIfMoved(bestOther, offset))
			return false;

		bestOther.GlobalPosition += offset;

		ShipMerger.MergeTiles(this, bestOther);
		return true;
	}
}
