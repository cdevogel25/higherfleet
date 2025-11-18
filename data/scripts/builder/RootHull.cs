using Godot;

public partial class RootHull : BuilderObject_Placeable
{
	public override void _Ready()
	{
		// this is the root builder object (the bridge)
		IsRoot = true;

		foreach (Node2D c in GetChildren())
		{
			if (c is Marker2D marker && marker.Name.ToString().StartsWith("Snap"))
			{
				SnapPoints.Add(marker);
			}
		}
	}
}
