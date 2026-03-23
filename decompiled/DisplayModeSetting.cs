using TMPro;
using UnityEngine;

public class DisplayModeSetting : BaseSettingOption
{
	[SerializeField]
	private TMP_Dropdown dropdown;

	private void Awake()
	{
		dropdown.onValueChanged.AddListener(SetDisplayMode);
		LoadSavedDisplayMode();
	}

	private void OnDestroy()
	{
		dropdown.onValueChanged.RemoveListener(SetDisplayMode);
	}

	private void SetDisplayMode(int index)
	{
		SettingsManager.SetDisplayMode(index);
	}

	private void LoadSavedDisplayMode()
	{
		int num = PlayerPrefs.GetInt("DisplayMode", 1);
		dropdown.value = num;
		dropdown.RefreshShownValue();
		SetDisplayMode(num);
	}
}
