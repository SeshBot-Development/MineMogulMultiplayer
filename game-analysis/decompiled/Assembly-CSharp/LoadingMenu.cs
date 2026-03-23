using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Token: 0x020000BA RID: 186
public class LoadingMenu : MonoBehaviour
{
	// Token: 0x060004FF RID: 1279 RVA: 0x0001A2A9 File Offset: 0x000184A9
	private void OnEnable()
	{
		TMP_Text versionNumberText = this._versionNumberText;
		VersionManager instance = Singleton<VersionManager>.Instance;
		versionNumberText.text = ((instance != null) ? instance.GetFormattedVersionText() : null);
		this.RefreshSaveFileList();
	}

	// Token: 0x06000500 RID: 1280 RVA: 0x0001A2D0 File Offset: 0x000184D0
	public void RefreshSaveFileList()
	{
		this._noFileSelectedPanel.SetActive(true);
		this._selectedFileInfoPanel.SetActive(false);
		this._mainInfoPanel.SetActive(true);
		this._settingsPanel.SetActive(false);
		this._confirmDeletePanel.SetActive(false);
		this._confirmLoadDemoSavePanel.SetActive(false);
		foreach (object obj in this._saveFilesListContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this._saveFileButtons.Clear();
		List<SaveFileHeaderFileCombo> allSaveFileHeaderFileCombos = SavingLoadingManager.GetAllSaveFileHeaderFileCombos();
		if (allSaveFileHeaderFileCombos.Count == 0)
		{
			Debug.Log("Couldn't find any save files.");
			Object.Instantiate<GameObject>(this._noSaveFilesFoundButtonPrefab, this._saveFilesListContainer);
			return;
		}
		foreach (SaveFileHeaderFileCombo saveFileHeaderFileCombo in allSaveFileHeaderFileCombos.OrderByDescending((SaveFileHeaderFileCombo x) => x.SaveFileHeader.SaveTimestamp))
		{
			Object.Instantiate<SaveFileButton>(this._saveFileButtonPrefab, this._saveFilesListContainer).Initialize(saveFileHeaderFileCombo.FullFilePath, saveFileHeaderFileCombo.SaveFileHeader, this);
		}
	}

	// Token: 0x06000501 RID: 1281 RVA: 0x0001A424 File Offset: 0x00018624
	public void SelectSaveFile(string selectedSaveFilePath, SaveFileHeader selectedSaveFileHeader)
	{
		this._selectedSaveFileFullPath = selectedSaveFilePath;
		this._SelectedSaveFileHeader = selectedSaveFileHeader;
		this.RefreshSelectedFileInfo();
	}

	// Token: 0x06000502 RID: 1282 RVA: 0x0001A43C File Offset: 0x0001863C
	public void RefreshSelectedFileInfo()
	{
		this._noFileSelectedPanel.SetActive(false);
		this._selectedFileInfoPanel.SetActive(true);
		this._mainInfoPanel.SetActive(true);
		this._settingsPanel.SetActive(false);
		this._confirmDeletePanel.SetActive(false);
		this._confirmLoadDemoSavePanel.SetActive(false);
		this._saveFileNameText.text = Path.GetFileNameWithoutExtension(this._selectedSaveFileFullPath);
		this._saveVersionNumberText.text = this._SelectedSaveFileHeader.GameVersion;
		this._lastSaveTimeText.text = (this._lastSaveTimeText.text = TimeUtil.GetDisplaySaveTime(this._SelectedSaveFileHeader.SaveTimestamp));
		TMP_Text levelNameText = this._levelNameText;
		LevelInfo levelByID = Singleton<LevelManager>.Instance.GetLevelByID(this._SelectedSaveFileHeader.LevelID);
		levelNameText.text = ((levelByID != null) ? levelByID.DisplayName : null) ?? ("<color=red>Missing: " + this._SelectedSaveFileHeader.LevelID + "</color>");
		this._moneyText.text = string.Format("${0:#,##0.00}", this._SelectedSaveFileHeader.Money);
		this._playTimeText.text = SavingLoadingManager.GetFormattedPlaytime(this._SelectedSaveFileHeader.TotalPlayTimeSeconds);
		base.StartCoroutine(this.SetScreenshotForJson(this._selectedSaveFileFullPath));
		if (Singleton<SavingLoadingManager>.Instance.IsSaveFileCompatible(this._SelectedSaveFileHeader.SaveVersion))
		{
			this._saveOutOfDateWarning.SetActive(false);
			this._saveDataIsCompatibleThing.SetActive(true);
		}
		else
		{
			if (this._SelectedSaveFileHeader.SaveVersion < 15)
			{
				this._saveOutOfDateText.text = this._saveFileOutOfDateString;
			}
			else
			{
				this._saveOutOfDateText.text = this._saveFileTooNewString;
			}
			this._saveOutOfDateWarning.SetActive(true);
			this._saveDataIsCompatibleThing.SetActive(false);
		}
		this._debugQuickLoadButton.SetActive(!(Singleton<DebugManager>.Instance == null) && Singleton<DebugManager>.Instance.DevModeEnabled);
	}

	// Token: 0x06000503 RID: 1283 RVA: 0x0001A624 File Offset: 0x00018824
	public void OnLoadGamePressed()
	{
		LevelInfo levelByID = Singleton<LevelManager>.Instance.GetLevelByID(this._SelectedSaveFileHeader.LevelID);
		if (levelByID.LevelID == "DemoCave")
		{
			this._mainInfoPanel.SetActive(false);
			this._settingsPanel.SetActive(false);
			this._confirmDeletePanel.SetActive(false);
			this._confirmLoadDemoSavePanel.SetActive(true);
			return;
		}
		Singleton<SavingLoadingManager>.Instance.LoadSceneThenLoadSave(this._selectedSaveFileFullPath, levelByID.SceneName);
	}

	// Token: 0x06000504 RID: 1284 RVA: 0x0001A6A0 File Offset: 0x000188A0
	public void OnConfirmLoadGamePressed()
	{
		LevelInfo levelByID = Singleton<LevelManager>.Instance.GetLevelByID(this._SelectedSaveFileHeader.LevelID);
		string text = ((levelByID != null) ? levelByID.SceneName : null);
		Singleton<SavingLoadingManager>.Instance.LoadSceneThenLoadSave(this._selectedSaveFileFullPath, text);
	}

	// Token: 0x06000505 RID: 1285 RVA: 0x0001A6E0 File Offset: 0x000188E0
	public void OnQuickLoadPressed()
	{
		Singleton<SavingLoadingManager>.Instance.LoadGame(this._selectedSaveFileFullPath);
	}

	// Token: 0x06000506 RID: 1286 RVA: 0x0001A6F2 File Offset: 0x000188F2
	public void OnFileSettingsPressed()
	{
		this._mainInfoPanel.SetActive(false);
		this._settingsPanel.SetActive(true);
		this._confirmDeletePanel.SetActive(false);
		this._confirmLoadDemoSavePanel.SetActive(false);
	}

	// Token: 0x06000507 RID: 1287 RVA: 0x0001A724 File Offset: 0x00018924
	public void OnBackFromSettingsPressed()
	{
		this._confirmDeletePanel.SetActive(false);
		this._settingsPanel.SetActive(false);
		this._mainInfoPanel.SetActive(true);
		this._confirmLoadDemoSavePanel.SetActive(false);
	}

	// Token: 0x06000508 RID: 1288 RVA: 0x0001A756 File Offset: 0x00018956
	public void OnConfirmDeletePressed()
	{
		Singleton<SavingLoadingManager>.Instance.DeleteSaveFile(this._selectedSaveFileFullPath);
		this.RefreshSaveFileList();
	}

	// Token: 0x06000509 RID: 1289 RVA: 0x0001A76E File Offset: 0x0001896E
	public void OnOpenDeletePanelPressed()
	{
		this._confirmDeletePanel.SetActive(true);
		this._settingsPanel.SetActive(false);
		this._mainInfoPanel.SetActive(false);
		this._confirmLoadDemoSavePanel.SetActive(false);
	}

	// Token: 0x0600050A RID: 1290 RVA: 0x0001A7A0 File Offset: 0x000189A0
	public IEnumerator SetScreenshotForJson(string fullFilePath)
	{
		if (this._fileScreenshotImage == null || string.IsNullOrWhiteSpace(fullFilePath))
		{
			yield break;
		}
		string imgPath = LoadingMenu.FindSiblingImage(fullFilePath);
		if (string.IsNullOrEmpty(imgPath))
		{
			this._fileScreenshotImage.texture = this._defaultSaveFileImage;
			this._aspectRatioFitter.aspectRatio = 1.777778f;
			yield break;
		}
		string absoluteUri = new Uri(imgPath).AbsoluteUri;
		using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(absoluteUri, true))
		{
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(string.Concat(new string[] { "Failed to load screenshot: ", req.error, " (", imgPath, ")" }));
				this._fileScreenshotImage.texture = this._defaultSaveFileImage;
				this._aspectRatioFitter.aspectRatio = 1.777778f;
				yield break;
			}
			Texture2D content = DownloadHandlerTexture.GetContent(req);
			if (this._selectedSaveFileFullPath != fullFilePath)
			{
				Object.Destroy(content);
				yield break;
			}
			if (this._loadedRuntimeTexture != null)
			{
				Object.Destroy(this._loadedRuntimeTexture);
			}
			this._loadedRuntimeTexture = content;
			this._fileScreenshotImage.texture = content;
			if (content != null)
			{
				float num = (float)content.width;
				float num2 = (float)content.height;
				if (num > 0f && num2 > 0f)
				{
					this._aspectRatioFitter.aspectRatio = Mathf.Clamp(num / num2, 0.01f, 100f);
				}
			}
		}
		UnityWebRequest req = null;
		yield break;
		yield break;
	}

	// Token: 0x0600050B RID: 1291 RVA: 0x0001A7B8 File Offset: 0x000189B8
	private static string FindSiblingImage(string jsonPath)
	{
		if (!Path.IsPathRooted(jsonPath))
		{
			return null;
		}
		string[] array = new string[] { ".png", ".jpg", ".jpeg" };
		foreach (string text in array)
		{
			string text2 = Path.ChangeExtension(jsonPath, text);
			if (File.Exists(text2))
			{
				return text2;
			}
		}
		string directoryName = Path.GetDirectoryName(jsonPath);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(jsonPath);
		if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
		{
			return null;
		}
		HashSet<string> hashSet = new HashSet<string>(array, StringComparer.OrdinalIgnoreCase);
		foreach (string text3 in Directory.EnumerateFiles(directoryName, fileNameWithoutExtension + ".*", SearchOption.TopDirectoryOnly))
		{
			if (hashSet.Contains(Path.GetExtension(text3)))
			{
				return text3;
			}
		}
		return null;
	}

	// Token: 0x0600050C RID: 1292 RVA: 0x0001A8B4 File Offset: 0x00018AB4
	private void OnDisable()
	{
		if (this._loadedRuntimeTexture != null)
		{
			Object.Destroy(this._loadedRuntimeTexture);
		}
	}

	// Token: 0x04000625 RID: 1573
	[Header("Main Stuff")]
	[SerializeField]
	private Transform _saveFilesListContainer;

	// Token: 0x04000626 RID: 1574
	[SerializeField]
	private SaveFileButton _saveFileButtonPrefab;

	// Token: 0x04000627 RID: 1575
	[SerializeField]
	private GameObject _noSaveFilesFoundButtonPrefab;

	// Token: 0x04000628 RID: 1576
	[SerializeField]
	private TMP_Text _versionNumberText;

	// Token: 0x04000629 RID: 1577
	[SerializeField]
	private GameObject _mainInfoPanel;

	// Token: 0x0400062A RID: 1578
	[SerializeField]
	private GameObject _settingsPanel;

	// Token: 0x0400062B RID: 1579
	[SerializeField]
	private GameObject _confirmDeletePanel;

	// Token: 0x0400062C RID: 1580
	[SerializeField]
	private GameObject _confirmLoadDemoSavePanel;

	// Token: 0x0400062D RID: 1581
	[Header("Save File Info Panel")]
	[SerializeField]
	private GameObject _noFileSelectedPanel;

	// Token: 0x0400062E RID: 1582
	[SerializeField]
	private GameObject _selectedFileInfoPanel;

	// Token: 0x0400062F RID: 1583
	[SerializeField]
	private TMP_Text _saveFileNameText;

	// Token: 0x04000630 RID: 1584
	[SerializeField]
	private TMP_Text _saveVersionNumberText;

	// Token: 0x04000631 RID: 1585
	[SerializeField]
	private TMP_Text _lastSaveTimeText;

	// Token: 0x04000632 RID: 1586
	[SerializeField]
	private TMP_Text _levelNameText;

	// Token: 0x04000633 RID: 1587
	[SerializeField]
	private TMP_Text _moneyText;

	// Token: 0x04000634 RID: 1588
	[SerializeField]
	private TMP_Text _playTimeText;

	// Token: 0x04000635 RID: 1589
	[SerializeField]
	private RawImage _fileScreenshotImage;

	// Token: 0x04000636 RID: 1590
	[SerializeField]
	private GameObject _debugQuickLoadButton;

	// Token: 0x04000637 RID: 1591
	[SerializeField]
	private AspectRatioFitter _aspectRatioFitter;

	// Token: 0x04000638 RID: 1592
	[SerializeField]
	private Texture2D _defaultSaveFileImage;

	// Token: 0x04000639 RID: 1593
	[SerializeField]
	private GameObject _saveDataIsCompatibleThing;

	// Token: 0x0400063A RID: 1594
	[SerializeField]
	private GameObject _saveOutOfDateWarning;

	// Token: 0x0400063B RID: 1595
	[SerializeField]
	private TMP_Text _saveOutOfDateText;

	// Token: 0x0400063C RID: 1596
	[TextArea]
	[SerializeField]
	private string _saveFileOutOfDateString;

	// Token: 0x0400063D RID: 1597
	[TextArea]
	[SerializeField]
	private string _saveFileTooNewString;

	// Token: 0x0400063E RID: 1598
	private string _selectedSaveFileFullPath;

	// Token: 0x0400063F RID: 1599
	private SaveFileHeader _SelectedSaveFileHeader;

	// Token: 0x04000640 RID: 1600
	private List<SaveFileButton> _saveFileButtons = new List<SaveFileButton>();

	// Token: 0x04000641 RID: 1601
	private Texture2D _loadedRuntimeTexture;
}
