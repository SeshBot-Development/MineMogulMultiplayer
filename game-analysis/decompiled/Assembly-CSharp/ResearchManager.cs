using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000A4 RID: 164
[DefaultExecutionOrder(-90)]
public class ResearchManager : Singleton<ResearchManager>
{
	// Token: 0x1400000D RID: 13
	// (add) Token: 0x06000481 RID: 1153 RVA: 0x000188B4 File Offset: 0x00016AB4
	// (remove) Token: 0x06000482 RID: 1154 RVA: 0x000188EC File Offset: 0x00016AEC
	public event Action<int> ResearchTicketsUpdated;

	// Token: 0x1400000E RID: 14
	// (add) Token: 0x06000483 RID: 1155 RVA: 0x00018924 File Offset: 0x00016B24
	// (remove) Token: 0x06000484 RID: 1156 RVA: 0x0001895C File Offset: 0x00016B5C
	public event Action<ResearchItemDefinition> ResearchItemResearched;

	// Token: 0x06000485 RID: 1157 RVA: 0x00018994 File Offset: 0x00016B94
	public void ResearchItem(ResearchItemDefinition researchItem)
	{
		if (this.IsResearchItemCompleted(researchItem))
		{
			return;
		}
		if (!researchItem.CanAfford())
		{
			return;
		}
		if (researchItem.IsLocked())
		{
			return;
		}
		this.ResearchTickets -= researchItem.GetResearchTicketCost();
		Singleton<EconomyManager>.Instance.AddMoney(-researchItem.GetMoneyCost());
		this.CompletedResearchItems.Add(researchItem.GetSavableObjectID());
		researchItem.OnResearched();
		Action<ResearchItemDefinition> researchItemResearched = this.ResearchItemResearched;
		if (researchItemResearched == null)
		{
			return;
		}
		researchItemResearched(researchItem);
	}

	// Token: 0x06000486 RID: 1158 RVA: 0x00018A09 File Offset: 0x00016C09
	public bool IsResearchItemCompleted(ResearchItemDefinition researchItem)
	{
		return this.CompletedResearchItems.Contains(researchItem.GetSavableObjectID());
	}

	// Token: 0x1700001A RID: 26
	// (get) Token: 0x06000487 RID: 1159 RVA: 0x00018A1C File Offset: 0x00016C1C
	// (set) Token: 0x06000488 RID: 1160 RVA: 0x00018A24 File Offset: 0x00016C24
	public int ResearchTickets
	{
		get
		{
			return this._researchTickets;
		}
		private set
		{
			this._researchTickets = value;
			Action<int> researchTicketsUpdated = this.ResearchTicketsUpdated;
			if (researchTicketsUpdated == null)
			{
				return;
			}
			researchTicketsUpdated(this._researchTickets);
		}
	}

	// Token: 0x06000489 RID: 1161 RVA: 0x00018A44 File Offset: 0x00016C44
	public ResearchItemDefinition GetResearchItemByID(SavableObjectID id)
	{
		foreach (ResearchItemDefinition researchItemDefinition in Singleton<SavingLoadingManager>.Instance.AllResearchItemDefinitions)
		{
			if (researchItemDefinition.GetSavableObjectID() == id)
			{
				return researchItemDefinition;
			}
		}
		Debug.LogError("ResearchManager: GetResearchItemByID - No research item found with ID " + id.ToString());
		return null;
	}

	// Token: 0x0600048A RID: 1162 RVA: 0x00018AC0 File Offset: 0x00016CC0
	public void LoadFromSaveFile(List<SavableObjectID> completedResearchItems)
	{
		this.CompletedResearchItems = completedResearchItems;
		foreach (SavableObjectID savableObjectID in this.CompletedResearchItems)
		{
			ResearchItemDefinition researchItemByID = this.GetResearchItemByID(savableObjectID);
			if (researchItemByID != null)
			{
				researchItemByID.OnResearched();
			}
			else
			{
				Debug.LogError("ResearchManager: LoadFromSaveFile - No research item found with ID " + savableObjectID.ToString());
			}
		}
	}

	// Token: 0x0600048B RID: 1163 RVA: 0x00018B48 File Offset: 0x00016D48
	public void SetResearchTickets(int amount)
	{
		this.ResearchTickets = amount;
	}

	// Token: 0x0600048C RID: 1164 RVA: 0x00018B51 File Offset: 0x00016D51
	public void AddResearchTickets(int amount)
	{
		this.ResearchTickets += amount;
		if (amount > 0)
		{
			Singleton<QuestManager>.Instance.TryGiveResearchTreeQuest();
		}
	}

	// Token: 0x0600048D RID: 1165 RVA: 0x00018B6F File Offset: 0x00016D6F
	public bool CanAffordResearch(int amount)
	{
		return this.ResearchTickets >= amount;
	}

	// Token: 0x0600048E RID: 1166 RVA: 0x00018B80 File Offset: 0x00016D80
	public void MigrateNewResearchPrices()
	{
		int researchTickets = this._researchTickets;
		this._researchTickets = 0;
		foreach (QuestID questID in Singleton<QuestManager>.Instance.GetCompletedQuestIDs())
		{
			Quest questByID = Singleton<QuestManager>.Instance.GetQuestByID(questID);
			this._researchTickets += questByID.RewardResearchTickets;
		}
		foreach (SavableObjectID savableObjectID in this.CompletedResearchItems)
		{
			ResearchItemDefinition researchItemByID = this.GetResearchItemByID(savableObjectID);
			if (researchItemByID != null)
			{
				this._researchTickets -= researchItemByID.GetResearchTicketCost();
			}
		}
		if (this._researchTickets < 0)
		{
			this._researchTickets = 0;
		}
		Action<int> researchTicketsUpdated = this.ResearchTicketsUpdated;
		if (researchTicketsUpdated != null)
		{
			researchTicketsUpdated(this._researchTickets);
		}
		Debug.Log(string.Format("Migrated research prices. Old ticket amount: {0}, New: {1}", researchTickets, this._researchTickets));
	}

	// Token: 0x0400051A RID: 1306
	[SerializeField]
	private int _researchTickets;

	// Token: 0x0400051D RID: 1309
	public List<SavableObjectID> CompletedResearchItems = new List<SavableObjectID>();
}
