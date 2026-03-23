using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestHud : MonoBehaviour
{
	private List<QuestInfoUI> _questInfoUIs = new List<QuestInfoUI>();

	public RectTransform QuestInfoUIsContainer;

	public QuestInfoUI QuestInfoUIPrefab;

	private void OnEnable()
	{
		RegenerateQuestList();
		Singleton<QuestManager>.Instance.QuestCompleted += OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestPaused += OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestActivated += OnQuestEvent;
		StartCoroutine(RebuildNextFrame());
	}

	private IEnumerator RebuildNextFrame()
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(QuestInfoUIsContainer);
	}

	private void OnDisable()
	{
		Singleton<QuestManager>.Instance.QuestCompleted -= OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestPaused -= OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestActivated -= OnQuestEvent;
	}

	private void Update()
	{
		if (QuestInfoUIsContainer.childCount == 0)
		{
			RegenerateQuestList();
		}
	}

	private void OnQuestEvent(Quest quest)
	{
		RegenerateQuestList();
	}

	public void RegenerateQuestList()
	{
		if (QuestInfoUIsContainer.childCount > 0)
		{
			foreach (Transform item in QuestInfoUIsContainer)
			{
				Object.Destroy(item.gameObject);
			}
			_questInfoUIs.Clear();
		}
		foreach (Quest item2 in Singleton<QuestManager>.Instance.ActiveQuests.OrderByDescending((Quest q) => q.UIPriority))
		{
			AddQuest(item2);
		}
	}

	private void AddQuest(Quest quest)
	{
		QuestInfoUI questInfoUI = Object.Instantiate(QuestInfoUIPrefab, QuestInfoUIsContainer);
		questInfoUI.Initialize(quest);
		_questInfoUIs.Add(questInfoUI);
	}
}
