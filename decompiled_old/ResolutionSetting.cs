using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSetting : BaseSettingOption
{
	[SerializeField]
	private TMP_Dropdown dropdown;

	private List<Vector2Int> resolutionOptions = new List<Vector2Int>();

	private void Awake()
	{
		dropdown.onValueChanged.AddListener(SetResolutionFromIndex);
		LoadSavedResolution();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		BuildResolutionList();
		PopulateDropdown();
		GetAndSelectCurrentIndex();
	}

	private void OnDestroy()
	{
		dropdown.onValueChanged.RemoveListener(SetResolutionFromIndex);
	}

	private void BuildResolutionList()
	{
		resolutionOptions.Clear();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			Vector2Int item = new Vector2Int(resolution.width, resolution.height);
			if (!resolutionOptions.Contains(item))
			{
				resolutionOptions.Add(item);
			}
		}
		resolutionOptions.Sort(delegate(Vector2Int a, Vector2Int b)
		{
			int value = a.x * a.y;
			return (b.x * b.y).CompareTo(value);
		});
	}

	private void PopulateDropdown()
	{
		dropdown.ClearOptions();
		List<string> list = new List<string>();
		foreach (Vector2Int resolutionOption in resolutionOptions)
		{
			list.Add($"{resolutionOption.x} x {resolutionOption.y}");
		}
		dropdown.AddOptions(list);
	}

	private void SetResolutionFromIndex(int index)
	{
		if (index >= 0 && index < resolutionOptions.Count)
		{
			Vector2Int vector2Int = resolutionOptions[index];
			PlayerPrefs.SetInt("ResolutionWidth", vector2Int.x);
			PlayerPrefs.SetInt("ResolutionHeight", vector2Int.y);
			Screen.SetResolution(vector2Int.x, vector2Int.y, Screen.fullScreenMode);
		}
	}

	private void LoadSavedResolution()
	{
		int andSelectCurrentIndex = GetAndSelectCurrentIndex();
		SetResolutionFromIndex(andSelectCurrentIndex);
	}

	private int GetAndSelectCurrentIndex()
	{
		int width = Screen.currentResolution.width;
		int height = Screen.currentResolution.height;
		int num = PlayerPrefs.GetInt("ResolutionWidth", width);
		int num2 = PlayerPrefs.GetInt("ResolutionHeight", height);
		int num3 = 0;
		for (int i = 0; i < resolutionOptions.Count; i++)
		{
			if (resolutionOptions[i].x == num && resolutionOptions[i].y == num2)
			{
				num3 = i;
				break;
			}
		}
		dropdown.value = num3;
		dropdown.RefreshShownValue();
		return num3;
	}
}
