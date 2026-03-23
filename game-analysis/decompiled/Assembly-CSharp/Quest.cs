using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Serialization;

// Token: 0x0200008B RID: 139
[Serializable]
public class Quest
{
	// Token: 0x060003D7 RID: 983 RVA: 0x00014FF4 File Offset: 0x000131F4
	public bool IsCompleted()
	{
		if (this._isCompleted)
		{
			return true;
		}
		int num = 0;
		using (List<QuestRequirement>.Enumerator enumerator = this.QuestRequirements.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsCompleted())
				{
					num++;
				}
			}
		}
		if (num == this.QuestRequirements.Count)
		{
			this._isCompleted = true;
			return true;
		}
		return false;
	}

	// Token: 0x060003D8 RID: 984 RVA: 0x00015070 File Offset: 0x00013270
	public bool TryActivateQuest()
	{
		return global::Singleton<QuestManager>.Instance.TryActivateQuest(this);
	}

	// Token: 0x060003D9 RID: 985 RVA: 0x0001507D File Offset: 0x0001327D
	public bool IsActive()
	{
		return global::Singleton<QuestManager>.Instance.ActiveQuests.Contains(this);
	}

	// Token: 0x060003DA RID: 986 RVA: 0x0001508F File Offset: 0x0001328F
	public void PauseQuest()
	{
		global::Singleton<QuestManager>.Instance.PauseQuest(this);
	}

	// Token: 0x060003DB RID: 987 RVA: 0x0001509C File Offset: 0x0001329C
	public bool IsLocked()
	{
		bool flag = this.PrerequisiteQuests.Count > 0;
		if (this.UnlockWhenAnyPrerequisitesAreComplete)
		{
			foreach (QuestDefinition questDefinition in this.PrerequisiteQuests)
			{
				Quest questByID = global::Singleton<QuestManager>.Instance.GetQuestByID(questDefinition.QuestID);
				if (questByID != null && questByID.IsCompleted())
				{
					return false;
				}
			}
			flag = true;
		}
		else
		{
			foreach (QuestDefinition questDefinition2 in this.PrerequisiteQuests)
			{
				Quest questByID2 = global::Singleton<QuestManager>.Instance.GetQuestByID(questDefinition2.QuestID);
				if (questByID2 != null && !questByID2.IsCompleted())
				{
					return true;
				}
			}
			flag = false;
		}
		return flag;
	}

	// Token: 0x060003DC RID: 988 RVA: 0x00015190 File Offset: 0x00013390
	public string GetRewardText()
	{
		string text = "";
		if (this.RewardMoney > 0f)
		{
			text = "Reward: ";
			text += string.Format("<color=#{0}>${1}</color>", global::Singleton<UIManager>.Instance.MoneyTextColor.ToHexString(), this.RewardMoney);
		}
		if (this.RewardResearchTickets > 0)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += " & ";
			}
			else
			{
				text = "Reward: ";
			}
			text += string.Format("<color=#{0}>{1} Research Ticket{2}</color>", global::Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), this.RewardResearchTickets, (this.RewardResearchTickets > 1) ? "s" : "");
		}
		if (this.ShopItemsToUnlock.Count > 0)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			string text2 = global::Singleton<QuestManager>.Instance.UnlockedItemRewardColor.ToHexString();
			string text3 = string.Join(", ", this.ShopItemsToUnlock.Select((ShopItemDefinition item) => item.GetName()));
			text = string.Concat(new string[] { text, "Unlocks: <color=#", text2, ">", text3, "</color>" });
		}
		if (!string.IsNullOrEmpty(this.OverrideRewardText))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += this.OverrideRewardText;
		}
		return text;
	}

	// Token: 0x060003DD RID: 989 RVA: 0x00015310 File Offset: 0x00013510
	public string GetRewardTextExcludingUnlocks()
	{
		string text = "";
		if (this.RewardMoney > 0f)
		{
			text += string.Format("<color=#{0}>${1}</color>", global::Singleton<UIManager>.Instance.MoneyTextColor.ToHexString(), this.RewardMoney);
		}
		if (this.RewardResearchTickets > 0)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += string.Format("<color=#{0}>{1} Research Ticket{2}</color>", global::Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), this.RewardResearchTickets, (this.RewardResearchTickets > 1) ? "s" : "");
		}
		if (!string.IsNullOrEmpty(this.OverrideRewardText))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += this.OverrideRewardText;
		}
		return text;
	}

	// Token: 0x060003DE RID: 990 RVA: 0x000153E7 File Offset: 0x000135E7
	public void DebugUnlock()
	{
		this._isCompleted = true;
	}

	// Token: 0x060003DF RID: 991 RVA: 0x000153F0 File Offset: 0x000135F0
	public void UnlockFromLoadingSaveFile()
	{
		this._isCompleted = true;
		foreach (ShopItemDefinition shopItemDefinition in this.ShopItemsToUnlock)
		{
			global::Singleton<EconomyManager>.Instance.UnlockShopItem(shopItemDefinition);
		}
		foreach (ShopItemDefinition shopItemDefinition2 in this.HiddenShopItemsToUnlock)
		{
			global::Singleton<EconomyManager>.Instance.UnlockShopItem(shopItemDefinition2);
		}
	}

	// Token: 0x04000425 RID: 1061
	public QuestID QuestID;

	// Token: 0x04000426 RID: 1062
	public string Name;

	// Token: 0x04000427 RID: 1063
	public string Description;

	// Token: 0x04000428 RID: 1064
	public float UIPriority = 100f;

	// Token: 0x04000429 RID: 1065
	public string OverrideRewardText;

	// Token: 0x0400042A RID: 1066
	public bool HideInQuestTree;

	// Token: 0x0400042B RID: 1067
	public bool UnlockWhenAnyPrerequisitesAreComplete;

	// Token: 0x0400042C RID: 1068
	public List<QuestDefinition> PrerequisiteQuests = new List<QuestDefinition>();

	// Token: 0x0400042D RID: 1069
	public List<QuestRequirement> QuestRequirements = new List<QuestRequirement>();

	// Token: 0x0400042E RID: 1070
	[FormerlySerializedAs("QuestsToUnlock")]
	public List<QuestDefinition> QuestsToAutoStart = new List<QuestDefinition>();

	// Token: 0x0400042F RID: 1071
	public List<ShopItemDefinition> ShopItemsToUnlock = new List<ShopItemDefinition>();

	// Token: 0x04000430 RID: 1072
	public List<ShopItemDefinition> HiddenShopItemsToUnlock = new List<ShopItemDefinition>();

	// Token: 0x04000431 RID: 1073
	public float RewardMoney;

	// Token: 0x04000432 RID: 1074
	public int RewardResearchTickets;

	// Token: 0x04000433 RID: 1075
	private bool _isCompleted;
}
