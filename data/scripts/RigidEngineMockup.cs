using Godot;
using System;

public partial class RigidEngineMockup : RigidBody2D
{
	[Export]
	public float mass = 10.0f;
	[Export]
	public float rotationSpeed = 6.0f;
	[Export]
	public float thrust = 100.0f;

	public Sprite2D thrustSprite;
	private float initialRotation;

	public override void _Ready()
	{
		thrustSprite = GetNode<Sprite2D>("thrustSprite");
		initialRotation = Rotation;
	}

	public void GetInput(double delta)
	{
		Vector2 dir = Vector2.Zero;

		if (
			Input.IsActionPressed("up") ||
			Input.IsActionPressed("down") ||
			Input.IsActionPressed("left") ||
			Input.IsActionPressed("right")
		)
		{
			if(Input.IsActionPressed("up")) dir.Y = 1;
			if(Input.IsActionPressed("down")) dir.Y = -1;
			if(Input.IsActionPressed("left")) dir.X = -1;
			if(Input.IsActionPressed("right")) dir.X = 1;

			dir = dir.Normalized();
			thrustSprite.Visible = true;
			ApplyForce(Vector2.Right.Rotated(Rotation) * thrust * (float)delta);
		} else
		{
			dir = Vector2.Zero;
			thrustSprite.Visible = false;
		}

		float targetAngle = dir != Vector2.Zero ? dir.Angle() : initialRotation;
		float currentAngle = Rotation;
		float desiredAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * (float)delta);

		float requiredAngVel = (desiredAngle - currentAngle) / (float)delta;
		AngularVelocity = Mathf.Clamp(requiredAngVel, -rotationSpeed, rotationSpeed);

		if (dir == Vector2.Zero)
		{
			Vector2 forward = new Vector2(0, -1).Rotated(Rotation);
			ApplyForce(forward * thrust);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput(delta);
	}
}
