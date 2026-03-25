using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPreviewRewardEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _rewardText;

	[SerializeField]
	private KeybindTokenText _descriptionText;

	[SerializeField]
	private Image _icon;

	public void Initialize(string itemName, Sprite sprite, string description)
	{
		_rewardText.text = "<u>" + itemName + "</u>";
		_descriptionText.SetText(description);
		_icon.sprite = sprite;
	}
}
