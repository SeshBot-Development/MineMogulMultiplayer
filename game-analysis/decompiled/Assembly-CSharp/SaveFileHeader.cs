using System;

// Token: 0x020000C6 RID: 198
[Serializable]
public class SaveFileHeader
{
	// Token: 0x0400068F RID: 1679
	public int SaveVersion;

	// Token: 0x04000690 RID: 1680
	public string GameVersion = "Unknown";

	// Token: 0x04000691 RID: 1681
	public string SaveFileName = "Unnamed Save File";

	// Token: 0x04000692 RID: 1682
	public string SaveTimestamp = "Unknown Time";

	// Token: 0x04000693 RID: 1683
	public string LevelID = "DemoCave";

	// Token: 0x04000694 RID: 1684
	public float Money;

	// Token: 0x04000695 RID: 1685
	public int ResearchTickets;

	// Token: 0x04000696 RID: 1686
	public double TotalPlayTimeSeconds;
}
