using System;
using System.Collections.Generic;
using Unity.VisualScripting;

// Token: 0x0200002D RID: 45
[Serializable]
public class ContractInstance
{
	// Token: 0x06000147 RID: 327 RVA: 0x00007984 File Offset: 0x00005B84
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

	// Token: 0x06000148 RID: 328 RVA: 0x00007A00 File Offset: 0x00005C00
	public void DebugUnlock()
	{
		this._isCompleted = true;
	}

	// Token: 0x06000149 RID: 329 RVA: 0x00007A0C File Offset: 0x00005C0C
	public string GetRequirementsText()
	{
		string text = "";
		foreach (QuestRequirement questRequirement in this.QuestRequirements)
		{
			if (text != "")
			{
				text += "\n";
			}
			text += questRequirement.GetRequirementText();
		}
		return text;
	}

	// Token: 0x0600014A RID: 330 RVA: 0x00007A88 File Offset: 0x00005C88
	public string GetPercentCompleteText()
	{
		if (this.IsCompleted())
		{
			return "100%";
		}
		int num = 0;
		int num2 = 0;
		foreach (QuestRequirement questRequirement in this.QuestRequirements)
		{
			ResourceQuestRequirement resourceQuestRequirement = questRequirement as ResourceQuestRequirement;
			if (resourceQuestRequirement != null)
			{
				num += resourceQuestRequirement.AmountRequired;
				num2 += Math.Min(resourceQuestRequirement.CurrentAmount, resourceQuestRequirement.AmountRequired);
			}
		}
		if (num == 0)
		{
			return "0%";
		}
		return string.Format("{0}%", num2 * 100 / num);
	}

	// Token: 0x0600014B RID: 331 RVA: 0x00007B2C File Offset: 0x00005D2C
	public string GetRewardText()
	{
		return "Payout: " + string.Format("<color=#{0}>${1}</color>", global::Singleton<UIManager>.Instance.MoneyTextColor.ToHexString(), this.RewardMoney);
	}

	// Token: 0x04000145 RID: 325
	public string Name;

	// Token: 0x04000146 RID: 326
	public string Description;

	// Token: 0x04000147 RID: 327
	public List<QuestRequirement> QuestRequirements = new List<QuestRequirement>();

	// Token: 0x04000148 RID: 328
	public float RewardMoney;

	// Token: 0x04000149 RID: 329
	private bool _isCompleted;
}
