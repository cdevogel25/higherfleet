using Godot;

public partial class RootHull : BuilderObject_Placeable
{
	// Every other BuilderObject_Placeable will connect to this one in the scene tree, and should be reparented to it when placed.
	// (and removed as its child when being dragged). It also serves as the central node of ShipGraph which contains all connected parts,
	// their neighbors, and their offset from root in order to maintain their positions when the ship moves.
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
