using TMPro;
using UnityEngine;

public class InfoMessagePopup : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _messageText;

	[SerializeField]
	private TMP_Text _headertext;

	[SerializeField]
	private TMP_Text _gameVersionText;

	public void ShowInfoPopup(string header, string message)
	{
		base.gameObject.SetActive(value: true);
		_headertext.text = header;
		_messageText.text = message;
		_gameVersionText.text = Singleton<VersionManager>.Instance.GetVersionTextWithoutLabel();
	}
}
