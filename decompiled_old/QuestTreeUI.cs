using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestTreeUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform _questTreeItemsContainer;

	[SerializeField]
	private QuestTreeItemButton _questTreeItemButtonPrefab;

	[SerializeField]
	private QuestTreeQuestInfoUI _previewQuestInfoUI;

	[SerializeField]
	private GameObject _activateQuestButton;

	[SerializeField]
	private GameObject _pauseQuestButton;

	[SerializeField]
	private GameObject _questLockedButton;

	[SerializeField]
	private GameObject _questCompletedButton;

	[SerializeField]
	private ResearchTreeUI _researchTreeUI;

	[SerializeField]
	private GameObject _questTreeMenuPanel;

	[SerializeField]
	private Image _questMenuButtonBG;

	[SerializeField]
	private Image _researchMenuButtonBG;

	[SerializeField]
	private Color _inactiveTabColor;

	[SerializeField]
	private Color _activeTabColor;

	private List<QuestTreeItemButton> _questTreeItemButtons = new List<QuestTreeItemButton>();

	private Dictionary<QuestDefinition, QuestTreeItemButton> _questButtons = new Dictionary<QuestDefinition, QuestTreeItemButton>();

	private Quest _currentlyPreviewedQuest;

	[SerializeField]
	private TMP_Text _researchTicketsCountText;

	private void Update()
	{
		_researchTicketsCountText.text = $"<color=#{Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString()}>Research Tickets: ¤{Singleton<ResearchManager>.Instance.ResearchTickets}";
	}

	private void Start()
	{
		PreviewQuest(null);
	}

	private void OnEnable()
	{
		Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.OpenQuestTree);
		PopulateQuestTree();
		Singleton<QuestManager>.Instance.QuestActivated += RefreshQuestInfo;
		Singleton<QuestManager>.Instance.QuestPaused += RefreshQuestInfo;
		Singleton<QuestManager>.Instance.QuestCompleted += RefreshQuestInfo;
	}

	private void OnDisable()
	{
		Singleton<QuestManager>.Instance.QuestActivated -= RefreshQuestInfo;
		Singleton<QuestManager>.Instance.QuestPaused -= RefreshQuestInfo;
		Singleton<QuestManager>.Instance.QuestCompleted -= RefreshQuestInfo;
	}

	public void PreviewQuest(Quest quest)
	{
		_currentlyPreviewedQuest = quest;
		_previewQuestInfoUI.Initialize(quest);
		RefreshActivateButton();
	}

	public void SwitchToResearchTab()
	{
		_questTreeMenuPanel.SetActive(value: false);
		_researchTreeUI.gameObject.SetActive(value: true);
		_researchMenuButtonBG.color = _activeTabColor;
		_questMenuButtonBG.color = _inactiveTabColor;
	}

	public void SwitchToQuestTab()
	{
		_questTreeMenuPanel.SetActive(value: true);
		_researchTreeUI.gameObject.SetActive(value: false);
		_researchMenuButtonBG.color = _inactiveTabColor;
		_questMenuButtonBG.color = _activeTabColor;
	}

	private void OLDPopulateQuestTree()
	{
		foreach (Transform item in _questTreeItemsContainer)
		{
			Object.Destroy(item.gameObject);
		}
		_questTreeItemButtons.Clear();
		foreach (Quest allQuest in Singleton<QuestManager>.Instance.AllQuests)
		{
			QuestTreeItemButton questTreeItemButton = Object.Instantiate(_questTreeItemButtonPrefab, _questTreeItemsContainer);
			questTreeItemButton.Initialize(allQuest, this);
			_questTreeItemButtons.Add(questTreeItemButton);
		}
		StartCoroutine(DrawConnectionsNextFrame());
	}

	private void PopulateQuestTree()
	{
		foreach (QuestTreeItemButton item in _questTreeItemsContainer.GetComponentsInChildren<QuestTreeItemButton>().ToList())
		{
			if (item.QuestDefinition == null)
			{
				Debug.Log("Quest button without QuestDescription!");
				continue;
			}
			item.Initialize(Singleton<QuestManager>.Instance.GetQuestByID(item.QuestDefinition.QuestID), this);
			_questTreeItemButtons.Add(item);
		}
		StartCoroutine(DrawConnectionsNextFrame());
	}

	private IEnumerator DrawConnectionsNextFrame()
	{
		yield return null;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_questTreeItemsContainer);
		foreach (QuestTreeItemButton questTreeItemButton in _questTreeItemButtons)
		{
			questTreeItemButton.DrawConnections(_questTreeItemsContainer);
		}
	}

	public QuestTreeItemButton GetButtonForQuestID(QuestID questID)
	{
		return _questTreeItemButtons.FirstOrDefault((QuestTreeItemButton button) => button.Quest.QuestID == questID);
	}

	private void RefreshQuestInfo(Quest quest)
	{
		PopulateQuestTree();
		RefreshActivateButton();
	}

	public void OnActivateQuestPressed()
	{
		_currentlyPreviewedQuest?.TryActivateQuest();
	}

	public void OnPauseQuestPressed()
	{
		_currentlyPreviewedQuest?.PauseQuest();
	}

	public void RefreshActivateButton()
	{
		_activateQuestButton.SetActive(value: false);
		_pauseQuestButton.SetActive(value: false);
		_questLockedButton.SetActive(value: false);
		_questCompletedButton.SetActive(value: false);
		if (_currentlyPreviewedQuest != null)
		{
			if (_currentlyPreviewedQuest.IsCompleted())
			{
				_questCompletedButton.SetActive(value: true);
			}
			else if (_currentlyPreviewedQuest.IsActive())
			{
				_pauseQuestButton.SetActive(value: true);
			}
			else if (_currentlyPreviewedQuest.IsLocked())
			{
				_questLockedButton.SetActive(value: true);
			}
			else
			{
				_activateQuestButton.SetActive(value: true);
			}
		}
	}
}
