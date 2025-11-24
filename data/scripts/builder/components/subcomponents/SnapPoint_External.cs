using Godot;

public partial class SnapPoint_External : Area2D
{
	// marker for external snap points (ones that can connect to other components)
	public bool IsOccupied;
	public bool isInternal;
	public bool isExternal;

	public override void _Ready()
	{
		isInternal = false;
		isExternal = true;
		IsOccupied = false;
	}
	public void SetIsOccupied()
	{
		GD.Print("Snap point " + Name + " of " + GetParent<Component>().Name + " is now occupied.");
		IsOccupied = true;
	}
	
	public void SetIsUnoccupied()
	{
		GD.Print("Snap point " + Name + " of " + GetParent<Component>().Name + " is now unoccupied.");
		IsOccupied = false;
	}
}
