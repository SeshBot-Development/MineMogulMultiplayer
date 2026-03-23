using System;

// Token: 0x0200008C RID: 140
[Serializable]
public abstract class QuestRequirement
{
	// Token: 0x060003E1 RID: 993 RVA: 0x000154E9 File Offset: 0x000136E9
	public virtual string GetRequirementText()
	{
		return "invalid requirement";
	}

	// Token: 0x060003E2 RID: 994
	public abstract bool IsCompleted();

	// Token: 0x060003E3 RID: 995
	public abstract QuestRequirement Clone();

	// Token: 0x04000434 RID: 1076
	public bool IsHidden;

	// Token: 0x04000435 RID: 1077
	public bool UnlocksHiddenQuest;
}
