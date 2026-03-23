using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000098 RID: 152
[DefaultExecutionOrder(-100)]
public class QuestManager : Singleton<QuestManager>
{
	// Token: 0x1400000A RID: 10
	// (add) Token: 0x06000409 RID: 1033 RVA: 0x00015F9C File Offset: 0x0001419C
	// (remove) Token: 0x0600040A RID: 1034 RVA: 0x00015FD4 File Offset: 0x000141D4
	public event Action<Quest> QuestCompleted;

	// Token: 0x1400000B RID: 11
	// (add) Token: 0x0600040B RID: 1035 RVA: 0x0001600C File Offset: 0x0001420C
	// (remove) Token: 0x0600040C RID: 1036 RVA: 0x00016044 File Offset: 0x00014244
	public event Action<Quest> QuestActivated;

	// Token: 0x1400000C RID: 12
	// (add) Token: 0x0600040D RID: 1037 RVA: 0x0001607C File Offset: 0x0001427C
	// (remove) Token: 0x0600040E RID: 1038 RVA: 0x000160B4 File Offset: 0x000142B4
	public event Action<Quest> QuestPaused;

	// Token: 0x0600040F RID: 1039 RVA: 0x000160EC File Offset: 0x000142EC
	private void OnEnable()
	{
		foreach (QuestDefinition questDefinition in Singleton<SavingLoadingManager>.Instance.AllQuestDefinitions)
		{
			Quest quest = questDefinition.GenerateQuest();
			this.AllQuests.Add(quest);
			if (questDefinition == this.StartingQuest)
			{
				this.ActiveQuests.Add(quest);
			}
		}
	}

	// Token: 0x06000410 RID: 1040 RVA: 0x00016168 File Offset: 0x00014368
	private void Update()
	{
		for (int i = this.ActiveQuests.Count - 1; i >= 0; i--)
		{
			Quest quest = this.ActiveQuests[i];
			if (quest.IsCompleted())
			{
				foreach (QuestDefinition questDefinition in quest.QuestsToAutoStart)
				{
					this.TryActivateQuest(this.GetQuestByID(questDefinition.QuestID));
				}
				foreach (ShopItemDefinition shopItemDefinition in quest.ShopItemsToUnlock)
				{
					Singleton<EconomyManager>.Instance.UnlockShopItem(shopItemDefinition);
				}
				foreach (ShopItemDefinition shopItemDefinition2 in quest.HiddenShopItemsToUnlock)
				{
					Singleton<EconomyManager>.Instance.UnlockShopItem(shopItemDefinition2);
				}
				Singleton<EconomyManager>.Instance.AddMoney(quest.RewardMoney);
				Singleton<ResearchManager>.Instance.AddResearchTickets(quest.RewardResearchTickets);
				this.ActiveQuests.Remove(quest);
				Action<Quest> questCompleted = this.QuestCompleted;
				if (questCompleted != null)
				{
					questCompleted(quest);
				}
			}
		}
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x000162D0 File Offset: 0x000144D0
	public void ForceActivateQuest(QuestID questID)
	{
		Quest questByID = this.GetQuestByID(questID);
		if (questByID.IsCompleted())
		{
			return;
		}
		if (this.ActiveQuests.Contains(questByID))
		{
			return;
		}
		this.ActiveQuests.Add(questByID);
		Action<Quest> questActivated = this.QuestActivated;
		if (questActivated == null)
		{
			return;
		}
		questActivated(questByID);
	}

	// Token: 0x06000412 RID: 1042 RVA: 0x0001631C File Offset: 0x0001451C
	public bool TryActivateQuest(Quest quest)
	{
		if (quest.IsLocked())
		{
			return false;
		}
		if (quest.IsCompleted())
		{
			return false;
		}
		if (this.ActiveQuests.Contains(quest))
		{
			return false;
		}
		this.ActiveQuests.Add(quest);
		Action<Quest> questActivated = this.QuestActivated;
		if (questActivated != null)
		{
			questActivated(quest);
		}
		return true;
	}

	// Token: 0x06000413 RID: 1043 RVA: 0x0001636C File Offset: 0x0001456C
	public void PauseQuest(Quest quest)
	{
		if (this.ActiveQuests.Contains(quest))
		{
			this.ActiveQuests.Remove(quest);
			Action<Quest> questPaused = this.QuestPaused;
			if (questPaused == null)
			{
				return;
			}
			questPaused(quest);
		}
	}

	// Token: 0x06000414 RID: 1044 RVA: 0x0001639C File Offset: 0x0001459C
	public List<QuestID> GetCompletedQuestIDs()
	{
		return (from q in this.AllQuests
			where q.IsCompleted()
			select q.QuestID).ToList<QuestID>();
	}

	// Token: 0x06000415 RID: 1045 RVA: 0x000163FC File Offset: 0x000145FC
	public List<QuestID> GetActiveQuestIDs()
	{
		return this.ActiveQuests.Select((Quest q) => q.QuestID).ToList<QuestID>();
	}

	// Token: 0x06000416 RID: 1046 RVA: 0x00016430 File Offset: 0x00014630
	public Quest GetQuestByID(QuestID questID)
	{
		return this.AllQuests.FirstOrDefault((Quest q) => q.QuestID == questID);
	}

	// Token: 0x06000417 RID: 1047 RVA: 0x00016464 File Offset: 0x00014664
	public List<ActiveQuestEntry> GetActiveQuestSaveEntries()
	{
		List<ActiveQuestEntry> list = new List<ActiveQuestEntry>();
		foreach (Quest quest in this.ActiveQuests)
		{
			ActiveQuestEntry activeQuestEntry = new ActiveQuestEntry();
			activeQuestEntry.QuestID = quest.QuestID;
			foreach (ResourceQuestRequirement resourceQuestRequirement in quest.QuestRequirements.OfType<ResourceQuestRequirement>())
			{
				ResourceQuestRequirementEntry resourceQuestRequirementEntry = new ResourceQuestRequirementEntry();
				resourceQuestRequirementEntry.ResourceType = resourceQuestRequirement.ResourceType;
				resourceQuestRequirementEntry.PieceType = resourceQuestRequirement.PieceType;
				resourceQuestRequirementEntry.RequirePolishedResource = resourceQuestRequirement.RequirePolishedResource;
				resourceQuestRequirementEntry.CurrentAmount = resourceQuestRequirement.CurrentAmount;
				activeQuestEntry.ResourceRequirements.Add(resourceQuestRequirementEntry);
			}
			list.Add(activeQuestEntry);
		}
		return list;
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x00016564 File Offset: 0x00014764
	public void LoadFromSaveFile(SaveFile saveFile)
	{
		this.ActiveQuests.Clear();
		this.AllQuests.Clear();
		using (List<QuestDefinition>.Enumerator enumerator = Singleton<SavingLoadingManager>.Instance.AllQuestDefinitions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				QuestDefinition questDef = enumerator.Current;
				Quest quest = questDef.GenerateQuest();
				this.AllQuests.Add(quest);
				if (saveFile.CompletedQuestsIDs.Contains(questDef.QuestID))
				{
					quest.UnlockFromLoadingSaveFile();
				}
				ActiveQuestEntry activeQuestEntry = saveFile.ActiveQuests.FirstOrDefault((ActiveQuestEntry aq) => aq.QuestID == questDef.QuestID);
				if (activeQuestEntry != null)
				{
					foreach (ResourceQuestRequirementEntry resourceQuestRequirementEntry in activeQuestEntry.ResourceRequirements)
					{
						foreach (ResourceQuestRequirement resourceQuestRequirement in quest.QuestRequirements.OfType<ResourceQuestRequirement>())
						{
							if (resourceQuestRequirement.ResourceType == resourceQuestRequirementEntry.ResourceType && resourceQuestRequirement.PieceType == resourceQuestRequirementEntry.PieceType && resourceQuestRequirement.RequirePolishedResource == resourceQuestRequirementEntry.RequirePolishedResource)
							{
								resourceQuestRequirement.CurrentAmount = resourceQuestRequirementEntry.CurrentAmount;
								break;
							}
						}
					}
					this.ActiveQuests.Add(quest);
				}
			}
		}
		Action<Quest> questCompleted = this.QuestCompleted;
		if (questCompleted == null)
		{
			return;
		}
		questCompleted(null);
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x00016730 File Offset: 0x00014930
	public void DebugCompleteNextQuest()
	{
		if (this.ActiveQuests.Count > 0)
		{
			this.ActiveQuests.First<Quest>().DebugUnlock();
		}
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x00016750 File Offset: 0x00014950
	public List<Quest> GetAvailableQuests()
	{
		return this.AllQuests.Where((Quest q) => !q.HideInQuestTree && !q.IsLocked() && !q.IsCompleted() && !this.ActiveQuests.Contains(q)).ToList<Quest>();
	}

	// Token: 0x0600041B RID: 1051 RVA: 0x00016770 File Offset: 0x00014970
	public void ActivateQuestTrigger(TriggeredQuestRequirementType type, int amount = 1)
	{
		foreach (Quest quest in this.ActiveQuests)
		{
			foreach (QuestRequirement questRequirement in quest.QuestRequirements)
			{
				TriggeredQuestRequirement triggeredQuestRequirement = questRequirement as TriggeredQuestRequirement;
				if (triggeredQuestRequirement != null && triggeredQuestRequirement.TriggeredQuestRequirementType == type)
				{
					triggeredQuestRequirement.CurrentAmount += amount;
				}
			}
		}
	}

	// Token: 0x0600041C RID: 1052 RVA: 0x00016814 File Offset: 0x00014A14
	public void TryGiveResearchTreeQuest()
	{
		this.TryActivateQuest(this.GetQuestByID(QuestID.Open_ResearchTree));
	}

	// Token: 0x0600041D RID: 1053 RVA: 0x00016828 File Offset: 0x00014A28
	public void TryGiveInventoryQuest()
	{
		this.TryActivateQuest(this.GetQuestByID(QuestID.Open_Inventory));
	}

	// Token: 0x0600041E RID: 1054 RVA: 0x0001683C File Offset: 0x00014A3C
	public void OnResourceDeposited(ResourceType resourceType, PieceType pieceType, float polishedPercent, int amount)
	{
		foreach (Quest quest in this.ActiveQuests)
		{
			foreach (QuestRequirement questRequirement in quest.QuestRequirements)
			{
				if (amount <= 0)
				{
					return;
				}
				ResourceQuestRequirement resourceQuestRequirement = questRequirement as ResourceQuestRequirement;
				if (resourceQuestRequirement != null && resourceQuestRequirement.ResourceType == resourceType && resourceQuestRequirement.PieceType == pieceType)
				{
					if (resourceQuestRequirement.RequirePolishedResource)
					{
						if (polishedPercent >= 1f)
						{
							resourceQuestRequirement.CurrentAmount += amount;
						}
					}
					else
					{
						resourceQuestRequirement.CurrentAmount += amount;
					}
				}
			}
		}
	}

	// Token: 0x040004B0 RID: 1200
	public QuestDefinition StartingQuest;

	// Token: 0x040004B1 RID: 1201
	public List<Quest> ActiveQuests = new List<Quest>();

	// Token: 0x040004B2 RID: 1202
	public List<Quest> AllQuests = new List<Quest>();

	// Token: 0x040004B3 RID: 1203
	public Color UnlockedItemRewardColor;
}
