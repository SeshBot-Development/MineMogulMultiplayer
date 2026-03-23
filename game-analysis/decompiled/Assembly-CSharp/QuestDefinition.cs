using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000094 RID: 148
[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/New Quest")]
public class QuestDefinition : ScriptableObject
{
	// Token: 0x060003F9 RID: 1017 RVA: 0x000158D0 File Offset: 0x00013AD0
	public Quest GenerateQuest()
	{
		Quest quest = new Quest();
		quest.QuestID = this.QuestID;
		quest.Name = this.Name;
		quest.Description = this.Description;
		quest.RewardMoney = this.RewardMoney;
		quest.RewardResearchTickets = this.RewardResearchTickets;
		quest.UIPriority = this.UIPriority;
		quest.OverrideRewardText = this.OverrideRewardText;
		quest.HideInQuestTree = this.HideInQuestTree;
		foreach (UnlockResearchQuestRequirement unlockResearchQuestRequirement in this.UnlockResearchQuestRequirements)
		{
			quest.QuestRequirements.Add(unlockResearchQuestRequirement.Clone());
		}
		foreach (ShopItemQuestRequirement shopItemQuestRequirement in this.ShopItemQuestRequirements)
		{
			quest.QuestRequirements.Add(shopItemQuestRequirement.Clone());
		}
		foreach (TimedQuestRequirement timedQuestRequirement in this.TimedRequirements)
		{
			quest.QuestRequirements.Add(timedQuestRequirement.Clone());
		}
		foreach (TriggeredQuestRequirement triggeredQuestRequirement in this.TriggeredRequirements)
		{
			quest.QuestRequirements.Add(triggeredQuestRequirement.Clone());
		}
		foreach (ResourceQuestRequirement resourceQuestRequirement in this.ResourceRequirements)
		{
			quest.QuestRequirements.Add(resourceQuestRequirement.Clone());
		}
		quest.UnlockWhenAnyPrerequisitesAreComplete = this.UnlockWhenAnyPrerequisitesAreComplete;
		quest.PrerequisiteQuests = this.PrerequisiteQuests;
		quest.QuestsToAutoStart = this.QuestsToAutoStart;
		quest.ShopItemsToUnlock = this.ShopItemsToUnlock;
		quest.HiddenShopItemsToUnlock = this.HiddenShopItemsToUnlock;
		return quest;
	}

	// Token: 0x060003FA RID: 1018 RVA: 0x00015B0C File Offset: 0x00013D0C
	public Sprite GetOverrideIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(this.OverrideQuestProgrammerIcon != null))
			{
				return this.OverrideQuestIcon;
			}
			return this.OverrideQuestProgrammerIcon;
		}
		else
		{
			if (!(this.OverrideQuestIcon != null))
			{
				return this.OverrideQuestProgrammerIcon;
			}
			return this.OverrideQuestIcon;
		}
	}

	// Token: 0x04000452 RID: 1106
	public QuestID QuestID;

	// Token: 0x04000453 RID: 1107
	public string Name;

	// Token: 0x04000454 RID: 1108
	[TextArea]
	public string Description;

	// Token: 0x04000455 RID: 1109
	public float UIPriority = 100f;

	// Token: 0x04000456 RID: 1110
	public string OverrideRewardText;

	// Token: 0x04000457 RID: 1111
	public Sprite OverrideQuestIcon;

	// Token: 0x04000458 RID: 1112
	public Sprite OverrideQuestProgrammerIcon;

	// Token: 0x04000459 RID: 1113
	public bool HideInQuestTree;

	// Token: 0x0400045A RID: 1114
	[Tooltip("OFF = requires all prerequisites")]
	public bool UnlockWhenAnyPrerequisitesAreComplete;

	// Token: 0x0400045B RID: 1115
	public List<QuestDefinition> PrerequisiteQuests = new List<QuestDefinition>();

	// Token: 0x0400045C RID: 1116
	public List<UnlockResearchQuestRequirement> UnlockResearchQuestRequirements = new List<UnlockResearchQuestRequirement>();

	// Token: 0x0400045D RID: 1117
	public List<ShopItemQuestRequirement> ShopItemQuestRequirements = new List<ShopItemQuestRequirement>();

	// Token: 0x0400045E RID: 1118
	public List<TimedQuestRequirement> TimedRequirements = new List<TimedQuestRequirement>();

	// Token: 0x0400045F RID: 1119
	public List<TriggeredQuestRequirement> TriggeredRequirements = new List<TriggeredQuestRequirement>();

	// Token: 0x04000460 RID: 1120
	public List<ResourceQuestRequirement> ResourceRequirements = new List<ResourceQuestRequirement>();

	// Token: 0x04000461 RID: 1121
	[FormerlySerializedAs("QuestsToUnlock")]
	public List<QuestDefinition> QuestsToAutoStart = new List<QuestDefinition>();

	// Token: 0x04000462 RID: 1122
	public List<ShopItemDefinition> ShopItemsToUnlock = new List<ShopItemDefinition>();

	// Token: 0x04000463 RID: 1123
	public List<ShopItemDefinition> HiddenShopItemsToUnlock = new List<ShopItemDefinition>();

	// Token: 0x04000464 RID: 1124
	public float RewardMoney;

	// Token: 0x04000465 RID: 1125
	public int RewardResearchTickets;
}
