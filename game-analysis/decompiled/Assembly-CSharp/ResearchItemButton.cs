using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

// Token: 0x020000A2 RID: 162
public class ResearchItemButton : MonoBehaviour
{
	// Token: 0x06000468 RID: 1128 RVA: 0x0001808F File Offset: 0x0001628F
	public void Initialize(ResearchTreeUI parentResearchTreeUI)
	{
		this._parentResearchTreeUI = parentResearchTreeUI;
		this.RefreshDisplay();
	}

	// Token: 0x06000469 RID: 1129 RVA: 0x000180A0 File Offset: 0x000162A0
	private void OnEnable()
	{
		this._icon.sprite = this.ResearchItemDefinition.GetIcon();
		this.RefreshDisplay();
		global::Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += this.OnResearchTicketsUpdated;
		global::Singleton<ResearchManager>.Instance.ResearchItemResearched += this.OnOtherItemResearched;
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x000180F5 File Offset: 0x000162F5
	private void OnDisable()
	{
		global::Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= this.OnResearchTicketsUpdated;
		global::Singleton<ResearchManager>.Instance.ResearchItemResearched -= this.OnOtherItemResearched;
	}

	// Token: 0x0600046B RID: 1131 RVA: 0x00018123 File Offset: 0x00016323
	public void OnPressed()
	{
		this._parentResearchTreeUI.PreviewResearch(this.ResearchItemDefinition);
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x00018136 File Offset: 0x00016336
	[Obsolete]
	public void OLD_OnPressed()
	{
		if (!this.ResearchItemDefinition.IsLocked() && this.ResearchItemDefinition.CanAfford())
		{
			global::Singleton<ResearchManager>.Instance.ResearchItem(this.ResearchItemDefinition);
			this.RefreshDisplay();
		}
	}

	// Token: 0x0600046D RID: 1133 RVA: 0x00018168 File Offset: 0x00016368
	private void OnResearchTicketsUpdated(int amount)
	{
		if (this.ResearchItemDefinition.IsLocked())
		{
			return;
		}
		if (global::Singleton<ResearchManager>.Instance.IsResearchItemCompleted(this.ResearchItemDefinition))
		{
			return;
		}
		this.RefreshDisplay();
	}

	// Token: 0x0600046E RID: 1134 RVA: 0x00018191 File Offset: 0x00016391
	private void OnOtherItemResearched(ResearchItemDefinition researchedItem)
	{
		if (researchedItem == this.ResearchItemDefinition || (this.ResearchItemDefinition.PrerequisiteResearch.Contains(researchedItem) && !this.ResearchItemDefinition.IsLocked()))
		{
			this.RefreshDisplay();
		}
	}

	// Token: 0x0600046F RID: 1135 RVA: 0x000181C8 File Offset: 0x000163C8
	public void RefreshDisplay()
	{
		if (this.ResearchItemDefinition.IsLocked())
		{
			this.SetColors(this._lockedColor);
			this._costText.text = string.Format("¤{0}", this.ResearchItemDefinition.GetResearchTicketCost());
			if (this.ResearchItemDefinition.GetMoneyCost() > 0f)
			{
				TMP_Text costText = this._costText;
				costText.text = costText.text + "  " + EconomyManager.GetFormattedMoneyString(this.ResearchItemDefinition.GetMoneyCost(), false);
				return;
			}
		}
		else
		{
			if (global::Singleton<ResearchManager>.Instance.IsResearchItemCompleted(this.ResearchItemDefinition))
			{
				this.SetColors(this._researchedColor);
				this._costText.text = "Researched";
				return;
			}
			if (this.ResearchItemDefinition.CanAfford())
			{
				this.SetColors(this._availableColor);
			}
			else
			{
				this.SetColors(this._tooExpensiveColor);
			}
			this._costText.text = string.Format("<color=#{0}>¤{1}", global::Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString(), this.ResearchItemDefinition.GetResearchTicketCost());
			if (this.ResearchItemDefinition.GetMoneyCost() > 0f)
			{
				TMP_Text costText2 = this._costText;
				costText2.text = string.Concat(new string[]
				{
					costText2.text,
					"  <color=#",
					global::Singleton<UIManager>.Instance.MoneyTextColor.ToHexString(),
					">",
					EconomyManager.GetFormattedMoneyString(this.ResearchItemDefinition.GetMoneyCost(), false)
				});
			}
		}
	}

	// Token: 0x06000470 RID: 1136 RVA: 0x00018348 File Offset: 0x00016548
	private void SetColors(Color color)
	{
		foreach (Graphic graphic in this._graphicsToChangeColor)
		{
			graphic.color = color;
		}
		foreach (UILineRenderer uilineRenderer in this._allLines)
		{
			uilineRenderer.color = color;
		}
	}

	// Token: 0x06000471 RID: 1137 RVA: 0x000183DC File Offset: 0x000165DC
	private void OnValidate()
	{
		if (this.ResearchItemDefinition == null)
		{
			return;
		}
		base.name = "Research Button - " + this.ResearchItemDefinition.GetName();
		this._researchNameText.text = this.ResearchItemDefinition.GetName();
		this._icon.sprite = this.ResearchItemDefinition.GetIcon();
		this._costText.text = string.Format("¤{0}", this.ResearchItemDefinition.GetResearchTicketCost());
		if (this.ResearchItemDefinition.GetMoneyCost() > 0f)
		{
			TMP_Text costText = this._costText;
			costText.text += string.Format("  ${0}", this.ResearchItemDefinition.GetMoneyCost());
		}
		RectTransform component = base.GetComponent<RectTransform>();
		if (component != null)
		{
			Vector2 anchoredPosition = component.anchoredPosition;
			float num = 10f;
			float num2 = 10f;
			anchoredPosition.x = Mathf.Round(anchoredPosition.x / num) * num;
			anchoredPosition.y = Mathf.Round(anchoredPosition.y / num2) * num2;
			component.anchoredPosition = anchoredPosition;
		}
	}

	// Token: 0x06000472 RID: 1138 RVA: 0x000184FC File Offset: 0x000166FC
	public Vector2 GetLineStartPoint()
	{
		return base.GetComponent<RectTransform>().TransformPoint(base.GetComponent<RectTransform>().rect.center);
	}

	// Token: 0x06000473 RID: 1139 RVA: 0x00018534 File Offset: 0x00016734
	public Vector2 GetLineEndPoint()
	{
		return base.GetComponent<RectTransform>().TransformPoint(base.GetComponent<RectTransform>().rect.center);
	}

	// Token: 0x06000474 RID: 1140 RVA: 0x0001856C File Offset: 0x0001676C
	public void DrawConnections(RectTransform contentContainer)
	{
		if (this.ResearchItemDefinition == null)
		{
			Debug.LogError("ResearchItemButton: " + base.name + " is missing ResearchItemDefinition");
			return;
		}
		if (this.ResearchItemDefinition.PrerequisiteResearch.Count == 0)
		{
			return;
		}
		foreach (UILineRenderer uilineRenderer in this._allLines.ToList<UILineRenderer>())
		{
			Object.Destroy(uilineRenderer.gameObject);
		}
		this._allLines.Clear();
		foreach (ResearchItemDefinition researchItemDefinition in this.ResearchItemDefinition.PrerequisiteResearch)
		{
			ResearchItemButton buttonForResearchDefinition = this._parentResearchTreeUI.GetButtonForResearchDefinition(researchItemDefinition);
			if (buttonForResearchDefinition)
			{
				UILineRenderer uilineRenderer2 = Object.Instantiate<UILineRenderer>(this._linePrefab, contentContainer);
				Color color = (this.ResearchItemDefinition.IsResearched() ? this._researchedColor : (this.ResearchItemDefinition.IsLocked() ? this._lockedColor : (this.ResearchItemDefinition.CanAfford() ? this._availableColor : this._tooExpensiveColor)));
				uilineRenderer2.color = color;
				RectTransform component = uilineRenderer2.GetComponent<RectTransform>();
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.one;
				component.offsetMin = Vector2.zero;
				component.offsetMax = Vector2.zero;
				component.anchoredPosition = Vector2.zero;
				component.SetAsFirstSibling();
				Vector3 vector = this.GetLineStartPoint();
				Vector3 vector2 = buttonForResearchDefinition.GetLineEndPoint();
				Vector2 vector3 = contentContainer.InverseTransformPoint(vector);
				Vector2 vector4 = contentContainer.InverseTransformPoint(vector2);
				uilineRenderer2.Points = new Vector2[] { vector3, vector4 };
				uilineRenderer2.SetAllDirty();
				this._allLines.Add(uilineRenderer2);
			}
		}
	}

	// Token: 0x0400050B RID: 1291
	public ResearchItemDefinition ResearchItemDefinition;

	// Token: 0x0400050C RID: 1292
	[SerializeField]
	private TMP_Text _researchNameText;

	// Token: 0x0400050D RID: 1293
	[SerializeField]
	private TMP_Text _costText;

	// Token: 0x0400050E RID: 1294
	[SerializeField]
	private Image _icon;

	// Token: 0x0400050F RID: 1295
	[SerializeField]
	private Color _availableColor;

	// Token: 0x04000510 RID: 1296
	[SerializeField]
	private Color _lockedColor;

	// Token: 0x04000511 RID: 1297
	[SerializeField]
	private Color _researchedColor;

	// Token: 0x04000512 RID: 1298
	[SerializeField]
	private Color _tooExpensiveColor;

	// Token: 0x04000513 RID: 1299
	[SerializeField]
	private List<Graphic> _graphicsToChangeColor;

	// Token: 0x04000514 RID: 1300
	[SerializeField]
	private UILineRenderer _linePrefab;

	// Token: 0x04000515 RID: 1301
	private ResearchTreeUI _parentResearchTreeUI;

	// Token: 0x04000516 RID: 1302
	private List<UILineRenderer> _allLines = new List<UILineRenderer>();
}
