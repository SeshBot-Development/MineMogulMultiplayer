using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class ResearchManager : Singleton<ResearchManager>
{
	[SerializeField]
	private int _researchTickets;

	public List<SavableObjectID> CompletedResearchItems = new List<SavableObjectID>();

	public int ResearchTickets
	{
		get
		{
			return _researchTickets;
		}
		private set
		{
			_researchTickets = value;
			this.ResearchTicketsUpdated?.Invoke(_researchTickets);
		}
	}

	public event Action<int> ResearchTicketsUpdated;

	public event Action<ResearchItemDefinition> ResearchItemResearched;

	public void ResearchItem(ResearchItemDefinition researchItem)
	{
		if (!IsResearchItemCompleted(researchItem) && researchItem.CanAfford() && !researchItem.IsLocked())
		{
			ResearchTickets -= researchItem.GetResearchTicketCost();
			Singleton<EconomyManager>.Instance.AddMoney(0f - researchItem.GetMoneyCost());
			CompletedResearchItems.Add(researchItem.GetSavableObjectID());
			researchItem.OnResearched();
			this.ResearchItemResearched?.Invoke(researchItem);
		}
	}

	public bool IsResearchItemCompleted(ResearchItemDefinition researchItem)
	{
		return CompletedResearchItems.Contains(researchItem.GetSavableObjectID());
	}

	public ResearchItemDefinition GetResearchItemByID(SavableObjectID id)
	{
		foreach (ResearchItemDefinition allResearchItemDefinition in Singleton<SavingLoadingManager>.Instance.AllResearchItemDefinitions)
		{
			if (allResearchItemDefinition.GetSavableObjectID() == id)
			{
				return allResearchItemDefinition;
			}
		}
		Debug.LogError("ResearchManager: GetResearchItemByID - No research item found with ID " + id);
		return null;
	}

	public void LoadFromSaveFile(List<SavableObjectID> completedResearchItems)
	{
		CompletedResearchItems = completedResearchItems;
		foreach (SavableObjectID completedResearchItem in CompletedResearchItems)
		{
			ResearchItemDefinition researchItemByID = GetResearchItemByID(completedResearchItem);
			if (researchItemByID != null)
			{
				researchItemByID.OnResearched();
			}
			else
			{
				Debug.LogError("ResearchManager: LoadFromSaveFile - No research item found with ID " + completedResearchItem);
			}
		}
	}

	public void SetResearchTickets(int amount)
	{
		ResearchTickets = amount;
	}

	public void AddResearchTickets(int amount)
	{
		ResearchTickets += amount;
		if (amount > 0)
		{
			Singleton<QuestManager>.Instance.TryGiveResearchTreeQuest();
		}
	}

	public bool CanAffordResearch(int amount)
	{
		return ResearchTickets >= amount;
	}

	public void MigrateNewResearchPrices()
	{
		int researchTickets = _researchTickets;
		_researchTickets = 0;
		foreach (QuestID completedQuestID in Singleton<QuestManager>.Instance.GetCompletedQuestIDs())
		{
			Quest questByID = Singleton<QuestManager>.Instance.GetQuestByID(completedQuestID);
			_researchTickets += questByID.RewardResearchTickets;
		}
		foreach (SavableObjectID completedResearchItem in CompletedResearchItems)
		{
			ResearchItemDefinition researchItemByID = GetResearchItemByID(completedResearchItem);
			if (researchItemByID != null)
			{
				_researchTickets -= researchItemByID.GetResearchTicketCost();
			}
		}
		if (_researchTickets < 0)
		{
			_researchTickets = 0;
		}
		this.ResearchTicketsUpdated?.Invoke(_researchTickets);
		Debug.Log($"Migrated research prices. Old ticket amount: {researchTickets}, New: {_researchTickets}");
	}
}
