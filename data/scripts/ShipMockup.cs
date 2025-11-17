using Godot;
using System;

public partial class ShipMockup : RigidBody2D
{
	[Export]
	public float turnBias = 0.6f;
	[Export]
	public float mass = 60.0f;

	[Export]
	public NodePath leftEnginePath;
	[Export]
	public NodePath rightEnginePath;

	private EngineMockup leftEngine;
	private EngineMockup rightEngine;

	private int thrustDecay = 1000;

	public override void _Ready()
	{
		leftEngine = GetNode<EngineMockup>("leftEngine");
		rightEngine = GetNode<EngineMockup>("rightEngine");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Get thrust from each engine (world-space)
		if (leftEngine != null)
		{
			Vector2 leftForce = leftEngine.GetGlobalThrust(delta);
			if (leftForce != Vector2.Zero)
				ApplyForce(leftForce, ToLocal(leftEngine.GlobalPosition));
		}

		if (rightEngine != null)
		{
			Vector2 rightForce = rightEngine.GetGlobalThrust(delta);
			if (rightForce != Vector2.Zero)
				ApplyForce(rightForce, ToLocal(rightEngine.GlobalPosition));
		}

		// simple lateral damping for the ship's body (optional)
		if (LinearVelocity.X > 0)
			ApplyForce(new Vector2(-thrustDecay, 0));
		else if (LinearVelocity.X < 0)
			ApplyForce(new Vector2(thrustDecay, 0));
	}
}
