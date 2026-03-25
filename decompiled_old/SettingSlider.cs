using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingSlider : BaseSettingOption
{
	[Header("UI References")]
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TMP_InputField valueInput;

	[Header("Slider Settings")]
	[SerializeField]
	private string settingKey = "UnnamedSetting";

	[SerializeField]
	private float defaultValue = 1f;

	[SerializeField]
	private float minValue;

	[SerializeField]
	private float maxValue = 1f;

	[SerializeField]
	private bool showAsPercent;

	[SerializeField]
	private bool useInts;

	[SerializeField]
	private bool maxMeansInfinite;

	[SerializeField]
	private bool useIncrement;

	[SerializeField]
	private float increment = 10f;

	public Action<float> onValueChanged;

	private bool _suppressEvents;

	private void Awake()
	{
		slider.minValue = minValue;
		slider.maxValue = maxValue;
		slider.wholeNumbers = useInts;
		float value = LoadValue();
		slider.value = value;
		UpdateInputField(value);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		slider.onValueChanged.AddListener(OnSliderChanged);
		valueInput.onEndEdit.AddListener(OnInputSubmitted);
	}

	private void OnDisable()
	{
		slider.onValueChanged.RemoveListener(OnSliderChanged);
		valueInput.onEndEdit.RemoveListener(OnInputSubmitted);
	}

	private void OnSliderChanged(float value)
	{
		if (!_suppressEvents)
		{
			value = SnapToIncrement(value);
			if (useInts)
			{
				value = Mathf.Round(value);
			}
			value = Mathf.Clamp(value, minValue, maxValue);
			SaveAndApply(value);
		}
	}

	private float SnapToIncrement(float value)
	{
		if (!useIncrement || increment <= 0f)
		{
			return value;
		}
		return Mathf.Round(value / increment) * increment;
	}

	private void OnInputSubmitted(string input)
	{
		if (_suppressEvents)
		{
			return;
		}
		input = input.Replace("%", "");
		if (!(useInts ? int.TryParse(input, out var _) : float.TryParse(input, out var _)))
		{
			UpdateInputField(slider.value);
			return;
		}
		float value = float.Parse(input);
		value = SnapToIncrement(value);
		if (showAsPercent)
		{
			value /= 100f;
		}
		if (useInts)
		{
			value = Mathf.Round(value);
		}
		value = Mathf.Clamp(value, minValue, maxValue);
		_suppressEvents = true;
		slider.value = value;
		_suppressEvents = false;
		SaveAndApply(value);
	}

	private void SaveAndApply(float value)
	{
		if (useInts)
		{
			PlayerPrefs.SetInt(settingKey, Mathf.RoundToInt(value));
		}
		else
		{
			PlayerPrefs.SetFloat(settingKey, value);
		}
		UpdateInputField(value);
		onValueChanged?.Invoke(value);
	}

	private float LoadValue()
	{
		if (useInts)
		{
			return PlayerPrefs.GetInt(settingKey, Mathf.RoundToInt(defaultValue));
		}
		return PlayerPrefs.GetFloat(settingKey, defaultValue);
	}

	private void UpdateInputField(float value)
	{
		if (!(valueInput == null))
		{
			_suppressEvents = true;
			if (maxMeansInfinite && Mathf.Approximately(value, maxValue))
			{
				valueInput.text = "Infinite";
			}
			else if (showAsPercent)
			{
				valueInput.text = $"{Mathf.RoundToInt(value * 100f)}%";
			}
			else if (useInts)
			{
				valueInput.text = $"{Mathf.RoundToInt(value)}";
			}
			else
			{
				valueInput.text = $"{value:0.##}";
			}
			_suppressEvents = false;
		}
	}

	public void RefreshFromSaved()
	{
		float value = LoadValue();
		slider.value = value;
		UpdateInputField(value);
	}
}
