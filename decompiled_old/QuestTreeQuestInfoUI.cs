using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestTreeQuestInfoUI : MonoBehaviour
{
	private Quest _quest;

	private List<QuestRequirementUI> _requirementUIs = new List<QuestRequirementUI>();

	public TMP_Text NameText;

	public TMP_Text DescriptionText;

	public TMP_Text RewardText;

	public RectTransform RequirementsContainer;

	public QuestRequirementUI RequirementUIPrefab;

	public GameObject RewardsHeader;

	public GameObject UnlocksHeader;

	public RectTransform UnlocksContainer;

	public QuestPreviewRewardEntry UnlocksRewardUIPrefab;

	public void Initialize(Quest quest)
	{
		if (quest == null)
		{
			NameText.text = "Select a Quest";
			DescriptionText.text = "";
			RewardText.text = "";
			foreach (Transform item in RequirementsContainer)
			{
				Object.Destroy(item.gameObject);
			}
			foreach (Transform item2 in UnlocksContainer)
			{
				Object.Destroy(item2.gameObject);
			}
			RewardsHeader.SetActive(value: false);
			UnlocksHeader.SetActive(value: false);
			return;
		}
		_quest = quest;
		NameText.text = _quest.Name;
		DescriptionText.text = _quest.Description;
		foreach (Transform item3 in RequirementsContainer)
		{
			Object.Destroy(item3.gameObject);
		}
		_requirementUIs.Clear();
		foreach (QuestRequirement questRequirement in _quest.QuestRequirements)
		{
			if (!questRequirement.IsHidden)
			{
				QuestRequirementUI questRequirementUI = Object.Instantiate(RequirementUIPrefab, RequirementsContainer);
				questRequirementUI.Initialize(questRequirement);
				_requirementUIs.Add(questRequirementUI);
			}
		}
		RewardText.text = quest.GetRewardTextExcludingUnlocks();
		RewardsHeader.SetActive(!string.IsNullOrEmpty(RewardText.text));
		foreach (Transform item4 in UnlocksContainer)
		{
			Object.Destroy(item4.gameObject);
		}
		List<ShopItemDefinition> list = _quest.ShopItemsToUnlock.ToList();
		list.AddRange(_quest.HiddenShopItemsToUnlock);
		UnlocksHeader.SetActive(list.Count > 0);
		foreach (ShopItemDefinition item5 in list)
		{
			if (!item5.IsDummyItem)
			{
				Object.Instantiate(UnlocksRewardUIPrefab, UnlocksContainer).Initialize(item5.GetName(), item5.GetIcon(), item5.GetDescription());
			}
		}
		StartCoroutine(WaitThenRebuildLayout());
	}

	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
	}

	private void Update()
	{
		if (_quest != null && !_quest.IsCompleted())
		{
			RefreshDisplay();
		}
	}

	public void RefreshDisplay()
	{
		foreach (QuestRequirementUI requirementUI in _requirementUIs)
		{
			requirementUI.RefreshDisplay();
		}
	}
}
