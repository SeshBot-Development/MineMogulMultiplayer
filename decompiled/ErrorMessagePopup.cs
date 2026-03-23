using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessagePopup : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _messageText;

	[SerializeField]
	private TMP_Text _stackTraceText;

	[SerializeField]
	private TMP_Text _gameVersionText;

	[SerializeField]
	private TMP_Text _mapNameText;

	[SerializeField]
	private Toggle _dontShowAgainToggle;

	private void OnEnable()
	{
		_dontShowAgainToggle.isOn = Singleton<DebugManager>.Instance.DontShowErrorAgainThisSession;
	}

	private void OnDisable()
	{
		Singleton<DebugManager>.Instance.DontShowErrorAgainThisSession = _dontShowAgainToggle.isOn;
	}

	public void ShowErrorPopup(string message, string stackTrace)
	{
		base.gameObject.SetActive(value: true);
		_messageText.text = message;
		_stackTraceText.text = stackTrace;
		_gameVersionText.text = Singleton<VersionManager>.Instance.GetFormattedVersionText();
		_mapNameText.text = Singleton<LevelManager>.Instance.GetCurrentMapName();
	}
}
