using System;
using TMPro;
using UnityEngine;

// Token: 0x02000058 RID: 88
public class InfoMessagePopup : MonoBehaviour
{
	// Token: 0x06000235 RID: 565 RVA: 0x0000AFAD File Offset: 0x000091AD
	public void ShowInfoPopup(string header, string message)
	{
		base.gameObject.SetActive(true);
		this._headertext.text = header;
		this._messageText.text = message;
		this._gameVersionText.text = Singleton<VersionManager>.Instance.GetVersionTextWithoutLabel();
	}

	// Token: 0x04000209 RID: 521
	[SerializeField]
	private TMP_Text _messageText;

	// Token: 0x0400020A RID: 522
	[SerializeField]
	private TMP_Text _headertext;

	// Token: 0x0400020B RID: 523
	[SerializeField]
	private TMP_Text _gameVersionText;
}
