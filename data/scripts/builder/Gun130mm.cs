using Godot;
using System;

public partial class Gun130mm : Area2D
{
	float RotationSpeed = Mathf.Pi * 0.75f;
	public override void _Process(double delta)
	{
		Vector2 mouseDirection = GetGlobalMousePosition() - GlobalPosition;
		float targetAngle = mouseDirection.Angle();
		float angleDifference = Mathf.Wrap(targetAngle - Rotation, -Mathf.Pi, Mathf.Pi);
		float maxRotationThisFrame = RotationSpeed * (float)delta;

		Rotation = Mathf.LerpAngle(Rotation, targetAngle, Math.Min(1, maxRotationThisFrame / Math.Abs(angleDifference)));
	}
}
