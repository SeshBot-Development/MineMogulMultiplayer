using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestInfoUI : MonoBehaviour
{
	private Quest _quest;

	private List<QuestRequirementUI> _requirementUIs = new List<QuestRequirementUI>();

	public TMP_Text NameText;

	public TMP_Text RewardText;

	public RectTransform RequirementsContainer;

	public QuestRequirementUI RequirementUIPrefab;

	public void Initialize(Quest quest)
	{
		_quest = quest;
		NameText.text = _quest.Name;
		foreach (Transform item in RequirementsContainer)
		{
			Object.Destroy(item.gameObject);
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
		string rewardText = quest.GetRewardText();
		if (string.IsNullOrEmpty(rewardText))
		{
			Object.Destroy(RewardText.gameObject);
		}
		else
		{
			RewardText.text = rewardText;
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
		if (_quest == null || _quest.IsCompleted())
		{
			Object.Destroy(base.gameObject);
		}
		else
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
