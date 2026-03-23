using System;
using UnityEngine;

// Token: 0x0200008E RID: 142
[Serializable]
public class TriggeredQuestRequirement : QuestRequirement
{
	// Token: 0x060003E9 RID: 1001 RVA: 0x000155FC File Offset: 0x000137FC
	public override string GetRequirementText()
	{
		string text;
		if (this.AmountRequired == 1)
		{
			text = this.RequirementName;
		}
		else
		{
			text = string.Format("{0}: ({1}/{2})", this.RequirementName, this.CurrentAmount, this.AmountRequired);
		}
		return Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(text);
	}

	// Token: 0x060003EA RID: 1002 RVA: 0x00015653 File Offset: 0x00013853
	public override bool IsCompleted()
	{
		return this.CurrentAmount >= this.AmountRequired;
	}

	// Token: 0x060003EB RID: 1003 RVA: 0x00015668 File Offset: 0x00013868
	public override QuestRequirement Clone()
	{
		return new TriggeredQuestRequirement
		{
			TriggeredQuestRequirementType = this.TriggeredQuestRequirementType,
			RequirementName = this.RequirementName,
			AmountRequired = this.AmountRequired,
			IsHidden = this.IsHidden,
			UnlocksHiddenQuest = this.UnlocksHiddenQuest,
			CurrentAmount = 0
		};
	}

	// Token: 0x0400043C RID: 1084
	public TriggeredQuestRequirementType TriggeredQuestRequirementType;

	// Token: 0x0400043D RID: 1085
	public string RequirementName;

	// Token: 0x0400043E RID: 1086
	public int AmountRequired;

	// Token: 0x0400043F RID: 1087
	[HideInInspector]
	public int CurrentAmount;
}
