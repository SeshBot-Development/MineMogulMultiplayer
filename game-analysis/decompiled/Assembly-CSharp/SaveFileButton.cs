using System;
using System.IO;
using TMPro;
using UnityEngine;

// Token: 0x020000BE RID: 190
public class SaveFileButton : MonoBehaviour
{
	// Token: 0x06000520 RID: 1312 RVA: 0x0001AC08 File Offset: 0x00018E08
	public void Initialize(string saveFilePath, SaveFileHeader saveFileHeader, LoadingMenu loadingMenu)
	{
		this._loadingMenu = loadingMenu;
		this._saveFilePath = saveFilePath;
		this._saveFileHeader = saveFileHeader;
		this._saveFileNameText.text = Path.GetFileNameWithoutExtension(this._saveFilePath);
		this._saveVersionNumberText.text = this._saveFileHeader.GameVersion;
		this._lastSaveTimeText.text = TimeUtil.GetDisplaySaveTime(this._saveFileHeader.SaveTimestamp);
	}

	// Token: 0x06000521 RID: 1313 RVA: 0x0001AC71 File Offset: 0x00018E71
	public void OnSelected()
	{
		this._loadingMenu.SelectSaveFile(this._saveFilePath, this._saveFileHeader);
	}

	// Token: 0x0400064E RID: 1614
	[SerializeField]
	private TMP_Text _saveFileNameText;

	// Token: 0x0400064F RID: 1615
	[SerializeField]
	private TMP_Text _saveVersionNumberText;

	// Token: 0x04000650 RID: 1616
	[SerializeField]
	private TMP_Text _lastSaveTimeText;

	// Token: 0x04000651 RID: 1617
	private string _saveFilePath;

	// Token: 0x04000652 RID: 1618
	private SaveFileHeader _saveFileHeader;

	// Token: 0x04000653 RID: 1619
	private LoadingMenu _loadingMenu;
}
