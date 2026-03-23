using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000097 RID: 151
public class QuestInfoUI : MonoBehaviour
{
	// Token: 0x06000404 RID: 1028 RVA: 0x00015DD0 File Offset: 0x00013FD0
	public void Initialize(Quest quest)
	{
		this._quest = quest;
		this.NameText.text = this._quest.Name;
		foreach (object obj in this.RequirementsContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this._requirementUIs.Clear();
		foreach (QuestRequirement questRequirement in this._quest.QuestRequirements)
		{
			if (!questRequirement.IsHidden)
			{
				QuestRequirementUI questRequirementUI = Object.Instantiate<QuestRequirementUI>(this.RequirementUIPrefab, this.RequirementsContainer);
				questRequirementUI.Initialize(questRequirement);
				this._requirementUIs.Add(questRequirementUI);
			}
		}
		string rewardText = quest.GetRewardText();
		if (string.IsNullOrEmpty(rewardText))
		{
			Object.Destroy(this.RewardText.gameObject);
		}
		else
		{
			this.RewardText.text = rewardText;
		}
		base.StartCoroutine(this.WaitThenRebuildLayout());
	}

	// Token: 0x06000405 RID: 1029 RVA: 0x00015F00 File Offset: 0x00014100
	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield break;
	}

	// Token: 0x06000406 RID: 1030 RVA: 0x00015F0F File Offset: 0x0001410F
	private void Update()
	{
		if (this._quest == null || this._quest.IsCompleted())
		{
			Object.Destroy(base.gameObject);
			return;
		}
		this.RefreshDisplay();
	}

	// Token: 0x06000407 RID: 1031 RVA: 0x00015F38 File Offset: 0x00014138
	public void RefreshDisplay()
	{
		foreach (QuestRequirementUI questRequirementUI in this._requirementUIs)
		{
			questRequirementUI.RefreshDisplay();
		}
	}

	// Token: 0x040004AA RID: 1194
	private Quest _quest;

	// Token: 0x040004AB RID: 1195
	private List<QuestRequirementUI> _requirementUIs = new List<QuestRequirementUI>();

	// Token: 0x040004AC RID: 1196
	public TMP_Text NameText;

	// Token: 0x040004AD RID: 1197
	public TMP_Text RewardText;

	// Token: 0x040004AE RID: 1198
	public RectTransform RequirementsContainer;

	// Token: 0x040004AF RID: 1199
	public QuestRequirementUI RequirementUIPrefab;
}
