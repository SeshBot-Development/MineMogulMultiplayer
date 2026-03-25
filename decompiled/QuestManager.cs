using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class QuestManager : Singleton<QuestManager>
{
	public QuestDefinition StartingQuest;

	public List<Quest> ActiveQuests = new List<Quest>();

	public List<Quest> AllQuests = new List<Quest>();

	public Color UnlockedItemRewardColor;

	public event Action<Quest> QuestCompleted;

	public event Action<Quest> QuestActivated;

	public event Action<Quest> QuestPaused;

	private void OnEnable()
	{
		foreach (QuestDefinition allQuestDefinition in Singleton<SavingLoadingManager>.Instance.AllQuestDefinitions)
		{
			Quest item = allQuestDefinition.GenerateQuest();
			AllQuests.Add(item);
			if (allQuestDefinition == StartingQuest && !Singleton<GamemodeManager>.Instance.ShouldDisableQuests())
			{
				ActiveQuests.Add(item);
			}
		}
	}

	private void Update()
	{
		for (int num = ActiveQuests.Count - 1; num >= 0; num--)
		{
			Quest quest = ActiveQuests[num];
			if (quest.IsCompleted())
			{
				foreach (QuestDefinition item in quest.QuestsToAutoStart)
				{
					TryActivateQuest(GetQuestByID(item.QuestID));
				}
				foreach (ShopItemDefinition item2 in quest.ShopItemsToUnlock)
				{
					Singleton<EconomyManager>.Instance.UnlockShopItem(item2);
				}
				foreach (ShopItemDefinition item3 in quest.HiddenShopItemsToUnlock)
				{
					Singleton<EconomyManager>.Instance.UnlockShopItem(item3);
				}
				Singleton<EconomyManager>.Instance.AddMoney(quest.RewardMoney);
				Singleton<ResearchManager>.Instance.AddResearchTickets(quest.RewardResearchTickets);
				ActiveQuests.Remove(quest);
				this.QuestCompleted?.Invoke(quest);
			}
		}
	}

	public void ForceActivateQuest(QuestID questID)
	{
		Quest questByID = GetQuestByID(questID);
		if (!questByID.IsCompleted() && !ActiveQuests.Contains(questByID))
		{
			ActiveQuests.Add(questByID);
			this.QuestActivated?.Invoke(questByID);
		}
	}

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
		if (ActiveQuests.Contains(quest))
		{
			return false;
		}
		ActiveQuests.Add(quest);
		this.QuestActivated?.Invoke(quest);
		return true;
	}

	public void PauseQuest(Quest quest)
	{
		if (ActiveQuests.Contains(quest))
		{
			ActiveQuests.Remove(quest);
			this.QuestPaused?.Invoke(quest);
		}
	}

	public List<QuestID> GetCompletedQuestIDs()
	{
		return (from q in AllQuests
			where q.IsCompleted()
			select q.QuestID).ToList();
	}

	public List<QuestID> GetActiveQuestIDs()
	{
		return ActiveQuests.Select((Quest q) => q.QuestID).ToList();
	}

	public Quest GetQuestByID(QuestID questID)
	{
		return AllQuests.FirstOrDefault((Quest q) => q.QuestID == questID);
	}

	public List<ActiveQuestEntry> GetActiveQuestSaveEntries()
	{
		List<ActiveQuestEntry> list = new List<ActiveQuestEntry>();
		foreach (Quest activeQuest in ActiveQuests)
		{
			ActiveQuestEntry activeQuestEntry = new ActiveQuestEntry();
			activeQuestEntry.QuestID = activeQuest.QuestID;
			foreach (ResourceQuestRequirement item in activeQuest.QuestRequirements.OfType<ResourceQuestRequirement>())
			{
				ResourceQuestRequirementEntry resourceQuestRequirementEntry = new ResourceQuestRequirementEntry();
				resourceQuestRequirementEntry.ResourceType = item.ResourceType;
				resourceQuestRequirementEntry.PieceType = item.PieceType;
				resourceQuestRequirementEntry.RequirePolishedResource = item.RequirePolishedResource;
				resourceQuestRequirementEntry.CurrentAmount = item.CurrentAmount;
				activeQuestEntry.ResourceRequirements.Add(resourceQuestRequirementEntry);
			}
			list.Add(activeQuestEntry);
		}
		return list;
	}

	public void LoadFromSaveFile(SaveFile saveFile)
	{
		ActiveQuests.Clear();
		AllQuests.Clear();
		foreach (QuestDefinition questDef in Singleton<SavingLoadingManager>.Instance.AllQuestDefinitions)
		{
			Quest quest = questDef.GenerateQuest();
			AllQuests.Add(quest);
			if (saveFile.CompletedQuestsIDs.Contains(questDef.QuestID))
			{
				quest.UnlockFromLoadingSaveFile();
			}
			ActiveQuestEntry activeQuestEntry = saveFile.ActiveQuests.FirstOrDefault((ActiveQuestEntry aq) => aq.QuestID == questDef.QuestID);
			if (activeQuestEntry == null)
			{
				continue;
			}
			foreach (ResourceQuestRequirementEntry resourceRequirement in activeQuestEntry.ResourceRequirements)
			{
				foreach (ResourceQuestRequirement item in quest.QuestRequirements.OfType<ResourceQuestRequirement>())
				{
					if (item.ResourceType == resourceRequirement.ResourceType && item.PieceType == resourceRequirement.PieceType && item.RequirePolishedResource == resourceRequirement.RequirePolishedResource)
					{
						item.CurrentAmount = resourceRequirement.CurrentAmount;
						break;
					}
				}
			}
			ActiveQuests.Add(quest);
		}
		this.QuestCompleted?.Invoke(null);
	}

	public void DebugCompleteNextQuest()
	{
		if (ActiveQuests.Count > 0)
		{
			ActiveQuests.First().DebugUnlock();
		}
	}

	public List<Quest> GetAvailableQuests()
	{
		return AllQuests.Where((Quest q) => !q.HideInQuestTree && !q.IsLocked() && !q.IsCompleted() && !ActiveQuests.Contains(q)).ToList();
	}

	public void ActivateQuestTrigger(TriggeredQuestRequirementType type, int amount = 1)
	{
		foreach (Quest activeQuest in ActiveQuests)
		{
			foreach (QuestRequirement questRequirement in activeQuest.QuestRequirements)
			{
				if (questRequirement is TriggeredQuestRequirement triggeredQuestRequirement && triggeredQuestRequirement.TriggeredQuestRequirementType == type)
				{
					triggeredQuestRequirement.CurrentAmount += amount;
				}
			}
		}
	}

	public void TryGiveResearchTreeQuest()
	{
		if (Singleton<GamemodeManager>.Instance.GameModeType != GameModeType.Sandbox)
		{
			TryActivateQuest(GetQuestByID(QuestID.Open_ResearchTree));
		}
	}

	public void TryGiveInventoryQuest()
	{
		TryActivateQuest(GetQuestByID(QuestID.Open_Inventory));
	}

	public void OnResourceDeposited(ResourceType resourceType, PieceType pieceType, float polishedPercent, int amount)
	{
		foreach (Quest activeQuest in ActiveQuests)
		{
			foreach (QuestRequirement questRequirement in activeQuest.QuestRequirements)
			{
				if (amount <= 0)
				{
					return;
				}
				if (!(questRequirement is ResourceQuestRequirement resourceQuestRequirement) || resourceQuestRequirement.ResourceType != resourceType || resourceQuestRequirement.PieceType != pieceType)
				{
					continue;
				}
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
