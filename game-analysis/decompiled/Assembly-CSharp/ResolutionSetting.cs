using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020000D0 RID: 208
public class ResolutionSetting : BaseSettingOption
{
	// Token: 0x0600056D RID: 1389 RVA: 0x0001CABC File Offset: 0x0001ACBC
	private void Awake()
	{
		this.dropdown.onValueChanged.AddListener(new UnityAction<int>(this.SetResolutionFromIndex));
		this.LoadSavedResolution();
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x0001CAE0 File Offset: 0x0001ACE0
	protected override void OnEnable()
	{
		base.OnEnable();
		this.BuildResolutionList();
		this.PopulateDropdown();
		this.GetAndSelectCurrentIndex();
	}

	// Token: 0x0600056F RID: 1391 RVA: 0x0001CAFB File Offset: 0x0001ACFB
	private void OnDestroy()
	{
		this.dropdown.onValueChanged.RemoveListener(new UnityAction<int>(this.SetResolutionFromIndex));
	}

	// Token: 0x06000570 RID: 1392 RVA: 0x0001CB1C File Offset: 0x0001AD1C
	private void BuildResolutionList()
	{
		this.resolutionOptions.Clear();
		foreach (Resolution resolution in Screen.resolutions)
		{
			Vector2Int vector2Int = new Vector2Int(resolution.width, resolution.height);
			if (!this.resolutionOptions.Contains(vector2Int))
			{
				this.resolutionOptions.Add(vector2Int);
			}
		}
		this.resolutionOptions.Sort(delegate(Vector2Int a, Vector2Int b)
		{
			int num = a.x * a.y;
			return (b.x * b.y).CompareTo(num);
		});
	}

	// Token: 0x06000571 RID: 1393 RVA: 0x0001CBAC File Offset: 0x0001ADAC
	private void PopulateDropdown()
	{
		this.dropdown.ClearOptions();
		List<string> list = new List<string>();
		foreach (Vector2Int vector2Int in this.resolutionOptions)
		{
			list.Add(string.Format("{0} x {1}", vector2Int.x, vector2Int.y));
		}
		this.dropdown.AddOptions(list);
	}

	// Token: 0x06000572 RID: 1394 RVA: 0x0001CC40 File Offset: 0x0001AE40
	private void SetResolutionFromIndex(int index)
	{
		if (index < 0 || index >= this.resolutionOptions.Count)
		{
			return;
		}
		Vector2Int vector2Int = this.resolutionOptions[index];
		PlayerPrefs.SetInt("ResolutionWidth", vector2Int.x);
		PlayerPrefs.SetInt("ResolutionHeight", vector2Int.y);
		Screen.SetResolution(vector2Int.x, vector2Int.y, Screen.fullScreenMode);
	}

	// Token: 0x06000573 RID: 1395 RVA: 0x0001CCA8 File Offset: 0x0001AEA8
	private void LoadSavedResolution()
	{
		int andSelectCurrentIndex = this.GetAndSelectCurrentIndex();
		this.SetResolutionFromIndex(andSelectCurrentIndex);
	}

	// Token: 0x06000574 RID: 1396 RVA: 0x0001CCC4 File Offset: 0x0001AEC4
	private int GetAndSelectCurrentIndex()
	{
		int width = Screen.currentResolution.width;
		int height = Screen.currentResolution.height;
		int @int = PlayerPrefs.GetInt("ResolutionWidth", width);
		int int2 = PlayerPrefs.GetInt("ResolutionHeight", height);
		int num = 0;
		for (int i = 0; i < this.resolutionOptions.Count; i++)
		{
			if (this.resolutionOptions[i].x == @int && this.resolutionOptions[i].y == int2)
			{
				num = i;
				break;
			}
		}
		this.dropdown.value = num;
		this.dropdown.RefreshShownValue();
		return num;
	}

	// Token: 0x040006AA RID: 1706
	[SerializeField]
	private TMP_Dropdown dropdown;

	// Token: 0x040006AB RID: 1707
	private List<Vector2Int> resolutionOptions = new List<Vector2Int>();
}
