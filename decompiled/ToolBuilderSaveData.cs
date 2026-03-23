using System;

[Serializable]
public class ToolBuilderSaveData
{
	public bool IsInPlayerInventory;

	public int InventorySlotIndex = -1;

	public int Quantity = 1;

	public SavableObjectID BuildObjectID;
}
