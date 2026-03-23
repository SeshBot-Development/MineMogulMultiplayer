using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000A5 RID: 165
public class ResearchTreeSelectedResearchInfoUI : MonoBehaviour
{
	// Token: 0x06000490 RID: 1168 RVA: 0x00018CBF File Offset: 0x00016EBF
	private void OnEnable()
	{
		this.RefreshDisplay();
		global::Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += this.OnResearchTicketsUpdated;
	}

	// Token: 0x06000491 RID: 1169 RVA: 0x00018CDD File Offset: 0x00016EDD
	private void OnDisable()
	{
		global::Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= this.OnResearchTicketsUpdated;
	}

	// Token: 0x06000492 RID: 1170 RVA: 0x00018CF5 File Offset: 0x00016EF5
	private void OnOtherItemResearched(ResearchItemDefinition researchedItem)
	{
		this.RefreshDisplay();
	}

	// Token: 0x06000493 RID: 1171 RVA: 0x00018CFD File Offset: 0x00016EFD
	private void OnResearchTicketsUpdated(int amount)
	{
		this.RefreshDisplay();
	}

	// Token: 0x06000494 RID: 1172 RVA: 0x00018D05 File Offset: 0x00016F05
	public void RefreshDisplay()
	{
		this.Initialize(this._researchItemDefinition);
	}

	// Token: 0x06000495 RID: 1173 RVA: 0x00018D14 File Offset: 0x00016F14
	public void Initialize(ResearchItemDefinition researchItemDefinition)
	{
		foreach (object obj in this.UnlocksContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		if (researchItemDefinition == null)
		{
			this.NameText.text = "Select a Research Item";
			this.DescriptionText.text = "";
			this.ResearchTicketCostText.text = "";
			this.MoneyCostText.text = "";
			this.UpgradesText.text = "";
			this.UnlocksHeader.SetActive(false);
			this.UpgradesHeader.SetActive(false);
			return;
		}
		this._researchItemDefinition = researchItemDefinition;
		this.NameText.text = this._researchItemDefinition.GetName();
		if (!this._researchItemDefinition.IsLocked())
		{
			this._researchItemDefinition.IsResearched();
		}
		int researchTicketCost = this._researchItemDefinition.GetResearchTicketCost();
		if (researchTicketCost > 0)
		{
			this.ResearchTicketCostText.text = string.Format("<color=#{0}>¤{1} Research Ticket{2}", global::Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), researchTicketCost, (researchTicketCost > 1) ? "s" : "");
		}
		else
		{
			this.ResearchTicketCostText.text = "";
		}
		if (this._researchItemDefinition.GetMoneyCost() > 0f)
		{
			this.MoneyCostText.text = global::Singleton<EconomyManager>.Instance.GetColoredFormattedMoneyString(this._researchItemDefinition.GetMoneyCost(), false);
		}
		else
		{
			this.MoneyCostText.text = "";
		}
		ShopItemResearchItemDefinition shopItemResearchItemDefinition = this._researchItemDefinition as ShopItemResearchItemDefinition;
		if (shopItemResearchItemDefinition != null)
		{
			this.UpgradesText.text = "";
			List<ShopItemDefinition> shopItemDefinitions = shopItemResearchItemDefinition.ShopItemDefinitions;
			this.UnlocksHeader.SetActive(shopItemDefinitions.Count > 0);
			this.UpgradesHeader.SetActive(false);
			using (List<ShopItemDefinition>.Enumerator enumerator2 = shopItemDefinitions.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ShopItemDefinition shopItemDefinition = enumerator2.Current;
					if (!shopItemDefinition.IsDummyItem)
					{
						Object.Instantiate<QuestPreviewRewardEntry>(this.UnlocksRewardUIPrefab, this.UnlocksContainer).Initialize(shopItemDefinition.GetName(), shopItemDefinition.GetIcon(), shopItemDefinition.GetDescription());
					}
				}
				goto IL_026E;
			}
		}
		UpgradeDepositBoxResearchItemDefinition upgradeDepositBoxResearchItemDefinition = this._researchItemDefinition as UpgradeDepositBoxResearchItemDefinition;
		if (upgradeDepositBoxResearchItemDefinition != null)
		{
			this.UpgradesHeader.SetActive(true);
			this.UnlocksHeader.SetActive(false);
			this.UpgradesText.text = upgradeDepositBoxResearchItemDefinition.GetDescription();
		}
		IL_026E:
		base.StartCoroutine(this.WaitThenRebuildLayout());
	}

	// Token: 0x06000496 RID: 1174 RVA: 0x00018FB8 File Offset: 0x000171B8
	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield break;
	}

	// Token: 0x0400051E RID: 1310
	public TMP_Text NameText;

	// Token: 0x0400051F RID: 1311
	public TMP_Text DescriptionText;

	// Token: 0x04000520 RID: 1312
	public TMP_Text ResearchTicketCostText;

	// Token: 0x04000521 RID: 1313
	public TMP_Text MoneyCostText;

	// Token: 0x04000522 RID: 1314
	public GameObject UnlocksHeader;

	// Token: 0x04000523 RID: 1315
	public GameObject UpgradesHeader;

	// Token: 0x04000524 RID: 1316
	public TMP_Text UpgradesText;

	// Token: 0x04000525 RID: 1317
	public RectTransform UnlocksContainer;

	// Token: 0x04000526 RID: 1318
	public QuestPreviewRewardEntry UnlocksRewardUIPrefab;

	// Token: 0x04000527 RID: 1319
	[SerializeField]
	private Color _availableColor;

	// Token: 0x04000528 RID: 1320
	[SerializeField]
	private Color _lockedColor;

	// Token: 0x04000529 RID: 1321
	[SerializeField]
	private Color _researchedColor;

	// Token: 0x0400052A RID: 1322
	[SerializeField]
	private Color _tooExpensiveColor;

	// Token: 0x0400052B RID: 1323
	private ResearchItemDefinition _researchItemDefinition;
}
