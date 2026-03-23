using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200009D RID: 157
public class QuestTreeUI : MonoBehaviour
{
	// Token: 0x06000435 RID: 1077 RVA: 0x000173CF File Offset: 0x000155CF
	private void Update()
	{
		this._researchTicketsCountText.text = string.Format("<color=#{0}>Research Tickets: ¤{1}", global::Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), global::Singleton<ResearchManager>.Instance.ResearchTickets);
	}

	// Token: 0x06000436 RID: 1078 RVA: 0x00017404 File Offset: 0x00015604
	private void Start()
	{
		this.PreviewQuest(null);
	}

	// Token: 0x06000437 RID: 1079 RVA: 0x00017410 File Offset: 0x00015610
	private void OnEnable()
	{
		global::Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.OpenQuestTree, 1);
		this.PopulateQuestTree();
		global::Singleton<QuestManager>.Instance.QuestActivated += this.RefreshQuestInfo;
		global::Singleton<QuestManager>.Instance.QuestPaused += this.RefreshQuestInfo;
		global::Singleton<QuestManager>.Instance.QuestCompleted += this.RefreshQuestInfo;
	}

	// Token: 0x06000438 RID: 1080 RVA: 0x00017474 File Offset: 0x00015674
	private void OnDisable()
	{
		global::Singleton<QuestManager>.Instance.QuestActivated -= this.RefreshQuestInfo;
		global::Singleton<QuestManager>.Instance.QuestPaused -= this.RefreshQuestInfo;
		global::Singleton<QuestManager>.Instance.QuestCompleted -= this.RefreshQuestInfo;
	}

	// Token: 0x06000439 RID: 1081 RVA: 0x000174C3 File Offset: 0x000156C3
	public void PreviewQuest(Quest quest)
	{
		this._currentlyPreviewedQuest = quest;
		this._previewQuestInfoUI.Initialize(quest);
		this.RefreshActivateButton();
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x000174E0 File Offset: 0x000156E0
	public void SwitchToResearchTab()
	{
		this._questTreeMenuPanel.SetActive(false);
		this._researchTreeUI.gameObject.SetActive(true);
		this._researchMenuButtonBG.color = this._activeTabColor;
		this._questMenuButtonBG.color = this._inactiveTabColor;
	}

	// Token: 0x0600043B RID: 1083 RVA: 0x0001752C File Offset: 0x0001572C
	public void SwitchToQuestTab()
	{
		this._questTreeMenuPanel.SetActive(true);
		this._researchTreeUI.gameObject.SetActive(false);
		this._researchMenuButtonBG.color = this._inactiveTabColor;
		this._questMenuButtonBG.color = this._activeTabColor;
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x00017578 File Offset: 0x00015778
	private void OLDPopulateQuestTree()
	{
		foreach (object obj in this._questTreeItemsContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this._questTreeItemButtons.Clear();
		foreach (Quest quest in global::Singleton<QuestManager>.Instance.AllQuests)
		{
			QuestTreeItemButton questTreeItemButton = Object.Instantiate<QuestTreeItemButton>(this._questTreeItemButtonPrefab, this._questTreeItemsContainer);
			questTreeItemButton.Initialize(quest, this);
			this._questTreeItemButtons.Add(questTreeItemButton);
		}
		base.StartCoroutine(this.DrawConnectionsNextFrame());
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x00017654 File Offset: 0x00015854
	private void PopulateQuestTree()
	{
		foreach (QuestTreeItemButton questTreeItemButton in this._questTreeItemsContainer.GetComponentsInChildren<QuestTreeItemButton>().ToList<QuestTreeItemButton>())
		{
			if (questTreeItemButton.QuestDefinition == null)
			{
				Debug.Log("Quest button without QuestDescription!");
			}
			else
			{
				questTreeItemButton.Initialize(global::Singleton<QuestManager>.Instance.GetQuestByID(questTreeItemButton.QuestDefinition.QuestID), this);
				this._questTreeItemButtons.Add(questTreeItemButton);
			}
		}
		base.StartCoroutine(this.DrawConnectionsNextFrame());
	}

	// Token: 0x0600043E RID: 1086 RVA: 0x000176FC File Offset: 0x000158FC
	private IEnumerator DrawConnectionsNextFrame()
	{
		yield return null;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(this._questTreeItemsContainer);
		using (List<QuestTreeItemButton>.Enumerator enumerator = this._questTreeItemButtons.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				QuestTreeItemButton questTreeItemButton = enumerator.Current;
				questTreeItemButton.DrawConnections(this._questTreeItemsContainer);
			}
			yield break;
		}
		yield break;
	}

	// Token: 0x0600043F RID: 1087 RVA: 0x0001770C File Offset: 0x0001590C
	public QuestTreeItemButton GetButtonForQuestID(QuestID questID)
	{
		return this._questTreeItemButtons.FirstOrDefault((QuestTreeItemButton button) => button.Quest.QuestID == questID);
	}

	// Token: 0x06000440 RID: 1088 RVA: 0x0001773D File Offset: 0x0001593D
	private void RefreshQuestInfo(Quest quest)
	{
		this.PopulateQuestTree();
		this.RefreshActivateButton();
	}

	// Token: 0x06000441 RID: 1089 RVA: 0x0001774B File Offset: 0x0001594B
	public void OnActivateQuestPressed()
	{
		Quest currentlyPreviewedQuest = this._currentlyPreviewedQuest;
		if (currentlyPreviewedQuest == null)
		{
			return;
		}
		currentlyPreviewedQuest.TryActivateQuest();
	}

	// Token: 0x06000442 RID: 1090 RVA: 0x0001775E File Offset: 0x0001595E
	public void OnPauseQuestPressed()
	{
		Quest currentlyPreviewedQuest = this._currentlyPreviewedQuest;
		if (currentlyPreviewedQuest == null)
		{
			return;
		}
		currentlyPreviewedQuest.PauseQuest();
	}

	// Token: 0x06000443 RID: 1091 RVA: 0x00017770 File Offset: 0x00015970
	public void RefreshActivateButton()
	{
		this._activateQuestButton.SetActive(false);
		this._pauseQuestButton.SetActive(false);
		this._questLockedButton.SetActive(false);
		this._questCompletedButton.SetActive(false);
		if (this._currentlyPreviewedQuest == null)
		{
			return;
		}
		if (this._currentlyPreviewedQuest.IsCompleted())
		{
			this._questCompletedButton.SetActive(true);
			return;
		}
		if (this._currentlyPreviewedQuest.IsActive())
		{
			this._pauseQuestButton.SetActive(true);
			return;
		}
		if (this._currentlyPreviewedQuest.IsLocked())
		{
			this._questLockedButton.SetActive(true);
			return;
		}
		this._activateQuestButton.SetActive(true);
	}

	// Token: 0x040004D9 RID: 1241
	[SerializeField]
	private RectTransform _questTreeItemsContainer;

	// Token: 0x040004DA RID: 1242
	[SerializeField]
	private QuestTreeItemButton _questTreeItemButtonPrefab;

	// Token: 0x040004DB RID: 1243
	[SerializeField]
	private QuestTreeQuestInfoUI _previewQuestInfoUI;

	// Token: 0x040004DC RID: 1244
	[SerializeField]
	private GameObject _activateQuestButton;

	// Token: 0x040004DD RID: 1245
	[SerializeField]
	private GameObject _pauseQuestButton;

	// Token: 0x040004DE RID: 1246
	[SerializeField]
	private GameObject _questLockedButton;

	// Token: 0x040004DF RID: 1247
	[SerializeField]
	private GameObject _questCompletedButton;

	// Token: 0x040004E0 RID: 1248
	[SerializeField]
	private ResearchTreeUI _researchTreeUI;

	// Token: 0x040004E1 RID: 1249
	[SerializeField]
	private GameObject _questTreeMenuPanel;

	// Token: 0x040004E2 RID: 1250
	[SerializeField]
	private Image _questMenuButtonBG;

	// Token: 0x040004E3 RID: 1251
	[SerializeField]
	private Image _researchMenuButtonBG;

	// Token: 0x040004E4 RID: 1252
	[SerializeField]
	private Color _inactiveTabColor;

	// Token: 0x040004E5 RID: 1253
	[SerializeField]
	private Color _activeTabColor;

	// Token: 0x040004E6 RID: 1254
	private List<QuestTreeItemButton> _questTreeItemButtons = new List<QuestTreeItemButton>();

	// Token: 0x040004E7 RID: 1255
	private Dictionary<QuestDefinition, QuestTreeItemButton> _questButtons = new Dictionary<QuestDefinition, QuestTreeItemButton>();

	// Token: 0x040004E8 RID: 1256
	private Quest _currentlyPreviewedQuest;

	// Token: 0x040004E9 RID: 1257
	[SerializeField]
	private TMP_Text _researchTicketsCountText;
}
