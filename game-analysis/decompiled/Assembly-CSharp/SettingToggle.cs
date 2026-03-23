using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000D5 RID: 213
public class SettingToggle : BaseSettingOption
{
	// Token: 0x060005A9 RID: 1449 RVA: 0x0001DAFC File Offset: 0x0001BCFC
	private void Awake()
	{
		bool flag = PlayerPrefs.GetInt(this.settingKey, this.defaultValue ? 1 : 0) == 1;
		this.toggle.isOn = flag;
		this.UpdateLabel(flag);
	}

	// Token: 0x060005AA RID: 1450 RVA: 0x0001DB37 File Offset: 0x0001BD37
	protected override void OnEnable()
	{
		base.OnEnable();
		this.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnToggleChanged));
	}

	// Token: 0x060005AB RID: 1451 RVA: 0x0001DB5B File Offset: 0x0001BD5B
	private void OnDisable()
	{
		this.toggle.onValueChanged.RemoveListener(new UnityAction<bool>(this.OnToggleChanged));
	}

	// Token: 0x060005AC RID: 1452 RVA: 0x0001DB79 File Offset: 0x0001BD79
	private void OnToggleChanged(bool value)
	{
		if (this._suppressEvents)
		{
			return;
		}
		this.SaveAndApply(value);
	}

	// Token: 0x060005AD RID: 1453 RVA: 0x0001DB8B File Offset: 0x0001BD8B
	private void SaveAndApply(bool value)
	{
		PlayerPrefs.SetInt(this.settingKey, value ? 1 : 0);
		this.UpdateLabel(value);
		Action<bool> action = this.onValueChanged;
		if (action == null)
		{
			return;
		}
		action(value);
	}

	// Token: 0x060005AE RID: 1454 RVA: 0x0001DBB7 File Offset: 0x0001BDB7
	private void UpdateLabel(bool value)
	{
		if (this._onOffLabel != null)
		{
			this._onOffLabel.text = (value ? this.onText : this.offText);
		}
	}

	// Token: 0x060005AF RID: 1455 RVA: 0x0001DBE4 File Offset: 0x0001BDE4
	public void RefreshFromSaved()
	{
		bool flag = PlayerPrefs.GetInt(this.settingKey, this.defaultValue ? 1 : 0) == 1;
		this._suppressEvents = true;
		this.toggle.isOn = flag;
		this._suppressEvents = false;
		this.UpdateLabel(flag);
	}

	// Token: 0x040006E2 RID: 1762
	[Header("UI References")]
	[SerializeField]
	private Toggle toggle;

	// Token: 0x040006E3 RID: 1763
	[SerializeField]
	private TMP_Text _onOffLabel;

	// Token: 0x040006E4 RID: 1764
	[Header("Toggle Settings")]
	[SerializeField]
	private string settingKey = "UnnamedBoolSetting";

	// Token: 0x040006E5 RID: 1765
	[SerializeField]
	private bool defaultValue = true;

	// Token: 0x040006E6 RID: 1766
	[SerializeField]
	private string onText = "On";

	// Token: 0x040006E7 RID: 1767
	[SerializeField]
	private string offText = "Off";

	// Token: 0x040006E8 RID: 1768
	public Action<bool> onValueChanged;

	// Token: 0x040006E9 RID: 1769
	private bool _suppressEvents;
}
