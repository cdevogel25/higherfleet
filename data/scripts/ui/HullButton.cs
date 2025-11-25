using Godot;
using System;
using System.Collections.Generic;

public partial class HullButton : Control
{
	private Component_Structural hull;
	private PackedScene hullScene;
	private int hullIndex = 0;

	public override void _Ready()
	{
		hullScene = GD.Load<PackedScene>("res://data/gameObjects/builder/hull.tscn");
	}
	private void OnPressed()
	{
		hull = hullScene.Instantiate<Component_Structural>();
		hull.Name = "Hull_" + hullIndex++;
		GetTree().Root.GetNode<Component_Bridge>("Node2D/RootHull").AddChild(hull);
		GD.Print(hull.Name + " created.");
	}
}
