using System;

// Token: 0x02000090 RID: 144
[Serializable]
public class UnlockResearchQuestRequirement : QuestRequirement
{
	// Token: 0x060003F1 RID: 1009 RVA: 0x0001576B File Offset: 0x0001396B
	public override string GetRequirementText()
	{
		return "Unlock " + this.ResearchItemDefinition.GetName() + " in the Research Tree";
	}

	// Token: 0x060003F2 RID: 1010 RVA: 0x00015787 File Offset: 0x00013987
	public override bool IsCompleted()
	{
		return this.ResearchItemDefinition.IsResearched();
	}

	// Token: 0x060003F3 RID: 1011 RVA: 0x00015794 File Offset: 0x00013994
	public override QuestRequirement Clone()
	{
		return new UnlockResearchQuestRequirement
		{
			ResearchItemDefinition = this.ResearchItemDefinition,
			IsHidden = this.IsHidden,
			UnlocksHiddenQuest = this.UnlocksHiddenQuest
		};
	}

	// Token: 0x04000442 RID: 1090
	public ResearchItemDefinition ResearchItemDefinition;
}
