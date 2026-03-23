using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200009C RID: 156
public class QuestTreeQuestInfoUI : MonoBehaviour
{
	// Token: 0x06000430 RID: 1072 RVA: 0x00017028 File Offset: 0x00015228
	public void Initialize(Quest quest)
	{
		if (quest == null)
		{
			this.NameText.text = "Select a Quest";
			this.DescriptionText.text = "";
			this.RewardText.text = "";
			foreach (object obj in this.RequirementsContainer)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
			foreach (object obj2 in this.UnlocksContainer)
			{
				Object.Destroy(((Transform)obj2).gameObject);
			}
			this.RewardsHeader.SetActive(false);
			this.UnlocksHeader.SetActive(false);
			return;
		}
		this._quest = quest;
		this.NameText.text = this._quest.Name;
		this.DescriptionText.text = this._quest.Description;
		foreach (object obj3 in this.RequirementsContainer)
		{
			Object.Destroy(((Transform)obj3).gameObject);
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
		this.RewardText.text = quest.GetRewardTextExcludingUnlocks();
		this.RewardsHeader.SetActive(!string.IsNullOrEmpty(this.RewardText.text));
		foreach (object obj4 in this.UnlocksContainer)
		{
			Object.Destroy(((Transform)obj4).gameObject);
		}
		List<ShopItemDefinition> list = this._quest.ShopItemsToUnlock.ToList<ShopItemDefinition>();
		list.AddRange(this._quest.HiddenShopItemsToUnlock);
		this.UnlocksHeader.SetActive(list.Count > 0);
		foreach (ShopItemDefinition shopItemDefinition in list)
		{
			if (!shopItemDefinition.IsDummyItem)
			{
				Object.Instantiate<QuestPreviewRewardEntry>(this.UnlocksRewardUIPrefab, this.UnlocksContainer).Initialize(shopItemDefinition.GetName(), shopItemDefinition.GetIcon(), shopItemDefinition.GetDescription());
			}
		}
		base.StartCoroutine(this.WaitThenRebuildLayout());
	}

	// Token: 0x06000431 RID: 1073 RVA: 0x00017340 File Offset: 0x00015540
	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield break;
	}

	// Token: 0x06000432 RID: 1074 RVA: 0x0001734F File Offset: 0x0001554F
	private void Update()
	{
		if (this._quest != null && !this._quest.IsCompleted())
		{
			this.RefreshDisplay();
		}
	}

	// Token: 0x06000433 RID: 1075 RVA: 0x0001736C File Offset: 0x0001556C
	public void RefreshDisplay()
	{
		foreach (QuestRequirementUI questRequirementUI in this._requirementUIs)
		{
			questRequirementUI.RefreshDisplay();
		}
	}

	// Token: 0x040004CE RID: 1230
	private Quest _quest;

	// Token: 0x040004CF RID: 1231
	private List<QuestRequirementUI> _requirementUIs = new List<QuestRequirementUI>();

	// Token: 0x040004D0 RID: 1232
	public TMP_Text NameText;

	// Token: 0x040004D1 RID: 1233
	public TMP_Text DescriptionText;

	// Token: 0x040004D2 RID: 1234
	public TMP_Text RewardText;

	// Token: 0x040004D3 RID: 1235
	public RectTransform RequirementsContainer;

	// Token: 0x040004D4 RID: 1236
	public QuestRequirementUI RequirementUIPrefab;

	// Token: 0x040004D5 RID: 1237
	public GameObject RewardsHeader;

	// Token: 0x040004D6 RID: 1238
	public GameObject UnlocksHeader;

	// Token: 0x040004D7 RID: 1239
	public RectTransform UnlocksContainer;

	// Token: 0x040004D8 RID: 1240
	public QuestPreviewRewardEntry UnlocksRewardUIPrefab;
}
