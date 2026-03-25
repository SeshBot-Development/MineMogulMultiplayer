using System;
using TMPro;
using UnityEngine;

[Obsolete]
public class OldSaveLoadMenu : MonoBehaviour
{
	public TMP_Text SaveInfoText;

	public GameObject SaveOutOfDateWarning;

	public GameObject SaveDataIsCompatibleText;

	public GameObject GameSavedText;

	public TMP_Text LastSaveTimeText;

	public TMP_Text TempVersionNumberText;

	private void Awake()
	{
		CheckSaveFileVersion();
	}

	private void OnEnable()
	{
		GameSavedText.SetActive(value: false);
		TempVersionNumberText.text = Singleton<VersionManager>.Instance?.GetFormattedVersionText();
		if (Singleton<SavingLoadingManager>.Instance.LastSaveTime != 0f)
		{
			LastSaveTimeText.text = "Last save time: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		}
	}

	private void CheckSaveFileVersion()
	{
		SaveFileHeader legacySaveFileHeader = Singleton<SavingLoadingManager>.Instance.GetLegacySaveFileHeader();
		if (legacySaveFileHeader != null)
		{
			SaveInfoText.text = "Save File Version: " + legacySaveFileHeader.GameVersion;
			if (Singleton<SavingLoadingManager>.Instance.LastSaveTime == 0f)
			{
				LastSaveTimeText.text = "Save File date: " + legacySaveFileHeader.SaveTimestamp;
			}
			else
			{
				LastSaveTimeText.text = "Last save time: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
			}
			if (Singleton<SavingLoadingManager>.Instance.IsSaveFileCompatible(legacySaveFileHeader.SaveVersion))
			{
				SaveOutOfDateWarning.SetActive(value: false);
				SaveDataIsCompatibleText.SetActive(value: true);
			}
			else
			{
				SaveOutOfDateWarning.SetActive(value: true);
				SaveDataIsCompatibleText.SetActive(value: false);
			}
		}
		else
		{
			SaveInfoText.text = "No save file found";
			LastSaveTimeText.text = "";
			SaveOutOfDateWarning.SetActive(value: false);
			SaveDataIsCompatibleText.SetActive(value: false);
		}
	}

	public void OnSavePressed()
	{
		Singleton<SavingLoadingManager>.Instance.SaveGameWithActiveSaveFileName();
		SaveOutOfDateWarning.SetActive(value: false);
		SaveInfoText.text = "Save File " + Singleton<VersionManager>.Instance.GetFormattedVersionText();
		GameSavedText.SetActive(value: true);
		LastSaveTimeText.text = "Last save time: Now";
	}

	public void OnLoadPressed()
	{
	}

	public void OnLoadSceneThenLoadSavePressed()
	{
	}
}
