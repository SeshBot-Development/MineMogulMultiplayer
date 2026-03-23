using System;
using System.Collections.Generic;

// Token: 0x020000C7 RID: 199
[Serializable]
public class ActiveQuestEntry
{
	// Token: 0x04000697 RID: 1687
	public QuestID QuestID;

	// Token: 0x04000698 RID: 1688
	public List<ResourceQuestRequirementEntry> ResourceRequirements = new List<ResourceQuestRequirementEntry>();
}
