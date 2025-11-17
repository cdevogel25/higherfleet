using Godot;
using System;

public partial class EngineMockup : Node2D
{
	[Export]
	public float rotationSpeed = 6.0f;
	[Export]
	public float thrust = 5000.0f;

	public Sprite2D thrustSprite;
	private float initialRotation;

	public override void _Ready()
	{
		thrustSprite = GetNode<Sprite2D>("thrustSprite");
		initialRotation = Rotation;
	}

	// Compute visual rotation and return the engine's thrust vector in global coordinates.
	public Vector2 GetGlobalThrust(double delta)
	{
		Vector2 dir = Vector2.Zero;

		if (
			Input.IsActionPressed("up") ||
			Input.IsActionPressed("down") ||
			Input.IsActionPressed("left") ||
			Input.IsActionPressed("right")
		)
		{
			if (Input.IsActionPressed("up")) dir.Y -= 1;
			if (Input.IsActionPressed("down")) dir.Y += 1;
			if (Input.IsActionPressed("left")) dir.X -= 1;
			if (Input.IsActionPressed("right")) dir.X += 1;

			dir = dir.Normalized();
			thrustSprite.Visible = true;
		}
		else
		{
			dir = Vector2.Zero;
			thrustSprite.Visible = false;
		}

		float targetAngle = dir != Vector2.Zero ? dir.Angle() : initialRotation;
		float currentAngle = Rotation;
		Rotation = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * (float)delta);

		// Engine forward is "up" in local coords. Rotate by global rotation and return world-space force.
		Vector2 forward = new Vector2(0, -1).Rotated(Rotation);
		return forward * thrust;
	}
}
