using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
	[Header("Main Stuff")]
	[SerializeField]
	private Transform _saveFilesListContainer;

	[SerializeField]
	private SaveFileButton _saveFileButtonPrefab;

	[SerializeField]
	private GameObject _noSaveFilesFoundButtonPrefab;

	[SerializeField]
	private TMP_Text _versionNumberText;

	[SerializeField]
	private GameObject _mainInfoPanel;

	[SerializeField]
	private GameObject _settingsPanel;

	[SerializeField]
	private GameObject _confirmDeletePanel;

	[SerializeField]
	private GameObject _confirmLoadDemoSavePanel;

	[Header("Save File Info Panel")]
	[SerializeField]
	private GameObject _noFileSelectedPanel;

	[SerializeField]
	private GameObject _selectedFileInfoPanel;

	[SerializeField]
	private TMP_Text _saveFileNameText;

	[SerializeField]
	private TMP_Text _saveVersionNumberText;

	[SerializeField]
	private TMP_Text _lastSaveTimeText;

	[SerializeField]
	private TMP_Text _levelNameText;

	[SerializeField]
	private TMP_Text _gamemodeNameText;

	[SerializeField]
	private TMP_Text _moneyText;

	[SerializeField]
	private TMP_Text _playTimeText;

	[SerializeField]
	private RawImage _fileScreenshotImage;

	[SerializeField]
	private GameObject _debugQuickLoadButton;

	[SerializeField]
	private AspectRatioFitter _aspectRatioFitter;

	[SerializeField]
	private Texture2D _defaultSaveFileImage;

	[SerializeField]
	private Button _loadGameButton;

	[SerializeField]
	private GameObject _saveDataIsCompatibleThing;

	[SerializeField]
	private GameObject _saveOutOfDateWarning;

	[SerializeField]
	private TMP_Text _saveOutOfDateText;

	[TextArea]
	[SerializeField]
	private string _saveFileOutOfDateString;

	[TextArea]
	[SerializeField]
	private string _saveFileTooNewString;

	[TextArea]
	[SerializeField]
	private string _saveFileNotDemoVersionString;

	private string _selectedSaveFileFullPath;

	private SaveFileHeader _SelectedSaveFileHeader;

	private List<SaveFileButton> _saveFileButtons = new List<SaveFileButton>();

	private Texture2D _loadedRuntimeTexture;

	private void OnEnable()
	{
		_versionNumberText.text = Singleton<VersionManager>.Instance?.GetFormattedVersionText();
		RefreshSaveFileList();
	}

	public void RefreshSaveFileList()
	{
		_noFileSelectedPanel.SetActive(value: true);
		_selectedFileInfoPanel.SetActive(value: false);
		_mainInfoPanel.SetActive(value: true);
		_settingsPanel.SetActive(value: false);
		_confirmDeletePanel.SetActive(value: false);
		_confirmLoadDemoSavePanel.SetActive(value: false);
		foreach (Transform item in _saveFilesListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		_saveFileButtons.Clear();
		List<SaveFileHeaderFileCombo> allSaveFileHeaderFileCombos = SavingLoadingManager.GetAllSaveFileHeaderFileCombos();
		if (allSaveFileHeaderFileCombos.Count == 0)
		{
			Debug.Log("Couldn't find any save files.");
			UnityEngine.Object.Instantiate(_noSaveFilesFoundButtonPrefab, _saveFilesListContainer);
			return;
		}
		foreach (SaveFileHeaderFileCombo item2 in allSaveFileHeaderFileCombos.OrderByDescending((SaveFileHeaderFileCombo x) => x.SaveFileHeader.SaveTimestamp))
		{
			UnityEngine.Object.Instantiate(_saveFileButtonPrefab, _saveFilesListContainer).Initialize(item2.FullFilePath, item2.SaveFileHeader, this);
		}
	}

	public void SelectSaveFile(string selectedSaveFilePath, SaveFileHeader selectedSaveFileHeader)
	{
		_selectedSaveFileFullPath = selectedSaveFilePath;
		_SelectedSaveFileHeader = selectedSaveFileHeader;
		RefreshSelectedFileInfo();
	}

	public void RefreshSelectedFileInfo()
	{
		_noFileSelectedPanel.SetActive(value: false);
		_selectedFileInfoPanel.SetActive(value: true);
		_mainInfoPanel.SetActive(value: true);
		_settingsPanel.SetActive(value: false);
		_confirmDeletePanel.SetActive(value: false);
		_confirmLoadDemoSavePanel.SetActive(value: false);
		_saveFileNameText.text = Path.GetFileNameWithoutExtension(_selectedSaveFileFullPath);
		_saveVersionNumberText.text = _SelectedSaveFileHeader.GameVersion;
		TMP_Text lastSaveTimeText = _lastSaveTimeText;
		string text = (_lastSaveTimeText.text = TimeUtil.GetDisplaySaveTime(_SelectedSaveFileHeader.SaveTimestamp));
		lastSaveTimeText.text = text;
		_levelNameText.text = Singleton<LevelManager>.Instance.GetLevelByID(_SelectedSaveFileHeader.LevelID)?.DisplayName ?? ("<color=red>Missing: " + _SelectedSaveFileHeader.LevelID + "</color>");
		_gamemodeNameText.text = Singleton<GamemodeManager>.Instance.GetColoredFormattedGamemodeName(_SelectedSaveFileHeader.GameMode);
		_moneyText.text = $"${_SelectedSaveFileHeader.Money:#,##0.00}";
		_playTimeText.text = SavingLoadingManager.GetFormattedPlaytime(_SelectedSaveFileHeader.TotalPlayTimeSeconds);
		StartCoroutine(SetScreenshotForJson(_selectedSaveFileFullPath));
		if (Singleton<DemoManager>.Instance.IsDemoVersion && !_SelectedSaveFileHeader.IsPlayableInDemoVersion())
		{
			_saveOutOfDateText.text = _saveFileNotDemoVersionString;
			_saveOutOfDateWarning.SetActive(value: true);
			_saveDataIsCompatibleThing.SetActive(value: false);
			_loadGameButton.interactable = false;
			return;
		}
		_loadGameButton.interactable = true;
		if (Singleton<SavingLoadingManager>.Instance.IsSaveFileCompatible(_SelectedSaveFileHeader.SaveVersion))
		{
			_saveOutOfDateWarning.SetActive(value: false);
			_saveDataIsCompatibleThing.SetActive(value: true);
		}
		else
		{
			if (_SelectedSaveFileHeader.SaveVersion < 16)
			{
				_saveOutOfDateText.text = _saveFileOutOfDateString;
			}
			else
			{
				_saveOutOfDateText.text = _saveFileTooNewString;
			}
			_saveOutOfDateWarning.SetActive(value: true);
			_saveDataIsCompatibleThing.SetActive(value: false);
		}
		_debugQuickLoadButton.SetActive(!(Singleton<DebugManager>.Instance == null) && Singleton<DebugManager>.Instance.DevModeEnabled);
	}

	public void OnLoadGamePressed()
	{
		LevelInfo levelByID = Singleton<LevelManager>.Instance.GetLevelByID(_SelectedSaveFileHeader.LevelID);
		if (levelByID.LevelID == "DemoCave")
		{
			_mainInfoPanel.SetActive(value: false);
			_settingsPanel.SetActive(value: false);
			_confirmDeletePanel.SetActive(value: false);
			_confirmLoadDemoSavePanel.SetActive(value: true);
		}
		else
		{
			Singleton<SavingLoadingManager>.Instance.LoadSceneThenLoadSave(_selectedSaveFileFullPath, levelByID.SceneName);
		}
	}

	public void OnConfirmLoadGamePressed()
	{
		string sceneName = Singleton<LevelManager>.Instance.GetLevelByID(_SelectedSaveFileHeader.LevelID)?.SceneName;
		Singleton<SavingLoadingManager>.Instance.LoadSceneThenLoadSave(_selectedSaveFileFullPath, sceneName);
	}

	public void OnQuickLoadPressed()
	{
		Singleton<SavingLoadingManager>.Instance.LoadGame(_selectedSaveFileFullPath);
	}

	public void OnFileSettingsPressed()
	{
		_mainInfoPanel.SetActive(value: false);
		_settingsPanel.SetActive(value: true);
		_confirmDeletePanel.SetActive(value: false);
		_confirmLoadDemoSavePanel.SetActive(value: false);
	}

	public void OnBackFromSettingsPressed()
	{
		_confirmDeletePanel.SetActive(value: false);
		_settingsPanel.SetActive(value: false);
		_mainInfoPanel.SetActive(value: true);
		_confirmLoadDemoSavePanel.SetActive(value: false);
	}

	public void OnConfirmDeletePressed()
	{
		Singleton<SavingLoadingManager>.Instance.DeleteSaveFile(_selectedSaveFileFullPath);
		RefreshSaveFileList();
	}

	public void OnOpenDeletePanelPressed()
	{
		_confirmDeletePanel.SetActive(value: true);
		_settingsPanel.SetActive(value: false);
		_mainInfoPanel.SetActive(value: false);
		_confirmLoadDemoSavePanel.SetActive(value: false);
	}

	public IEnumerator SetScreenshotForJson(string fullFilePath)
	{
		if (_fileScreenshotImage == null || string.IsNullOrWhiteSpace(fullFilePath))
		{
			yield break;
		}
		string imgPath = FindSiblingImage(fullFilePath);
		if (string.IsNullOrEmpty(imgPath))
		{
			_fileScreenshotImage.texture = _defaultSaveFileImage;
			_aspectRatioFitter.aspectRatio = 1.777778f;
			yield break;
		}
		string absoluteUri = new Uri(imgPath).AbsoluteUri;
		using UnityWebRequest req = UnityWebRequestTexture.GetTexture(absoluteUri, nonReadable: true);
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("Failed to load screenshot: " + req.error + " (" + imgPath + ")");
			_fileScreenshotImage.texture = _defaultSaveFileImage;
			_aspectRatioFitter.aspectRatio = 1.777778f;
			yield break;
		}
		Texture2D content = DownloadHandlerTexture.GetContent(req);
		if (_selectedSaveFileFullPath != fullFilePath)
		{
			UnityEngine.Object.Destroy(content);
			yield break;
		}
		if (_loadedRuntimeTexture != null)
		{
			UnityEngine.Object.Destroy(_loadedRuntimeTexture);
		}
		_loadedRuntimeTexture = content;
		_fileScreenshotImage.texture = content;
		if (content != null)
		{
			float num = content.width;
			float num2 = content.height;
			if (num > 0f && num2 > 0f)
			{
				_aspectRatioFitter.aspectRatio = Mathf.Clamp(num / num2, 0.01f, 100f);
			}
		}
	}

	private static string FindSiblingImage(string jsonPath)
	{
		if (!Path.IsPathRooted(jsonPath))
		{
			return null;
		}
		string[] array = new string[3] { ".png", ".jpg", ".jpeg" };
		string[] array2 = array;
		foreach (string extension in array2)
		{
			string text = Path.ChangeExtension(jsonPath, extension);
			if (File.Exists(text))
			{
				return text;
			}
		}
		string directoryName = Path.GetDirectoryName(jsonPath);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(jsonPath);
		if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
		{
			return null;
		}
		HashSet<string> hashSet = new HashSet<string>(array, StringComparer.OrdinalIgnoreCase);
		foreach (string item in Directory.EnumerateFiles(directoryName, fileNameWithoutExtension + ".*", SearchOption.TopDirectoryOnly))
		{
			if (hashSet.Contains(Path.GetExtension(item)))
			{
				return item;
			}
		}
		return null;
	}

	private void OnDisable()
	{
		if (_loadedRuntimeTexture != null)
		{
			UnityEngine.Object.Destroy(_loadedRuntimeTexture);
		}
	}
}
