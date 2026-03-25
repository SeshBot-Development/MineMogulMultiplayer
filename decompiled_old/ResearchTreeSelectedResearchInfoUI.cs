using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ResearchTreeSelectedResearchInfoUI : MonoBehaviour
{
	public TMP_Text NameText;

	public TMP_Text DescriptionText;

	public TMP_Text ResearchTicketCostText;

	public TMP_Text MoneyCostText;

	public GameObject UnlocksHeader;

	public GameObject UpgradesHeader;

	public TMP_Text UpgradesText;

	public RectTransform UnlocksContainer;

	public QuestPreviewRewardEntry UnlocksRewardUIPrefab;

	[SerializeField]
	private Color _availableColor;

	[SerializeField]
	private Color _lockedColor;

	[SerializeField]
	private Color _researchedColor;

	[SerializeField]
	private Color _tooExpensiveColor;

	private ResearchItemDefinition _researchItemDefinition;

	private void OnEnable()
	{
		RefreshDisplay();
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += OnResearchTicketsUpdated;
	}

	private void OnDisable()
	{
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= OnResearchTicketsUpdated;
	}

	private void OnOtherItemResearched(ResearchItemDefinition researchedItem)
	{
		RefreshDisplay();
	}

	private void OnResearchTicketsUpdated(int amount)
	{
		RefreshDisplay();
	}

	public void RefreshDisplay()
	{
		Initialize(_researchItemDefinition);
	}

	public void Initialize(ResearchItemDefinition researchItemDefinition)
	{
		foreach (Transform item in UnlocksContainer)
		{
			Object.Destroy(item.gameObject);
		}
		if (researchItemDefinition == null)
		{
			NameText.text = "Select a Research Item";
			DescriptionText.text = "";
			ResearchTicketCostText.text = "";
			MoneyCostText.text = "";
			UpgradesText.text = "";
			UnlocksHeader.SetActive(value: false);
			UpgradesHeader.SetActive(value: false);
			return;
		}
		_researchItemDefinition = researchItemDefinition;
		NameText.text = _researchItemDefinition.GetName();
		if (!_researchItemDefinition.IsLocked())
		{
			_researchItemDefinition.IsResearched();
		}
		int researchTicketCost = _researchItemDefinition.GetResearchTicketCost();
		if (researchTicketCost > 0)
		{
			ResearchTicketCostText.text = string.Format("<color=#{0}>¤{1} Research Ticket{2}", Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), researchTicketCost, (researchTicketCost > 1) ? "s" : "");
		}
		else
		{
			ResearchTicketCostText.text = "";
		}
		if (_researchItemDefinition.GetMoneyCost() > 0f)
		{
			MoneyCostText.text = Singleton<EconomyManager>.Instance.GetColoredFormattedMoneyString(_researchItemDefinition.GetMoneyCost(), includeDecimal: false);
		}
		else
		{
			MoneyCostText.text = "";
		}
		if (_researchItemDefinition is ShopItemResearchItemDefinition shopItemResearchItemDefinition)
		{
			UpgradesText.text = "";
			List<ShopItemDefinition> shopItemDefinitions = shopItemResearchItemDefinition.ShopItemDefinitions;
			UnlocksHeader.SetActive(shopItemDefinitions.Count > 0);
			UpgradesHeader.SetActive(value: false);
			foreach (ShopItemDefinition item2 in shopItemDefinitions)
			{
				if (!item2.IsDummyItem)
				{
					Object.Instantiate(UnlocksRewardUIPrefab, UnlocksContainer).Initialize(item2.GetName(), item2.GetIcon(), item2.GetDescription());
				}
			}
		}
		else if (_researchItemDefinition is UpgradeDepositBoxResearchItemDefinition upgradeDepositBoxResearchItemDefinition)
		{
			UpgradesHeader.SetActive(value: true);
			UnlocksHeader.SetActive(value: false);
			UpgradesText.text = upgradeDepositBoxResearchItemDefinition.GetDescription();
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
}
