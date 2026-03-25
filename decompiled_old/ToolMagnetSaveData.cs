using System;

[Serializable]
public class ToolMagnetSaveData
{
	public bool IsInPlayerInventory;

	public int InventorySlotIndex = -1;

	public MagnetToolSelectionMode SelectionMode;
}
