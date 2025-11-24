using Godot;

public partial class HullButton : Control
{
	private Component_Structural hull;
	private PackedScene hullScene;
	private bool _isDragging = false;
	private int hullIndex = 0;

	public override void _Ready()
	{
		hullScene = GD.Load<PackedScene>("res://data/gameObjects/builder/hull.tscn");
	}
	private void OnPressed()
	{
		hull = hullScene.Instantiate<Component_Structural>();
		hull.Name = "Hull_" + hullIndex++;
		GetTree().CurrentScene.AddChild(hull);
		GD.Print(hull.Name + " created.");
	}
}
