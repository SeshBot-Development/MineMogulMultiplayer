using System;

// Token: 0x020000EB RID: 235
[Serializable]
public class ToolBuilderSaveData
{
	// Token: 0x04000788 RID: 1928
	public bool IsInPlayerInventory;

	// Token: 0x04000789 RID: 1929
	public int InventorySlotIndex = -1;

	// Token: 0x0400078A RID: 1930
	public int Quantity = 1;

	// Token: 0x0400078B RID: 1931
	public SavableObjectID BuildObjectID;
}
