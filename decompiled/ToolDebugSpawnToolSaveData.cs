using System;

[Serializable]
public class ToolDebugSpawnToolSaveData
{
	public bool IsInPlayerInventory;

	public int InventorySlotIndex = -1;

	public PieceType OrePieceType = PieceType.Ore;

	public ResourceType OreResourceType = ResourceType.Iron;

	public bool OreIsPolished;
}
