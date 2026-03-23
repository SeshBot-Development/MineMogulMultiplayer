using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200004E RID: 78
public class ErrorMessagePopup : MonoBehaviour
{
	// Token: 0x06000212 RID: 530 RVA: 0x0000A787 File Offset: 0x00008987
	private void OnEnable()
	{
		this._dontShowAgainToggle.isOn = Singleton<DebugManager>.Instance.DontShowErrorAgainThisSession;
	}

	// Token: 0x06000213 RID: 531 RVA: 0x0000A79E File Offset: 0x0000899E
	private void OnDisable()
	{
		Singleton<DebugManager>.Instance.DontShowErrorAgainThisSession = this._dontShowAgainToggle.isOn;
	}

	// Token: 0x06000214 RID: 532 RVA: 0x0000A7B8 File Offset: 0x000089B8
	public void ShowErrorPopup(string message, string stackTrace)
	{
		base.gameObject.SetActive(true);
		this._messageText.text = message;
		this._stackTraceText.text = stackTrace;
		this._gameVersionText.text = Singleton<VersionManager>.Instance.GetFormattedVersionText();
		this._mapNameText.text = Singleton<LevelManager>.Instance.GetCurrentMapName();
	}

	// Token: 0x040001F0 RID: 496
	[SerializeField]
	private TMP_Text _messageText;

	// Token: 0x040001F1 RID: 497
	[SerializeField]
	private TMP_Text _stackTraceText;

	// Token: 0x040001F2 RID: 498
	[SerializeField]
	private TMP_Text _gameVersionText;

	// Token: 0x040001F3 RID: 499
	[SerializeField]
	private TMP_Text _mapNameText;

	// Token: 0x040001F4 RID: 500
	[SerializeField]
	private Toggle _dontShowAgainToggle;
}
