using Godot;

public partial class SnapPoint_Internal : Marker2D
{
	// marker for external snap points (ones that can connect to other components)
	public bool IsOccupied;
	public bool isInternal;
	public bool isExternal;

	public override void _Ready()
	{
		isInternal = true;
		isExternal = false;
		IsOccupied = false;
	}
	public void SetIsOccupied()
	{
		IsOccupied = true;
	}

	public void SetIsUnoccupied()
	{
		IsOccupied = false;
	}
}
