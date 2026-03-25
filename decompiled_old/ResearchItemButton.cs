using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ResearchItemButton : MonoBehaviour
{
	public ResearchItemDefinition ResearchItemDefinition;

	[SerializeField]
	private TMP_Text _researchNameText;

	[SerializeField]
	private TMP_Text _costText;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Color _availableColor;

	[SerializeField]
	private Color _lockedColor;

	[SerializeField]
	private Color _researchedColor;

	[SerializeField]
	private Color _tooExpensiveColor;

	[SerializeField]
	private List<Graphic> _graphicsToChangeColor;

	[SerializeField]
	private UILineRenderer _linePrefab;

	private ResearchTreeUI _parentResearchTreeUI;

	private List<UILineRenderer> _allLines = new List<UILineRenderer>();

	public void Initialize(ResearchTreeUI parentResearchTreeUI)
	{
		_parentResearchTreeUI = parentResearchTreeUI;
		RefreshDisplay();
	}

	private void OnEnable()
	{
		_icon.sprite = ResearchItemDefinition.GetIcon();
		RefreshDisplay();
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += OnResearchTicketsUpdated;
		Singleton<ResearchManager>.Instance.ResearchItemResearched += OnOtherItemResearched;
	}

	private void OnDisable()
	{
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= OnResearchTicketsUpdated;
		Singleton<ResearchManager>.Instance.ResearchItemResearched -= OnOtherItemResearched;
	}

	public void OnPressed()
	{
		_parentResearchTreeUI.PreviewResearch(ResearchItemDefinition);
	}

	[Obsolete]
	public void OLD_OnPressed()
	{
		if (!ResearchItemDefinition.IsLocked() && ResearchItemDefinition.CanAfford())
		{
			Singleton<ResearchManager>.Instance.ResearchItem(ResearchItemDefinition);
			RefreshDisplay();
		}
	}

	private void OnResearchTicketsUpdated(int amount)
	{
		if (!ResearchItemDefinition.IsLocked() && !Singleton<ResearchManager>.Instance.IsResearchItemCompleted(ResearchItemDefinition))
		{
			RefreshDisplay();
		}
	}

	private void OnOtherItemResearched(ResearchItemDefinition researchedItem)
	{
		if (researchedItem == ResearchItemDefinition || (ResearchItemDefinition.PrerequisiteResearch.Contains(researchedItem) && !ResearchItemDefinition.IsLocked()))
		{
			RefreshDisplay();
		}
	}

	public void RefreshDisplay()
	{
		if (ResearchItemDefinition.IsLocked() || (Singleton<DemoManager>.Instance.IsDemoVersion && ResearchItemDefinition.IsLockedInDemo))
		{
			SetColors(_lockedColor);
			_costText.text = $"¤{ResearchItemDefinition.GetResearchTicketCost()}";
			if (ResearchItemDefinition.GetMoneyCost() > 0f)
			{
				TMP_Text costText = _costText;
				costText.text = costText.text + "  " + EconomyManager.GetFormattedMoneyString(ResearchItemDefinition.GetMoneyCost(), includeDecimal: false);
			}
			return;
		}
		if (Singleton<ResearchManager>.Instance.IsResearchItemCompleted(ResearchItemDefinition))
		{
			SetColors(_researchedColor);
			_costText.text = "Researched";
			return;
		}
		if (ResearchItemDefinition.CanAfford())
		{
			SetColors(_availableColor);
		}
		else
		{
			SetColors(_tooExpensiveColor);
		}
		_costText.text = $"<color=#{Singleton<UIManager>.Instance.ResearchTicketsTextColor.ToHexString()}>¤{ResearchItemDefinition.GetResearchTicketCost()}";
		if (ResearchItemDefinition.GetMoneyCost() > 0f)
		{
			TMP_Text costText2 = _costText;
			costText2.text = costText2.text + "  <color=#" + Singleton<UIManager>.Instance.MoneyTextColor.ToHexString() + ">" + EconomyManager.GetFormattedMoneyString(ResearchItemDefinition.GetMoneyCost(), includeDecimal: false);
		}
	}

	private void SetColors(Color color)
	{
		foreach (Graphic item in _graphicsToChangeColor)
		{
			item.color = color;
		}
		foreach (UILineRenderer allLine in _allLines)
		{
			allLine.color = color;
		}
	}

	private void OnValidate()
	{
		if (!(ResearchItemDefinition == null))
		{
			base.name = "Research Button - " + ResearchItemDefinition.GetName();
			_researchNameText.text = ResearchItemDefinition.GetName();
			_icon.sprite = ResearchItemDefinition.GetIcon();
			_costText.text = $"¤{ResearchItemDefinition.GetResearchTicketCost()}";
			if (ResearchItemDefinition.GetMoneyCost() > 0f)
			{
				_costText.text += $"  ${ResearchItemDefinition.GetMoneyCost()}";
			}
			RectTransform component = GetComponent<RectTransform>();
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
	}

	public Vector2 GetLineStartPoint()
	{
		return GetComponent<RectTransform>().TransformPoint(GetComponent<RectTransform>().rect.center);
	}

	public Vector2 GetLineEndPoint()
	{
		return GetComponent<RectTransform>().TransformPoint(GetComponent<RectTransform>().rect.center);
	}

	public void DrawConnections(RectTransform contentContainer)
	{
		if (ResearchItemDefinition == null)
		{
			Debug.LogError("ResearchItemButton: " + base.name + " is missing ResearchItemDefinition");
		}
		else
		{
			if (ResearchItemDefinition.PrerequisiteResearch.Count == 0)
			{
				return;
			}
			foreach (UILineRenderer item in _allLines.ToList())
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			_allLines.Clear();
			foreach (ResearchItemDefinition item2 in ResearchItemDefinition.PrerequisiteResearch)
			{
				ResearchItemButton buttonForResearchDefinition = _parentResearchTreeUI.GetButtonForResearchDefinition(item2);
				if ((bool)buttonForResearchDefinition)
				{
					UILineRenderer uILineRenderer = UnityEngine.Object.Instantiate(_linePrefab, contentContainer);
					Color color = (ResearchItemDefinition.IsResearched() ? _researchedColor : (ResearchItemDefinition.IsLocked() ? _lockedColor : (ResearchItemDefinition.CanAfford() ? _availableColor : _tooExpensiveColor)));
					uILineRenderer.color = color;
					RectTransform component = uILineRenderer.GetComponent<RectTransform>();
					component.anchorMin = Vector2.zero;
					component.anchorMax = Vector2.one;
					component.offsetMin = Vector2.zero;
					component.offsetMax = Vector2.zero;
					component.anchoredPosition = Vector2.zero;
					component.SetAsFirstSibling();
					Vector3 position = GetLineStartPoint();
					Vector3 position2 = buttonForResearchDefinition.GetLineEndPoint();
					Vector2 vector = contentContainer.InverseTransformPoint(position);
					Vector2 vector2 = contentContainer.InverseTransformPoint(position2);
					uILineRenderer.Points = new Vector2[2] { vector, vector2 };
					uILineRenderer.SetAllDirty();
					_allLines.Add(uILineRenderer);
				}
			}
		}
	}
}
