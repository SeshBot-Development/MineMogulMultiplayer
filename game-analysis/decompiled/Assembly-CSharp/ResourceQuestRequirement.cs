using System;
using UnityEngine;

// Token: 0x0200008D RID: 141
[Serializable]
public class ResourceQuestRequirement : QuestRequirement
{
	// Token: 0x060003E5 RID: 997 RVA: 0x000154F8 File Offset: 0x000136F8
	public override string GetRequirementText()
	{
		string text = "Deposit";
		if (!string.IsNullOrEmpty(this.OverrideDisplayName))
		{
			text = this.OverrideDisplayName;
		}
		return string.Format("{0} {1}: ({2}/{3})", new object[]
		{
			text,
			Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(this.ResourceType, this.PieceType, this.RequirePolishedResource),
			this.CurrentAmount,
			this.AmountRequired
		});
	}

	// Token: 0x060003E6 RID: 998 RVA: 0x0001556E File Offset: 0x0001376E
	public override bool IsCompleted()
	{
		return this.CurrentAmount >= this.AmountRequired;
	}

	// Token: 0x060003E7 RID: 999 RVA: 0x00015584 File Offset: 0x00013784
	public override QuestRequirement Clone()
	{
		return new ResourceQuestRequirement
		{
			ResourceType = this.ResourceType,
			OverrideDisplayName = this.OverrideDisplayName,
			PieceType = this.PieceType,
			RequirePolishedResource = this.RequirePolishedResource,
			AmountRequired = this.AmountRequired,
			IsHidden = this.IsHidden,
			UnlocksHiddenQuest = this.UnlocksHiddenQuest,
			CurrentAmount = 0
		};
	}

	// Token: 0x04000436 RID: 1078
	public ResourceType ResourceType;

	// Token: 0x04000437 RID: 1079
	public PieceType PieceType;

	// Token: 0x04000438 RID: 1080
	public bool RequirePolishedResource;

	// Token: 0x04000439 RID: 1081
	public int AmountRequired;

	// Token: 0x0400043A RID: 1082
	[HideInInspector]
	public int CurrentAmount;

	// Token: 0x0400043B RID: 1083
	public string OverrideDisplayName;
}
