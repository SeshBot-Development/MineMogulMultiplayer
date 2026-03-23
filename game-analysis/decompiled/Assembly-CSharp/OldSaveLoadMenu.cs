using System;
using TMPro;
using UnityEngine;

// Token: 0x020000BD RID: 189
[Obsolete]
public class OldSaveLoadMenu : MonoBehaviour
{
	// Token: 0x06000519 RID: 1305 RVA: 0x0001AA27 File Offset: 0x00018C27
	private void Awake()
	{
		this.CheckSaveFileVersion();
	}

	// Token: 0x0600051A RID: 1306 RVA: 0x0001AA30 File Offset: 0x00018C30
	private void OnEnable()
	{
		this.GameSavedText.SetActive(false);
		TMP_Text tempVersionNumberText = this.TempVersionNumberText;
		VersionManager instance = Singleton<VersionManager>.Instance;
		tempVersionNumberText.text = ((instance != null) ? instance.GetFormattedVersionText() : null);
		if (Singleton<SavingLoadingManager>.Instance.LastSaveTime != 0f)
		{
			this.LastSaveTimeText.text = "Last save time: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		}
	}

	// Token: 0x0600051B RID: 1307 RVA: 0x0001AA98 File Offset: 0x00018C98
	private void CheckSaveFileVersion()
	{
		SaveFileHeader legacySaveFileHeader = Singleton<SavingLoadingManager>.Instance.GetLegacySaveFileHeader();
		if (legacySaveFileHeader == null)
		{
			this.SaveInfoText.text = "No save file found";
			this.LastSaveTimeText.text = "";
			this.SaveOutOfDateWarning.SetActive(false);
			this.SaveDataIsCompatibleText.SetActive(false);
			return;
		}
		this.SaveInfoText.text = "Save File Version: " + legacySaveFileHeader.GameVersion;
		if (Singleton<SavingLoadingManager>.Instance.LastSaveTime == 0f)
		{
			this.LastSaveTimeText.text = "Save File date: " + legacySaveFileHeader.SaveTimestamp;
		}
		else
		{
			this.LastSaveTimeText.text = "Last save time: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		}
		if (Singleton<SavingLoadingManager>.Instance.IsSaveFileCompatible(legacySaveFileHeader.SaveVersion))
		{
			this.SaveOutOfDateWarning.SetActive(false);
			this.SaveDataIsCompatibleText.SetActive(true);
			return;
		}
		this.SaveOutOfDateWarning.SetActive(true);
		this.SaveDataIsCompatibleText.SetActive(false);
	}

	// Token: 0x0600051C RID: 1308 RVA: 0x0001AB9C File Offset: 0x00018D9C
	public void OnSavePressed()
	{
		Singleton<SavingLoadingManager>.Instance.SaveGameWithActiveSaveFileName();
		this.SaveOutOfDateWarning.SetActive(false);
		this.SaveInfoText.text = "Save File " + Singleton<VersionManager>.Instance.GetFormattedVersionText();
		this.GameSavedText.SetActive(true);
		this.LastSaveTimeText.text = "Last save time: Now";
	}

	// Token: 0x0600051D RID: 1309 RVA: 0x0001ABFA File Offset: 0x00018DFA
	public void OnLoadPressed()
	{
	}

	// Token: 0x0600051E RID: 1310 RVA: 0x0001ABFC File Offset: 0x00018DFC
	public void OnLoadSceneThenLoadSavePressed()
	{
	}

	// Token: 0x04000648 RID: 1608
	public TMP_Text SaveInfoText;

	// Token: 0x04000649 RID: 1609
	public GameObject SaveOutOfDateWarning;

	// Token: 0x0400064A RID: 1610
	public GameObject SaveDataIsCompatibleText;

	// Token: 0x0400064B RID: 1611
	public GameObject GameSavedText;

	// Token: 0x0400064C RID: 1612
	public TMP_Text LastSaveTimeText;

	// Token: 0x0400064D RID: 1613
	public TMP_Text TempVersionNumberText;
}
