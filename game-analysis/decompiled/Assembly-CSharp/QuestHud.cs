using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000096 RID: 150
public class QuestHud : MonoBehaviour
{
	// Token: 0x060003FC RID: 1020 RVA: 0x00015BD0 File Offset: 0x00013DD0
	private void OnEnable()
	{
		this.RegenerateQuestList();
		Singleton<QuestManager>.Instance.QuestCompleted += this.OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestPaused += this.OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestActivated += this.OnQuestEvent;
		base.StartCoroutine(this.RebuildNextFrame());
	}

	// Token: 0x060003FD RID: 1021 RVA: 0x00015C32 File Offset: 0x00013E32
	private IEnumerator RebuildNextFrame()
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.QuestInfoUIsContainer);
		yield break;
	}

	// Token: 0x060003FE RID: 1022 RVA: 0x00015C44 File Offset: 0x00013E44
	private void OnDisable()
	{
		Singleton<QuestManager>.Instance.QuestCompleted -= this.OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestPaused -= this.OnQuestEvent;
		Singleton<QuestManager>.Instance.QuestActivated -= this.OnQuestEvent;
	}

	// Token: 0x060003FF RID: 1023 RVA: 0x00015C93 File Offset: 0x00013E93
	private void Update()
	{
		if (this.QuestInfoUIsContainer.childCount == 0)
		{
			this.RegenerateQuestList();
		}
	}

	// Token: 0x06000400 RID: 1024 RVA: 0x00015CA8 File Offset: 0x00013EA8
	private void OnQuestEvent(Quest quest)
	{
		this.RegenerateQuestList();
	}

	// Token: 0x06000401 RID: 1025 RVA: 0x00015CB0 File Offset: 0x00013EB0
	public void RegenerateQuestList()
	{
		if (this.QuestInfoUIsContainer.childCount > 0)
		{
			foreach (object obj in this.QuestInfoUIsContainer)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
			this._questInfoUIs.Clear();
		}
		foreach (Quest quest in Singleton<QuestManager>.Instance.ActiveQuests.OrderByDescending((Quest q) => q.UIPriority))
		{
			this.AddQuest(quest);
		}
	}

	// Token: 0x06000402 RID: 1026 RVA: 0x00015D88 File Offset: 0x00013F88
	private void AddQuest(Quest quest)
	{
		QuestInfoUI questInfoUI = Object.Instantiate<QuestInfoUI>(this.QuestInfoUIPrefab, this.QuestInfoUIsContainer);
		questInfoUI.Initialize(quest);
		this._questInfoUIs.Add(questInfoUI);
	}

	// Token: 0x040004A7 RID: 1191
	private List<QuestInfoUI> _questInfoUIs = new List<QuestInfoUI>();

	// Token: 0x040004A8 RID: 1192
	public RectTransform QuestInfoUIsContainer;

	// Token: 0x040004A9 RID: 1193
	public QuestInfoUI QuestInfoUIPrefab;
}
