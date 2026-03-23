using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200006A RID: 106
public class NewGameMenu : MonoBehaviour
{
	// Token: 0x060002C4 RID: 708 RVA: 0x0000D6C4 File Offset: 0x0000B8C4
	private void OnEnable()
	{
		foreach (object obj in this._mapListContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this._mapSelectButtons.Clear();
		foreach (LevelInfo levelInfo in Singleton<LevelManager>.Instance.GetLevelsToShowInMapSelect())
		{
			MapSelectButton mapSelectButton = Object.Instantiate<MapSelectButton>(this._mapSelectButtonPrefab, this._mapListContainer);
			mapSelectButton.Initialize(levelInfo, this);
			this._mapSelectButtons.Add(mapSelectButton);
		}
		this.OnMapSelected(this._mapSelectButtons[0]);
		TMP_Text versionNumberText = this._versionNumberText;
		VersionManager instance = Singleton<VersionManager>.Instance;
		versionNumberText.text = ((instance != null) ? instance.GetFormattedVersionText() : null);
		if (string.IsNullOrEmpty(this._newSaveFileNameInputField.text))
		{
			this.SetDefaultUniqueName();
		}
		this.SaveFileNameAlreadyExists(this._newSaveFileNameInputField.text);
		this._newSaveFileNameInputField.onEndEdit.AddListener(new UnityAction<string>(this.OnInputSubmitted));
		TMP_InputField newSaveFileNameInputField = this._newSaveFileNameInputField;
		newSaveFileNameInputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(newSaveFileNameInputField.onValidateInput, new TMP_InputField.OnValidateInput(this.ValidateChar));
	}

	// Token: 0x060002C5 RID: 709 RVA: 0x0000D830 File Offset: 0x0000BA30
	public void OnMapSelected(MapSelectButton mapSelectButton)
	{
		this._selectedLevelInfo = mapSelectButton.LevelInfo;
		foreach (MapSelectButton mapSelectButton2 in this._mapSelectButtons)
		{
			mapSelectButton2.UpdateSelected(mapSelectButton2 == mapSelectButton);
		}
	}

	// Token: 0x060002C6 RID: 710 RVA: 0x0000D894 File Offset: 0x0000BA94
	private void OnDisable()
	{
		this._newSaveFileNameInputField.onEndEdit.RemoveListener(new UnityAction<string>(this.OnInputSubmitted));
		TMP_InputField newSaveFileNameInputField = this._newSaveFileNameInputField;
		newSaveFileNameInputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Remove(newSaveFileNameInputField.onValidateInput, new TMP_InputField.OnValidateInput(this.ValidateChar));
	}

	// Token: 0x060002C7 RID: 711 RVA: 0x0000D8E4 File Offset: 0x0000BAE4
	private void Update()
	{
		this._confirmNewGameButton.interactable = !string.IsNullOrEmpty(this._newSaveFileNameInputField.text);
	}

	// Token: 0x060002C8 RID: 712 RVA: 0x0000D904 File Offset: 0x0000BB04
	public void OnConfirmNewGamePressed()
	{
		if (string.IsNullOrEmpty(this._newSaveFileNameInputField.text))
		{
			return;
		}
		if (this.SaveFileNameAlreadyExists(this._newSaveFileNameInputField.text))
		{
			return;
		}
		Debug.Log("Loading level '" + this._selectedLevelInfo.LevelID + "' with new Save File: " + this._newSaveFileNameInputField.text);
		Singleton<SavingLoadingManager>.Instance.LoadSceneAndStartNewSaveFile(this._newSaveFileNameInputField.text, this._selectedLevelInfo.SceneName);
		base.gameObject.SetActive(false);
	}

	// Token: 0x060002C9 RID: 713 RVA: 0x0000D98E File Offset: 0x0000BB8E
	private void OnInputSubmitted(string input)
	{
		input = Path.GetFileNameWithoutExtension(input);
		this._newSaveFileNameInputField.text = input;
		this.SaveFileNameAlreadyExists(input);
	}

	// Token: 0x060002CA RID: 714 RVA: 0x0000D9AC File Offset: 0x0000BBAC
	private char ValidateChar(string current, int pos, char ch)
	{
		if (char.IsControl(ch))
		{
			return '\0';
		}
		if (Array.IndexOf<char>(NewGameMenu.InvalidChars, ch) >= 0)
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

	// Token: 0x060002CB RID: 715 RVA: 0x0000D9DC File Offset: 0x0000BBDC
	public bool SaveFileNameAlreadyExists(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			this._saveFileAlreadyExistsWarning.SetActive(false);
			return false;
		}
		if (File.Exists(SavingLoadingManager.GetFullSaveFilePath(fileName, true)))
		{
			this._saveFileAlreadyExistsWarning.SetActive(true);
			return true;
		}
		this._saveFileAlreadyExistsWarning.SetActive(false);
		return false;
	}

	// Token: 0x060002CC RID: 716 RVA: 0x0000DA28 File Offset: 0x0000BC28
	public void SetDefaultUniqueName()
	{
		string text = this.FirstAvailableName("New Game", 1000);
		this._newSaveFileNameInputField.SetTextWithoutNotify(text);
		this._newSaveFileNameInputField.caretPosition = text.Length;
	}

	// Token: 0x060002CD RID: 717 RVA: 0x0000DA64 File Offset: 0x0000BC64
	private string FirstAvailableName(string baseName, int maxLen)
	{
		for (int i = 0; i < 10000; i++)
		{
			string text = ((i == 0) ? "" : string.Format(" {0}", i));
			string text2 = NewGameMenu.TruncateForSuffix(baseName, text, maxLen);
			if (!File.Exists(SavingLoadingManager.GetFullSaveFilePath(text2, true)))
			{
				return text2;
			}
		}
		return "";
	}

	// Token: 0x060002CE RID: 718 RVA: 0x0000DABC File Offset: 0x0000BCBC
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

	// Token: 0x040002A1 RID: 673
	[SerializeField]
	private TMP_InputField _newSaveFileNameInputField;

	// Token: 0x040002A2 RID: 674
	[SerializeField]
	private Button _confirmNewGameButton;

	// Token: 0x040002A3 RID: 675
	[SerializeField]
	private GameObject _saveFileAlreadyExistsWarning;

	// Token: 0x040002A4 RID: 676
	[SerializeField]
	private TMP_Text _versionNumberText;

	// Token: 0x040002A5 RID: 677
	[SerializeField]
	private Transform _mapListContainer;

	// Token: 0x040002A6 RID: 678
	[SerializeField]
	private MapSelectButton _mapSelectButtonPrefab;

	// Token: 0x040002A7 RID: 679
	private List<MapSelectButton> _mapSelectButtons = new List<MapSelectButton>();

	// Token: 0x040002A8 RID: 680
	private LevelInfo _selectedLevelInfo;

	// Token: 0x040002A9 RID: 681
	private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();
}
