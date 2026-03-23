using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C5 RID: 197
[Serializable]
public class SaveFile
{
	// Token: 0x0400067B RID: 1659
	public int SaveVersion;

	// Token: 0x0400067C RID: 1660
	public string GameVersion = "Unknown";

	// Token: 0x0400067D RID: 1661
	public string SaveFileName = "Unnamed Save File";

	// Token: 0x0400067E RID: 1662
	public string SaveTimestamp = "Unknown Time";

	// Token: 0x0400067F RID: 1663
	public string LevelID = "DemoCave";

	// Token: 0x04000680 RID: 1664
	public float Money;

	// Token: 0x04000681 RID: 1665
	public int ResearchTickets;

	// Token: 0x04000682 RID: 1666
	public double TotalPlayTimeSeconds;

	// Token: 0x04000683 RID: 1667
	public Vector3 PlayerPosition = Vector3.zero;

	// Token: 0x04000684 RID: 1668
	public Vector3 PlayerRotation = Vector3.zero;

	// Token: 0x04000685 RID: 1669
	public List<SaveEntry> Entries = new List<SaveEntry>();

	// Token: 0x04000686 RID: 1670
	public List<BuildingObjectEntry> BuildingObjects = new List<BuildingObjectEntry>();

	// Token: 0x04000687 RID: 1671
	public List<Vector3> DestroyedStaticBreakablePositions = new List<Vector3>();

	// Token: 0x04000688 RID: 1672
	public List<QuestID> CompletedQuestsIDs = new List<QuestID>();

	// Token: 0x04000689 RID: 1673
	public List<ActiveQuestEntry> ActiveQuests = new List<ActiveQuestEntry>();

	// Token: 0x0400068A RID: 1674
	public List<OrePieceEntry> OrePieces = new List<OrePieceEntry>();

	// Token: 0x0400068B RID: 1675
	public List<WorldEventEntry> WorldEventEntries = new List<WorldEventEntry>();

	// Token: 0x0400068C RID: 1676
	public ShopPurchases ShopPurchases = new ShopPurchases();

	// Token: 0x0400068D RID: 1677
	public List<SavableObjectID> CompletedResearchItems = new List<SavableObjectID>();

	// Token: 0x0400068E RID: 1678
	public bool HasShownOreLimitPopup;
}
