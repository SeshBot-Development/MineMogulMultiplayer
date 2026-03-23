using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200009A RID: 154
public class QuestPreviewRewardEntry : MonoBehaviour
{
	// Token: 0x06000424 RID: 1060 RVA: 0x000169AE File Offset: 0x00014BAE
	public void Initialize(string itemName, Sprite sprite, string description)
	{
		this._rewardText.text = "<u>" + itemName + "</u>";
		this._descriptionText.SetText(description);
		this._icon.sprite = sprite;
	}

	// Token: 0x040004BA RID: 1210
	[SerializeField]
	private TMP_Text _rewardText;

	// Token: 0x040004BB RID: 1211
	[SerializeField]
	private KeybindTokenText _descriptionText;

	// Token: 0x040004BC RID: 1212
	[SerializeField]
	private Image _icon;
}
