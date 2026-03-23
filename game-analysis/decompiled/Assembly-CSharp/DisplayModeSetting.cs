using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000047 RID: 71
public class DisplayModeSetting : BaseSettingOption
{
	// Token: 0x060001E1 RID: 481 RVA: 0x00009F4C File Offset: 0x0000814C
	private void Awake()
	{
		this.dropdown.onValueChanged.AddListener(new UnityAction<int>(this.SetDisplayMode));
		this.LoadSavedDisplayMode();
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x00009F70 File Offset: 0x00008170
	private void OnDestroy()
	{
		this.dropdown.onValueChanged.RemoveListener(new UnityAction<int>(this.SetDisplayMode));
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x00009F8E File Offset: 0x0000818E
	private void SetDisplayMode(int index)
	{
		SettingsManager.SetDisplayMode(index);
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x00009F98 File Offset: 0x00008198
	private void LoadSavedDisplayMode()
	{
		int @int = PlayerPrefs.GetInt("DisplayMode", 1);
		this.dropdown.value = @int;
		this.dropdown.RefreshShownValue();
		this.SetDisplayMode(@int);
	}

	// Token: 0x040001D5 RID: 469
	[SerializeField]
	private TMP_Dropdown dropdown;
}
