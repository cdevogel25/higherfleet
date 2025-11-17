using Godot;
using System;
using System.Collections.Generic;

public partial class HullButton : Control
{
	private Hull hull;
	private PackedScene hullScene;
	private bool _isDragging = false;
	private int hullIndex = 0;

	public override void _Ready()
	{
		hullScene = GD.Load<PackedScene>("res://data/gameObjects/builder/hull.tscn");
	}
	private void OnPressed()
	{
		hull = hullScene.Instantiate<Hull>();
		hull.Name = "Hull_" + hullIndex++;
		GetTree().Root.AddChild(hull);
		GD.Print(hull.Name + " created.");
	}
}
