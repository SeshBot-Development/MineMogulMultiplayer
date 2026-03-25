using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Serialization;

[Serializable]
public class Quest
{
	public QuestID QuestID;

	public string Name;

	public string Description;

	public float UIPriority = 100f;

	public string OverrideRewardText;

	public bool HideInQuestTree;

	public bool UnlockWhenAnyPrerequisitesAreComplete;

	public List<QuestDefinition> PrerequisiteQuests = new List<QuestDefinition>();

	public List<QuestRequirement> QuestRequirements = new List<QuestRequirement>();

	[FormerlySerializedAs("QuestsToUnlock")]
	public List<QuestDefinition> QuestsToAutoStart = new List<QuestDefinition>();

	public List<ShopItemDefinition> ShopItemsToUnlock = new List<ShopItemDefinition>();

	public List<ShopItemDefinition> HiddenShopItemsToUnlock = new List<ShopItemDefinition>();

	public float RewardMoney;

	public int RewardResearchTickets;

	private bool _isCompleted;

	public bool IsCompleted()
	{
		if (_isCompleted)
		{
			return true;
		}
		int num = 0;
		foreach (QuestRequirement questRequirement in QuestRequirements)
		{
			if (questRequirement.IsCompleted())
			{
				num++;
			}
		}
		if (num == QuestRequirements.Count)
		{
			_isCompleted = true;
			return true;
		}
		return false;
	}

	public bool TryActivateQuest()
	{
		return Singleton<QuestManager>.Instance.TryActivateQuest(this);
	}

	public bool IsActive()
	{
		return Singleton<QuestManager>.Instance.ActiveQuests.Contains(this);
	}

	public void PauseQuest()
	{
		Singleton<QuestManager>.Instance.PauseQuest(this);
	}

	public bool IsLocked()
	{
		bool flag = PrerequisiteQuests.Count > 0;
		if (UnlockWhenAnyPrerequisitesAreComplete)
		{
			foreach (QuestDefinition prerequisiteQuest in PrerequisiteQuests)
			{
				Quest questByID = Singleton<QuestManager>.Instance.GetQuestByID(prerequisiteQuest.QuestID);
				if (questByID != null && questByID.IsCompleted())
				{
					return false;
				}
			}
			return true;
		}
		foreach (QuestDefinition prerequisiteQuest2 in PrerequisiteQuests)
		{
			Quest questByID2 = Singleton<QuestManager>.Instance.GetQuestByID(prerequisiteQuest2.QuestID);
			if (questByID2 != null && !questByID2.IsCompleted())
			{
				return true;
			}
		}
		return false;
	}

	public string GetRewardText()
	{
		string text = "";
		if (RewardMoney > 0f)
		{
			text = "Reward: ";
			text += $"<color=#{Singleton<UIManager>.Instance.MoneyTextColor.ToHexString()}>${RewardMoney}</color>";
		}
		if (RewardResearchTickets > 0)
		{
			text = (string.IsNullOrEmpty(text) ? "Reward: " : (text + " & "));
			text += string.Format("<color=#{0}>{1} Research Ticket{2}</color>", Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), RewardResearchTickets, (RewardResearchTickets > 1) ? "s" : "");
		}
		if (ShopItemsToUnlock.Count > 0)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			string text2 = Singleton<QuestManager>.Instance.UnlockedItemRewardColor.ToHexString();
			string text3 = string.Join(", ", ShopItemsToUnlock.Select((ShopItemDefinition item) => item.GetName()));
			text = text + "Unlocks: <color=#" + text2 + ">" + text3 + "</color>";
		}
		if (!string.IsNullOrEmpty(OverrideRewardText))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += OverrideRewardText;
		}
		return text;
	}

	public string GetRewardTextExcludingUnlocks()
	{
		string text = "";
		if (RewardMoney > 0f)
		{
			text += $"<color=#{Singleton<UIManager>.Instance.MoneyTextColor.ToHexString()}>${RewardMoney}</color>";
		}
		if (RewardResearchTickets > 0)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += string.Format("<color=#{0}>{1} Research Ticket{2}</color>", Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), RewardResearchTickets, (RewardResearchTickets > 1) ? "s" : "");
		}
		if (!string.IsNullOrEmpty(OverrideRewardText))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += OverrideRewardText;
		}
		return text;
	}

	public void DebugUnlock()
	{
		_isCompleted = true;
	}

	public void UnlockFromLoadingSaveFile()
	{
		_isCompleted = true;
		foreach (ShopItemDefinition item in ShopItemsToUnlock)
		{
			Singleton<EconomyManager>.Instance.UnlockShopItem(item);
		}
		foreach (ShopItemDefinition item2 in HiddenShopItemsToUnlock)
		{
			Singleton<EconomyManager>.Instance.UnlockShopItem(item2);
		}
	}
}
