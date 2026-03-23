using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

// Token: 0x0200009B RID: 155
public class QuestTreeItemButton : MonoBehaviour
{
	// Token: 0x06000426 RID: 1062 RVA: 0x000169EB File Offset: 0x00014BEB
	public void Initialize(Quest quest, QuestTreeUI parentQuestTreeUI)
	{
		this._parentQuestTreeUI = parentQuestTreeUI;
		this.Quest = quest;
		this.RefreshDisplay();
	}

	// Token: 0x06000427 RID: 1063 RVA: 0x00016A01 File Offset: 0x00014C01
	public void OnPressed()
	{
		this._parentQuestTreeUI.PreviewQuest(this.Quest);
	}

	// Token: 0x06000428 RID: 1064 RVA: 0x00016A14 File Offset: 0x00014C14
	private void OnValidate()
	{
		if (this.QuestDefinition == null)
		{
			return;
		}
		this._questNameText.text = this.QuestDefinition.Name;
		base.name = "Quest Button - " + this.QuestDefinition.Name;
		if (this.QuestDefinition.OverrideQuestIcon != null)
		{
			this._icon.sprite = this.QuestDefinition.OverrideQuestIcon;
		}
		RectTransform component = base.GetComponent<RectTransform>();
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

	// Token: 0x06000429 RID: 1065 RVA: 0x00016AE0 File Offset: 0x00014CE0
	public void RefreshDisplay()
	{
		if (this.Quest == null)
		{
			return;
		}
		this._questNameText.text = this.Quest.Name;
		if (this.QuestDefinition.GetOverrideIcon() != null)
		{
			this._icon.sprite = this.QuestDefinition.GetOverrideIcon();
		}
		else if (this.Quest.ShopItemsToUnlock.Count > 0)
		{
			this._icon.sprite = this.Quest.ShopItemsToUnlock[0].GetIcon();
		}
		else
		{
			this._icon.enabled = false;
		}
		if (this.Quest.IsCompleted())
		{
			this._questProgressText.text = "Completed!";
			this.SetColors(false, true);
			this.SetOutlineColors(this._completedColor);
		}
		else if (this.Quest.IsActive())
		{
			this._questProgressText.text = "In Progress";
			this.SetColors(true, false);
			this.SetOutlineColors(this._activeColor);
		}
		else if (this.Quest.IsLocked())
		{
			this._questProgressText.text = "Locked";
			this.SetColors(false, false);
			this.SetOutlineColors(this._unavailableTextColor);
		}
		else
		{
			this._questProgressText.text = "Available";
			this.SetColors(true, false);
			this.SetOutlineColors(this._availableTextColor);
		}
		bool flag = Singleton<QuestManager>.Instance.ActiveQuests.Contains(this.Quest);
		this._trackingOutline.SetActive(flag);
	}

	// Token: 0x0600042A RID: 1066 RVA: 0x00016C5C File Offset: 0x00014E5C
	private void SetOutlineColors(Color color)
	{
		foreach (Graphic graphic in this._graphicsToChangeColor)
		{
			graphic.color = color;
		}
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x00016CB0 File Offset: 0x00014EB0
	private void SetColors(bool isActive, bool isCompleted)
	{
		Color color = (isCompleted ? this._completedColor : (isActive ? this._availableTextColor : this._unavailableTextColor));
		foreach (TMP_Text tmp_Text in this._textsToChangeColor)
		{
			tmp_Text.color = color;
		}
		foreach (UILineRenderer uilineRenderer in this._allLines)
		{
			uilineRenderer.color = color;
		}
		if (isActive || isCompleted)
		{
			this._icon.material = null;
			return;
		}
		this._icon.material = Singleton<UIManager>.Instance.GrayscaleImageMaterial;
	}

	// Token: 0x0600042C RID: 1068 RVA: 0x00016D88 File Offset: 0x00014F88
	public Vector2 GetLineStartPoint()
	{
		return this._lineStartPoint.TransformPoint(this._lineStartPoint.rect.center);
	}

	// Token: 0x0600042D RID: 1069 RVA: 0x00016DC0 File Offset: 0x00014FC0
	public Vector2 GetLineEndPoint()
	{
		return this._LineEndPoint.TransformPoint(this._LineEndPoint.rect.center);
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x00016DF8 File Offset: 0x00014FF8
	public void DrawConnections(RectTransform contentContainer)
	{
		if (this.Quest == null)
		{
			Debug.LogError("Quest: " + this.QuestDefinition.name + " was not generated properly! Is it missing from the manager list?");
			return;
		}
		if (this.Quest.PrerequisiteQuests == null || this.Quest.PrerequisiteQuests.Count == 0)
		{
			return;
		}
		foreach (UILineRenderer uilineRenderer in this._allLines.ToList<UILineRenderer>())
		{
			Object.Destroy(uilineRenderer.gameObject);
		}
		this._allLines.Clear();
		foreach (QuestDefinition questDefinition in this.Quest.PrerequisiteQuests)
		{
			QuestTreeItemButton buttonForQuestID = this._parentQuestTreeUI.GetButtonForQuestID(questDefinition.QuestID);
			if (buttonForQuestID)
			{
				UILineRenderer uilineRenderer2 = Object.Instantiate<UILineRenderer>(this._linePrefab, contentContainer);
				uilineRenderer2.color = (this.Quest.IsCompleted() ? this._completedColor : (this.Quest.IsLocked() ? this._unavailableTextColor : this._availableTextColor));
				RectTransform component = uilineRenderer2.GetComponent<RectTransform>();
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.one;
				component.offsetMin = Vector2.zero;
				component.offsetMax = Vector2.zero;
				component.anchoredPosition = Vector2.zero;
				component.SetAsFirstSibling();
				Vector3 vector = this.GetLineStartPoint();
				Vector3 vector2 = buttonForQuestID.GetLineEndPoint();
				Vector2 vector3 = contentContainer.InverseTransformPoint(vector);
				Vector2 vector4 = contentContainer.InverseTransformPoint(vector2);
				uilineRenderer2.Points = new Vector2[] { vector3, vector4 };
				uilineRenderer2.SetAllDirty();
				this._allLines.Add(uilineRenderer2);
			}
		}
	}

	// Token: 0x040004BD RID: 1213
	public QuestDefinition QuestDefinition;

	// Token: 0x040004BE RID: 1214
	public Quest Quest;

	// Token: 0x040004BF RID: 1215
	[SerializeField]
	private TMP_Text _questNameText;

	// Token: 0x040004C0 RID: 1216
	[SerializeField]
	private TMP_Text _questProgressText;

	// Token: 0x040004C1 RID: 1217
	[SerializeField]
	private GameObject _trackingOutline;

	// Token: 0x040004C2 RID: 1218
	[SerializeField]
	private Image _icon;

	// Token: 0x040004C3 RID: 1219
	[SerializeField]
	private Color _availableTextColor;

	// Token: 0x040004C4 RID: 1220
	[SerializeField]
	private Color _unavailableTextColor;

	// Token: 0x040004C5 RID: 1221
	[SerializeField]
	private Color _activeColor;

	// Token: 0x040004C6 RID: 1222
	[SerializeField]
	private Color _completedColor;

	// Token: 0x040004C7 RID: 1223
	[SerializeField]
	private List<TMP_Text> _textsToChangeColor;

	// Token: 0x040004C8 RID: 1224
	[SerializeField]
	private List<Graphic> _graphicsToChangeColor;

	// Token: 0x040004C9 RID: 1225
	[SerializeField]
	private RectTransform _lineStartPoint;

	// Token: 0x040004CA RID: 1226
	[SerializeField]
	private RectTransform _LineEndPoint;

	// Token: 0x040004CB RID: 1227
	[SerializeField]
	private UILineRenderer _linePrefab;

	// Token: 0x040004CC RID: 1228
	private QuestTreeUI _parentQuestTreeUI;

	// Token: 0x040004CD RID: 1229
	private List<UILineRenderer> _allLines = new List<UILineRenderer>();
}
