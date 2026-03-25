using System.IO;
using TMPro;
using UnityEngine;

public class SaveFileButton : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _saveFileNameText;

	[SerializeField]
	private TMP_Text _saveVersionNumberText;

	[SerializeField]
	private TMP_Text _lastSaveTimeText;

	private string _saveFilePath;

	private SaveFileHeader _saveFileHeader;

	private LoadingMenu _loadingMenu;

	public void Initialize(string saveFilePath, SaveFileHeader saveFileHeader, LoadingMenu loadingMenu)
	{
		_loadingMenu = loadingMenu;
		_saveFilePath = saveFilePath;
		_saveFileHeader = saveFileHeader;
		_saveFileNameText.text = Path.GetFileNameWithoutExtension(_saveFilePath);
		_saveVersionNumberText.text = _saveFileHeader.GameVersion;
		_lastSaveTimeText.text = TimeUtil.GetDisplaySaveTime(_saveFileHeader.SaveTimestamp);
		if (Singleton<DemoManager>.Instance.IsDemoVersion && !_saveFileHeader.IsPlayableInDemoVersion())
		{
			_saveFileNameText.color = Color.gray;
			_lastSaveTimeText.color = Color.gray;
			_saveVersionNumberText.color = Color.gray;
		}
	}

	public void OnSelected()
	{
		_loadingMenu.SelectSaveFile(_saveFilePath, _saveFileHeader);
	}
}
