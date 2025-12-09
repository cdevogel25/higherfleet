using Godot;
using System;

public partial class FleetStrategicView : Area2D
{
	public Vector2 destination;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				destination = GetGlobalMousePosition();
			}
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		destination = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (destination != GlobalPosition)
		{
			Vector2 direction = (destination - GlobalPosition).Normalized();
			float speed = 200f; // Adjust speed as needed
			Vector2 movement = direction * speed * (float)delta;

			if (movement.Length() > (destination - GlobalPosition).Length())
			{
				GlobalPosition = destination;
			}
			else
			{
				GlobalPosition += movement;
			}
		}
	}

	public Vector2 GetDestination()
	{
		destination = GetGlobalMousePosition();
		return destination;
	}
}
