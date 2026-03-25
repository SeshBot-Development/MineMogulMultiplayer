using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewGameMenu : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _newSaveFileNameInputField;

	[SerializeField]
	private Button _confirmNewGameButton;

	[SerializeField]
	private GameObject _saveFileAlreadyExistsWarning;

	[SerializeField]
	private TMP_Text _versionNumberText;

	[SerializeField]
	private Transform _mapListContainer;

	[SerializeField]
	private MapSelectButton _mapSelectButtonPrefab;

	[SerializeField]
	private Toggle _standardGamemodeToggle;

	[SerializeField]
	private Toggle _sandboxGamemodeToggle;

	private List<MapSelectButton> _mapSelectButtons = new List<MapSelectButton>();

	private LevelInfo _selectedLevelInfo;

	private GameModeType _selectedGameMode;

	private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

	private void OnEnable()
	{
		foreach (Transform item in _mapListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		_mapSelectButtons.Clear();
		foreach (LevelInfo item2 in Singleton<LevelManager>.Instance.GetLevelsToShowInMapSelect())
		{
			MapSelectButton mapSelectButton = UnityEngine.Object.Instantiate(_mapSelectButtonPrefab, _mapListContainer);
			mapSelectButton.Initialize(item2, this);
			_mapSelectButtons.Add(mapSelectButton);
		}
		OnMapSelected(_mapSelectButtons[0]);
		_versionNumberText.text = Singleton<VersionManager>.Instance?.GetFormattedVersionText();
		if (string.IsNullOrEmpty(_newSaveFileNameInputField.text))
		{
			SetDefaultUniqueName();
		}
		SaveFileNameAlreadyExists(_newSaveFileNameInputField.text);
		_newSaveFileNameInputField.onEndEdit.AddListener(OnInputSubmitted);
		TMP_InputField newSaveFileNameInputField = _newSaveFileNameInputField;
		newSaveFileNameInputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(newSaveFileNameInputField.onValidateInput, new TMP_InputField.OnValidateInput(ValidateChar));
		_standardGamemodeToggle.onValueChanged.AddListener(OnStandardGamemodeToggleChanged);
		_sandboxGamemodeToggle.onValueChanged.AddListener(OnSandboxGamemodeToggleChanged);
	}

	public void OnMapSelected(MapSelectButton mapSelectButton)
	{
		bool flag = _selectedLevelInfo != null && _selectedLevelInfo.ShouldForceSandboxGamemode;
		_selectedLevelInfo = mapSelectButton.LevelInfo;
		foreach (MapSelectButton mapSelectButton2 in _mapSelectButtons)
		{
			mapSelectButton2.UpdateSelected(mapSelectButton2 == mapSelectButton);
		}
		if (_selectedLevelInfo.ShouldForceSandboxGamemode)
		{
			_selectedGameMode = GameModeType.Sandbox;
			_standardGamemodeToggle.interactable = false;
			_sandboxGamemodeToggle.isOn = true;
			return;
		}
		_standardGamemodeToggle.interactable = true;
		if (_selectedGameMode == GameModeType.Sandbox && flag)
		{
			_selectedGameMode = GameModeType.Standard;
			_standardGamemodeToggle.isOn = true;
		}
	}

	private void OnDisable()
	{
		_newSaveFileNameInputField.onEndEdit.RemoveListener(OnInputSubmitted);
		TMP_InputField newSaveFileNameInputField = _newSaveFileNameInputField;
		newSaveFileNameInputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Remove(newSaveFileNameInputField.onValidateInput, new TMP_InputField.OnValidateInput(ValidateChar));
		_standardGamemodeToggle.onValueChanged.RemoveListener(OnStandardGamemodeToggleChanged);
		_sandboxGamemodeToggle.onValueChanged.RemoveListener(OnSandboxGamemodeToggleChanged);
	}

	private void OnStandardGamemodeToggleChanged(bool value)
	{
		if (value)
		{
			_selectedGameMode = GameModeType.Standard;
		}
	}

	private void OnSandboxGamemodeToggleChanged(bool value)
	{
		if (value)
		{
			_selectedGameMode = GameModeType.Sandbox;
		}
	}

	private void Update()
	{
		_confirmNewGameButton.interactable = !string.IsNullOrEmpty(_newSaveFileNameInputField.text);
	}

	public void OnConfirmNewGamePressed()
	{
		if (!string.IsNullOrEmpty(_newSaveFileNameInputField.text) && !SaveFileNameAlreadyExists(_newSaveFileNameInputField.text))
		{
			Debug.Log("Loading level '" + _selectedLevelInfo.LevelID + "' with new Save File: " + _newSaveFileNameInputField.text);
			if (_selectedLevelInfo.ShouldForceSandboxGamemode)
			{
				_selectedGameMode = GameModeType.Sandbox;
			}
			Singleton<GamemodeManager>.Instance.GameModeType = _selectedGameMode;
			Singleton<SavingLoadingManager>.Instance.LoadSceneAndStartNewSaveFile(_newSaveFileNameInputField.text, _selectedLevelInfo.SceneName);
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnInputSubmitted(string input)
	{
		input = Path.GetFileNameWithoutExtension(input);
		_newSaveFileNameInputField.text = input;
		SaveFileNameAlreadyExists(input);
	}

	private char ValidateChar(string current, int pos, char ch)
	{
		if (char.IsControl(ch))
		{
			return '\0';
		}
		if (Array.IndexOf(InvalidChars, ch) >= 0)
		{
			return '\0';
		}
		if (ch == '.')
		{
			return '\0';
		}
		if (pos == 0 && ch == ' ')
		{
			return '\0';
		}
		return ch;
	}

	public bool SaveFileNameAlreadyExists(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			_saveFileAlreadyExistsWarning.SetActive(value: false);
			return false;
		}
		if (File.Exists(SavingLoadingManager.GetFullSaveFilePath(fileName)))
		{
			_saveFileAlreadyExistsWarning.SetActive(value: true);
			return true;
		}
		_saveFileAlreadyExistsWarning.SetActive(value: false);
		return false;
	}

	public void SetDefaultUniqueName()
	{
		string text = FirstAvailableName("New Game", 1000);
		_newSaveFileNameInputField.SetTextWithoutNotify(text);
		_newSaveFileNameInputField.caretPosition = text.Length;
	}

	private string FirstAvailableName(string baseName, int maxLen)
	{
		for (int i = 0; i < 10000; i++)
		{
			string suffix = ((i == 0) ? "" : $" {i}");
			string text = TruncateForSuffix(baseName, suffix, maxLen);
			if (!File.Exists(SavingLoadingManager.GetFullSaveFilePath(text)))
			{
				return text;
			}
		}
		return "";
	}

	private static string TruncateForSuffix(string baseName, string suffix, int maxLen)
	{
		int num = Math.Max(0, maxLen - suffix.Length);
		string text = baseName;
		if (text.Length > num)
		{
			int num2 = num;
			if (num2 > 0 && char.IsHighSurrogate(text[num2 - 1]))
			{
				num2--;
			}
			text = text.Substring(0, num2).TrimEnd();
		}
		return text + suffix;
	}
}
