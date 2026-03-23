using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000D2 RID: 210
public class SettingSlider : BaseSettingOption
{
	// Token: 0x0600057D RID: 1405 RVA: 0x0001CF18 File Offset: 0x0001B118
	private void Awake()
	{
		this.slider.minValue = this.minValue;
		this.slider.maxValue = this.maxValue;
		this.slider.wholeNumbers = this.useInts;
		float num = this.LoadValue();
		this.slider.value = num;
		this.UpdateInputField(num);
	}

	// Token: 0x0600057E RID: 1406 RVA: 0x0001CF72 File Offset: 0x0001B172
	protected override void OnEnable()
	{
		base.OnEnable();
		this.slider.onValueChanged.AddListener(new UnityAction<float>(this.OnSliderChanged));
		this.valueInput.onEndEdit.AddListener(new UnityAction<string>(this.OnInputSubmitted));
	}

	// Token: 0x0600057F RID: 1407 RVA: 0x0001CFB2 File Offset: 0x0001B1B2
	private void OnDisable()
	{
		this.slider.onValueChanged.RemoveListener(new UnityAction<float>(this.OnSliderChanged));
		this.valueInput.onEndEdit.RemoveListener(new UnityAction<string>(this.OnInputSubmitted));
	}

	// Token: 0x06000580 RID: 1408 RVA: 0x0001CFEC File Offset: 0x0001B1EC
	private void OnSliderChanged(float value)
	{
		if (this._suppressEvents)
		{
			return;
		}
		value = this.SnapToIncrement(value);
		if (this.useInts)
		{
			value = Mathf.Round(value);
		}
		value = Mathf.Clamp(value, this.minValue, this.maxValue);
		this.SaveAndApply(value);
	}

	// Token: 0x06000581 RID: 1409 RVA: 0x0001D02B File Offset: 0x0001B22B
	private float SnapToIncrement(float value)
	{
		if (!this.useIncrement || this.increment <= 0f)
		{
			return value;
		}
		return Mathf.Round(value / this.increment) * this.increment;
	}

	// Token: 0x06000582 RID: 1410 RVA: 0x0001D058 File Offset: 0x0001B258
	private void OnInputSubmitted(string input)
	{
		if (this._suppressEvents)
		{
			return;
		}
		input = input.Replace("%", "");
		int num;
		float num2;
		if (!(this.useInts ? int.TryParse(input, out num) : float.TryParse(input, out num2)))
		{
			this.UpdateInputField(this.slider.value);
			return;
		}
		float num3 = float.Parse(input);
		num3 = this.SnapToIncrement(num3);
		if (this.showAsPercent)
		{
			num3 /= 100f;
		}
		if (this.useInts)
		{
			num3 = Mathf.Round(num3);
		}
		num3 = Mathf.Clamp(num3, this.minValue, this.maxValue);
		this._suppressEvents = true;
		this.slider.value = num3;
		this._suppressEvents = false;
		this.SaveAndApply(num3);
	}

	// Token: 0x06000583 RID: 1411 RVA: 0x0001D110 File Offset: 0x0001B310
	private void SaveAndApply(float value)
	{
		if (this.useInts)
		{
			PlayerPrefs.SetInt(this.settingKey, Mathf.RoundToInt(value));
		}
		else
		{
			PlayerPrefs.SetFloat(this.settingKey, value);
		}
		this.UpdateInputField(value);
		Action<float> action = this.onValueChanged;
		if (action == null)
		{
			return;
		}
		action(value);
	}

	// Token: 0x06000584 RID: 1412 RVA: 0x0001D15C File Offset: 0x0001B35C
	private float LoadValue()
	{
		if (this.useInts)
		{
			return (float)PlayerPrefs.GetInt(this.settingKey, Mathf.RoundToInt(this.defaultValue));
		}
		return PlayerPrefs.GetFloat(this.settingKey, this.defaultValue);
	}

	// Token: 0x06000585 RID: 1413 RVA: 0x0001D190 File Offset: 0x0001B390
	private void UpdateInputField(float value)
	{
		if (this.valueInput == null)
		{
			return;
		}
		this._suppressEvents = true;
		if (this.maxMeansInfinite && Mathf.Approximately(value, this.maxValue))
		{
			this.valueInput.text = "Infinite";
		}
		else if (this.showAsPercent)
		{
			this.valueInput.text = string.Format("{0}%", Mathf.RoundToInt(value * 100f));
		}
		else if (this.useInts)
		{
			this.valueInput.text = string.Format("{0}", Mathf.RoundToInt(value));
		}
		else
		{
			this.valueInput.text = string.Format("{0:0.##}", value);
		}
		this._suppressEvents = false;
	}

	// Token: 0x06000586 RID: 1414 RVA: 0x0001D258 File Offset: 0x0001B458
	public void RefreshFromSaved()
	{
		float num = this.LoadValue();
		this.slider.value = num;
		this.UpdateInputField(num);
	}

	// Token: 0x040006B1 RID: 1713
	[Header("UI References")]
	[SerializeField]
	private Slider slider;

	// Token: 0x040006B2 RID: 1714
	[SerializeField]
	private TMP_InputField valueInput;

	// Token: 0x040006B3 RID: 1715
	[Header("Slider Settings")]
	[SerializeField]
	private string settingKey = "UnnamedSetting";

	// Token: 0x040006B4 RID: 1716
	[SerializeField]
	private float defaultValue = 1f;

	// Token: 0x040006B5 RID: 1717
	[SerializeField]
	private float minValue;

	// Token: 0x040006B6 RID: 1718
	[SerializeField]
	private float maxValue = 1f;

	// Token: 0x040006B7 RID: 1719
	[SerializeField]
	private bool showAsPercent;

	// Token: 0x040006B8 RID: 1720
	[SerializeField]
	private bool useInts;

	// Token: 0x040006B9 RID: 1721
	[SerializeField]
	private bool maxMeansInfinite;

	// Token: 0x040006BA RID: 1722
	[SerializeField]
	private bool useIncrement;

	// Token: 0x040006BB RID: 1723
	[SerializeField]
	private float increment = 10f;

	// Token: 0x040006BC RID: 1724
	public Action<float> onValueChanged;

	// Token: 0x040006BD RID: 1725
	private bool _suppressEvents;
}
