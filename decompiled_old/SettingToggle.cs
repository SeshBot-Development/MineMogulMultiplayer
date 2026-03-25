using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingToggle : BaseSettingOption
{
	[Header("UI References")]
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TMP_Text _onOffLabel;

	[Header("Toggle Settings")]
	[SerializeField]
	private string settingKey = "UnnamedBoolSetting";

	[SerializeField]
	private bool defaultValue = true;

	[SerializeField]
	private string onText = "On";

	[SerializeField]
	private string offText = "Off";

	public Action<bool> onValueChanged;

	private bool _suppressEvents;

	private void Awake()
	{
		bool flag = PlayerPrefs.GetInt(settingKey, defaultValue ? 1 : 0) == 1;
		toggle.isOn = flag;
		UpdateLabel(flag);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		toggle.onValueChanged.AddListener(OnToggleChanged);
	}

	private void OnDisable()
	{
		toggle.onValueChanged.RemoveListener(OnToggleChanged);
	}

	private void OnToggleChanged(bool value)
	{
		if (!_suppressEvents)
		{
			SaveAndApply(value);
		}
	}

	private void SaveAndApply(bool value)
	{
		PlayerPrefs.SetInt(settingKey, value ? 1 : 0);
		UpdateLabel(value);
		onValueChanged?.Invoke(value);
	}

	private void UpdateLabel(bool value)
	{
		if (_onOffLabel != null)
		{
			_onOffLabel.text = (value ? onText : offText);
		}
	}

	public void RefreshFromSaved()
	{
		bool flag = PlayerPrefs.GetInt(settingKey, defaultValue ? 1 : 0) == 1;
		_suppressEvents = true;
		toggle.isOn = flag;
		_suppressEvents = false;
		UpdateLabel(flag);
	}
}
