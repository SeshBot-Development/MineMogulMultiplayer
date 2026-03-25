using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class QuestTreeItemButton : MonoBehaviour
{
	public QuestDefinition QuestDefinition;

	public Quest Quest;

	[SerializeField]
	private TMP_Text _questNameText;

	[SerializeField]
	private TMP_Text _questProgressText;

	[SerializeField]
	private GameObject _trackingOutline;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Color _availableTextColor;

	[SerializeField]
	private Color _unavailableTextColor;

	[SerializeField]
	private Color _activeColor;

	[SerializeField]
	private Color _completedColor;

	[SerializeField]
	private List<TMP_Text> _textsToChangeColor;

	[SerializeField]
	private List<Graphic> _graphicsToChangeColor;

	[SerializeField]
	private RectTransform _lineStartPoint;

	[SerializeField]
	private RectTransform _LineEndPoint;

	[SerializeField]
	private UILineRenderer _linePrefab;

	private QuestTreeUI _parentQuestTreeUI;

	private List<UILineRenderer> _allLines = new List<UILineRenderer>();

	public void Initialize(Quest quest, QuestTreeUI parentQuestTreeUI)
	{
		_parentQuestTreeUI = parentQuestTreeUI;
		Quest = quest;
		RefreshDisplay();
	}

	public void OnPressed()
	{
		_parentQuestTreeUI.PreviewQuest(Quest);
	}

	private void OnValidate()
	{
		if (!(QuestDefinition == null))
		{
			_questNameText.text = QuestDefinition.Name;
			base.name = "Quest Button - " + QuestDefinition.Name;
			if (QuestDefinition.OverrideQuestIcon != null)
			{
				_icon.sprite = QuestDefinition.OverrideQuestIcon;
			}
			RectTransform component = GetComponent<RectTransform>();
			if (component != null)
			{
				Vector2 anchoredPosition = component.anchoredPosition;
				float num = 20f;
				float num2 = 15f;
				anchoredPosition.x = Mathf.Round(anchoredPosition.x / num) * num;
				anchoredPosition.y = Mathf.Round(anchoredPosition.y / num2) * num2;
				component.anchoredPosition = anchoredPosition;
			}
		}
	}

	public void RefreshDisplay()
	{
		if (Quest != null)
		{
			_questNameText.text = Quest.Name;
			if (QuestDefinition.GetOverrideIcon() != null)
			{
				_icon.sprite = QuestDefinition.GetOverrideIcon();
			}
			else if (Quest.ShopItemsToUnlock.Count > 0)
			{
				_icon.sprite = Quest.ShopItemsToUnlock[0].GetIcon();
			}
			else
			{
				_icon.enabled = false;
			}
			if (Quest.IsCompleted())
			{
				_questProgressText.text = "Completed!";
				SetColors(isActive: false, isCompleted: true);
				SetOutlineColors(_completedColor);
			}
			else if (Quest.IsActive())
			{
				_questProgressText.text = "In Progress";
				SetColors(isActive: true, isCompleted: false);
				SetOutlineColors(_activeColor);
			}
			else if (Quest.IsLocked())
			{
				_questProgressText.text = "Locked";
				SetColors(isActive: false, isCompleted: false);
				SetOutlineColors(_unavailableTextColor);
			}
			else
			{
				_questProgressText.text = "Available";
				SetColors(isActive: true, isCompleted: false);
				SetOutlineColors(_availableTextColor);
			}
			bool active = Singleton<QuestManager>.Instance.ActiveQuests.Contains(Quest);
			_trackingOutline.SetActive(active);
		}
	}

	private void SetOutlineColors(Color color)
	{
		foreach (Graphic item in _graphicsToChangeColor)
		{
			item.color = color;
		}
	}

	private void SetColors(bool isActive, bool isCompleted)
	{
		Color color = (isCompleted ? _completedColor : (isActive ? _availableTextColor : _unavailableTextColor));
		foreach (TMP_Text item in _textsToChangeColor)
		{
			item.color = color;
		}
		foreach (UILineRenderer allLine in _allLines)
		{
			allLine.color = color;
		}
		if (isActive || isCompleted)
		{
			_icon.material = null;
		}
		else
		{
			_icon.material = Singleton<UIManager>.Instance.GrayscaleImageMaterial;
		}
	}

	public Vector2 GetLineStartPoint()
	{
		return _lineStartPoint.TransformPoint(_lineStartPoint.rect.center);
	}

	public Vector2 GetLineEndPoint()
	{
		return _LineEndPoint.TransformPoint(_LineEndPoint.rect.center);
	}

	public void DrawConnections(RectTransform contentContainer)
	{
		if (Quest == null)
		{
			Debug.LogError("Quest: " + QuestDefinition.name + " was not generated properly! Is it missing from the manager list?");
		}
		else
		{
			if (Quest.PrerequisiteQuests == null || Quest.PrerequisiteQuests.Count == 0)
			{
				return;
			}
			foreach (UILineRenderer item in _allLines.ToList())
			{
				Object.Destroy(item.gameObject);
			}
			_allLines.Clear();
			foreach (QuestDefinition prerequisiteQuest in Quest.PrerequisiteQuests)
			{
				QuestTreeItemButton buttonForQuestID = _parentQuestTreeUI.GetButtonForQuestID(prerequisiteQuest.QuestID);
				if ((bool)buttonForQuestID)
				{
					UILineRenderer uILineRenderer = Object.Instantiate(_linePrefab, contentContainer);
					uILineRenderer.color = (Quest.IsCompleted() ? _completedColor : (Quest.IsLocked() ? _unavailableTextColor : _availableTextColor));
					RectTransform component = uILineRenderer.GetComponent<RectTransform>();
					component.anchorMin = Vector2.zero;
					component.anchorMax = Vector2.one;
					component.offsetMin = Vector2.zero;
					component.offsetMax = Vector2.zero;
					component.anchoredPosition = Vector2.zero;
					component.SetAsFirstSibling();
					Vector3 position = GetLineStartPoint();
					Vector3 position2 = buttonForQuestID.GetLineEndPoint();
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
