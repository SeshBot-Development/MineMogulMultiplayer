using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable]
public class ContractInstance
{
	public string Name;

	public string Description;

	public List<QuestRequirement> QuestRequirements = new List<QuestRequirement>();

	public float RewardMoney;

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

	public void DebugUnlock()
	{
		_isCompleted = true;
	}

	public string GetRequirementsText()
	{
		string text = "";
		foreach (QuestRequirement questRequirement in QuestRequirements)
		{
			if (text != "")
			{
				text += "\n";
			}
			text += questRequirement.GetRequirementText();
		}
		return text;
	}

	public string GetPercentCompleteText()
	{
		if (IsCompleted())
		{
			return "100%";
		}
		int num = 0;
		int num2 = 0;
		foreach (QuestRequirement questRequirement in QuestRequirements)
		{
			if (questRequirement is ResourceQuestRequirement resourceQuestRequirement)
			{
				num += resourceQuestRequirement.AmountRequired;
				num2 += Math.Min(resourceQuestRequirement.CurrentAmount, resourceQuestRequirement.AmountRequired);
			}
		}
		if (num == 0)
		{
			return "0%";
		}
		return $"{num2 * 100 / num}%";
	}

	public string GetRewardText()
	{
		return "Payout: " + $"<color=#{Singleton<UIManager>.Instance.MoneyTextColor.ToHexString()}>${RewardMoney}</color>";
	}
}
