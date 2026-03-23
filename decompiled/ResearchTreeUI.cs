using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResearchTreeUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform _researchTreeItemsContainer;

	[SerializeField]
	private ResearchTreeSelectedResearchInfoUI _previewResearchInfoUI;

	[SerializeField]
	private GameObject _purchaseButton;

	[SerializeField]
	private GameObject _cantAffordButton;

	[SerializeField]
	private GameObject _alreadyResearchedButton;

	[SerializeField]
	private GameObject _lockedButton;

	[SerializeField]
	private GameObject _lockedInDemoButton;

	[SerializeField]
	private SoundDefinition _unlockResearchSound;

	private List<ResearchItemButton> _researchItemButtons = new List<ResearchItemButton>();

	private ResearchItemDefinition _currentlyPreviewedResearch;

	private void Update()
	{
	}

	private void Start()
	{
		PreviewResearch(null);
	}

	private void OnEnable()
	{
		Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.OpenResearchTree);
		PopulateQuestTree();
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += OnResearchTicketsUpdated;
	}

	private void OnDisable()
	{
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= OnResearchTicketsUpdated;
	}

	private void OnResearchTicketsUpdated(int amount)
	{
		RefreshActivateButton();
	}

	public void PreviewResearch(ResearchItemDefinition researchItemDefinition)
	{
		_currentlyPreviewedResearch = researchItemDefinition;
		_previewResearchInfoUI.Initialize(researchItemDefinition);
		RefreshActivateButton();
	}

	private void PopulateQuestTree()
	{
		foreach (ResearchItemButton item in _researchTreeItemsContainer.GetComponentsInChildren<ResearchItemButton>().ToList())
		{
			if (item.ResearchItemDefinition == null)
			{
				Debug.Log("Research button without ResearchItemDefinition!");
				continue;
			}
			item.Initialize(this);
			_researchItemButtons.Add(item);
		}
		StartCoroutine(DrawConnectionsNextFrame());
	}

	private IEnumerator DrawConnectionsNextFrame()
	{
		yield return null;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_researchTreeItemsContainer);
		foreach (ResearchItemButton researchItemButton in _researchItemButtons)
		{
			researchItemButton.DrawConnections(_researchTreeItemsContainer);
		}
	}

	public ResearchItemButton GetButtonForResearchDefinition(ResearchItemDefinition researchItemDefinition)
	{
		return _researchItemButtons.FirstOrDefault((ResearchItemButton button) => button.ResearchItemDefinition == researchItemDefinition);
	}

	public void OnBuyResearchPressed()
	{
		if (!_currentlyPreviewedResearch.IsLocked() && _currentlyPreviewedResearch.CanAfford())
		{
			Singleton<ResearchManager>.Instance.ResearchItem(_currentlyPreviewedResearch);
			RefreshActivateButton();
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(_unlockResearchSound, Singleton<SoundManager>.Instance.PlayerTransform.position);
			Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.ResearchSomething);
		}
	}

	public void RefreshActivateButton()
	{
		_purchaseButton.SetActive(value: false);
		_cantAffordButton.SetActive(value: false);
		_alreadyResearchedButton.SetActive(value: false);
		_lockedButton.SetActive(value: false);
		_lockedInDemoButton.SetActive(value: false);
		if (!(_currentlyPreviewedResearch == null))
		{
			if (_currentlyPreviewedResearch.IsResearched())
			{
				_alreadyResearchedButton.SetActive(value: true);
			}
			else if (Singleton<DemoManager>.Instance.IsDemoVersion && _currentlyPreviewedResearch.IsLockedInDemo)
			{
				_lockedInDemoButton.SetActive(value: true);
			}
			else if (_currentlyPreviewedResearch.IsLocked())
			{
				_lockedButton.SetActive(value: true);
			}
			else if (!_currentlyPreviewedResearch.CanAfford())
			{
				_cantAffordButton.SetActive(value: true);
			}
			else
			{
				_purchaseButton.SetActive(value: true);
			}
		}
	}
}
