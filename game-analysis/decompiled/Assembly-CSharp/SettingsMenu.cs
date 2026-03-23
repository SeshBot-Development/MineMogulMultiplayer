using System;
using UnityEngine;

// Token: 0x020000D4 RID: 212
public class SettingsMenu : MonoBehaviour
{
	// Token: 0x0600058F RID: 1423 RVA: 0x0001D518 File Offset: 0x0001B718
	private void Start()
	{
		SettingSlider masterVolumeSlider = this._masterVolumeSlider;
		masterVolumeSlider.onValueChanged = (Action<float>)Delegate.Combine(masterVolumeSlider.onValueChanged, new Action<float>(this.ApplyMasterVolume));
		SettingSlider mouseSensitivitySlider = this._mouseSensitivitySlider;
		mouseSensitivitySlider.onValueChanged = (Action<float>)Delegate.Combine(mouseSensitivitySlider.onValueChanged, new Action<float>(this.ApplyMouseSensitivity));
		SettingSlider viewmodelBobScaleSlider = this._viewmodelBobScaleSlider;
		viewmodelBobScaleSlider.onValueChanged = (Action<float>)Delegate.Combine(viewmodelBobScaleSlider.onValueChanged, new Action<float>(this.ApplyViewmodelBobScale));
		SettingSlider cameraBobScaleSlider = this._cameraBobScaleSlider;
		cameraBobScaleSlider.onValueChanged = (Action<float>)Delegate.Combine(cameraBobScaleSlider.onValueChanged, new Action<float>(this.ApplyCameraBobScale));
		SettingToggle reverseHotbarScrollingToggle = this._reverseHotbarScrollingToggle;
		reverseHotbarScrollingToggle.onValueChanged = (Action<bool>)Delegate.Combine(reverseHotbarScrollingToggle.onValueChanged, new Action<bool>(this.ApplyReverseHotbarScrollingToggle));
		SettingToggle invertMouseXToggle = this._invertMouseXToggle;
		invertMouseXToggle.onValueChanged = (Action<bool>)Delegate.Combine(invertMouseXToggle.onValueChanged, new Action<bool>(this.ApplyInvertMouseXToggle));
		SettingToggle invertMouseYToggle = this._invertMouseYToggle;
		invertMouseYToggle.onValueChanged = (Action<bool>)Delegate.Combine(invertMouseYToggle.onValueChanged, new Action<bool>(this.ApplyInvertMouseYToggle));
		SettingToggle useProgrammerIconsToggle = this._useProgrammerIconsToggle;
		useProgrammerIconsToggle.onValueChanged = (Action<bool>)Delegate.Combine(useProgrammerIconsToggle.onValueChanged, new Action<bool>(this.ApplyUseProgrammerIconsToggle));
		SettingToggle toggleDuckingToggle = this._toggleDuckingToggle;
		toggleDuckingToggle.onValueChanged = (Action<bool>)Delegate.Combine(toggleDuckingToggle.onValueChanged, new Action<bool>(this.ApplyToggleDuckingToggle));
		SettingToggle alwaysShowHolidayShopItemsToggle = this._alwaysShowHolidayShopItemsToggle;
		alwaysShowHolidayShopItemsToggle.onValueChanged = (Action<bool>)Delegate.Combine(alwaysShowHolidayShopItemsToggle.onValueChanged, new Action<bool>(this.ApplyAlwaysShowHolidayShopItemsToggle));
		SettingToggle autoSaveEnabledToggle = this._autoSaveEnabledToggle;
		autoSaveEnabledToggle.onValueChanged = (Action<bool>)Delegate.Combine(autoSaveEnabledToggle.onValueChanged, new Action<bool>(this.ApplyAutoSaveEnabledToggle));
		SettingSlider autoSaveFrequencySlider = this._autoSaveFrequencySlider;
		autoSaveFrequencySlider.onValueChanged = (Action<float>)Delegate.Combine(autoSaveFrequencySlider.onValueChanged, new Action<float>(this.ApplySaveFrequencySlider));
		SettingToggle vSyncEnabledToggle = this._vSyncEnabledToggle;
		vSyncEnabledToggle.onValueChanged = (Action<bool>)Delegate.Combine(vSyncEnabledToggle.onValueChanged, new Action<bool>(this.ApplyVSyncToggle));
		SettingSlider fpsLimitSlider = this._fpsLimitSlider;
		fpsLimitSlider.onValueChanged = (Action<float>)Delegate.Combine(fpsLimitSlider.onValueChanged, new Action<float>(this.ApplyFPSLimit));
		SettingSlider desiredFOVSlider = this._desiredFOVSlider;
		desiredFOVSlider.onValueChanged = (Action<float>)Delegate.Combine(desiredFOVSlider.onValueChanged, new Action<float>(this.ApplyDesiredFOV));
		SettingToggle forceUnlockedCursorToggle = this._forceUnlockedCursorToggle;
		forceUnlockedCursorToggle.onValueChanged = (Action<bool>)Delegate.Combine(forceUnlockedCursorToggle.onValueChanged, new Action<bool>(this.ApplyForceUnlockedCursorToggle));
		SettingSlider movingPhysicsObjectLimitSlider = this._movingPhysicsObjectLimitSlider;
		movingPhysicsObjectLimitSlider.onValueChanged = (Action<float>)Delegate.Combine(movingPhysicsObjectLimitSlider.onValueChanged, new Action<float>(this.ApplyMovingPhysicsObjectLimit));
	}

	// Token: 0x06000590 RID: 1424 RVA: 0x0001D7BC File Offset: 0x0001B9BC
	public void OnEnable()
	{
		this.OnGeneralPagePressed();
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x0001D7C4 File Offset: 0x0001B9C4
	public void OnDisable()
	{
		Singleton<KeybindManager>.Instance.SaveKeybindsIfChanged();
	}

	// Token: 0x06000592 RID: 1426 RVA: 0x0001D7D0 File Offset: 0x0001B9D0
	public void OnGeneralPagePressed()
	{
		this._controlsPage.SetActive(false);
		this._generalPage.SetActive(true);
		this._graphicsPage.SetActive(false);
		this._accessibilityPage.SetActive(false);
		this._keybindsPage.SetActive(false);
	}

	// Token: 0x06000593 RID: 1427 RVA: 0x0001D80E File Offset: 0x0001BA0E
	public void OnControlsPagePressed()
	{
		this._controlsPage.SetActive(true);
		this._generalPage.SetActive(false);
		this._graphicsPage.SetActive(false);
		this._accessibilityPage.SetActive(false);
		this._keybindsPage.SetActive(false);
	}

	// Token: 0x06000594 RID: 1428 RVA: 0x0001D84C File Offset: 0x0001BA4C
	public void OnGraphicsPagePressed()
	{
		this._controlsPage.SetActive(false);
		this._generalPage.SetActive(false);
		this._graphicsPage.SetActive(true);
		this._accessibilityPage.SetActive(false);
		this._keybindsPage.SetActive(false);
	}

	// Token: 0x06000595 RID: 1429 RVA: 0x0001D88A File Offset: 0x0001BA8A
	public void OnAccessibilityPagePressed()
	{
		this._controlsPage.SetActive(false);
		this._generalPage.SetActive(false);
		this._graphicsPage.SetActive(false);
		this._accessibilityPage.SetActive(true);
		this._keybindsPage.SetActive(false);
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x0001D8C8 File Offset: 0x0001BAC8
	public void OnKeybindsPagePressed()
	{
		this._controlsPage.SetActive(false);
		this._generalPage.SetActive(false);
		this._graphicsPage.SetActive(false);
		this._accessibilityPage.SetActive(false);
		this._keybindsPage.SetActive(true);
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x0001D906 File Offset: 0x0001BB06
	private void ApplyMasterVolume(float value)
	{
		AudioListener.volume = value;
	}

	// Token: 0x06000598 RID: 1432 RVA: 0x0001D90E File Offset: 0x0001BB0E
	private void ApplyFPSLimit(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.FPSLimit = (int)value;
			Singleton<SettingsManager>.Instance.ApplySavedSettings();
		}
	}

	// Token: 0x06000599 RID: 1433 RVA: 0x0001D933 File Offset: 0x0001BB33
	private void ApplyMovingPhysicsObjectLimit(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.MovingPhysicsObjectLimit = (int)value;
			Singleton<SettingsManager>.Instance.ApplySavedSettings();
		}
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x0001D958 File Offset: 0x0001BB58
	private void ApplyVSyncToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.VSyncEnabled = value;
			Singleton<SettingsManager>.Instance.ApplySavedSettings();
		}
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x0001D97C File Offset: 0x0001BB7C
	private void ApplyDesiredFOV(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.DesiredFOV = value;
		}
	}

	// Token: 0x0600059C RID: 1436 RVA: 0x0001D996 File Offset: 0x0001BB96
	private void ApplyMouseSensitivity(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.MouseSensitivity = value;
		}
	}

	// Token: 0x0600059D RID: 1437 RVA: 0x0001D9B0 File Offset: 0x0001BBB0
	private void ApplyViewmodelBobScale(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.ViewmodelBobScale = value;
		}
	}

	// Token: 0x0600059E RID: 1438 RVA: 0x0001D9CA File Offset: 0x0001BBCA
	private void ApplyCameraBobScale(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.CameraBobScale = value;
		}
	}

	// Token: 0x0600059F RID: 1439 RVA: 0x0001D9E4 File Offset: 0x0001BBE4
	private void ApplyReverseHotbarScrollingToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.UseReverseHotbarScrolling = value;
		}
	}

	// Token: 0x060005A0 RID: 1440 RVA: 0x0001D9FE File Offset: 0x0001BBFE
	private void ApplyInvertMouseXToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.InvertMouseX = value;
		}
	}

	// Token: 0x060005A1 RID: 1441 RVA: 0x0001DA18 File Offset: 0x0001BC18
	private void ApplyInvertMouseYToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.InvertMouseY = value;
		}
	}

	// Token: 0x060005A2 RID: 1442 RVA: 0x0001DA32 File Offset: 0x0001BC32
	private void ApplyUseProgrammerIconsToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.UseProgrammerIcons = value;
		}
	}

	// Token: 0x060005A3 RID: 1443 RVA: 0x0001DA4C File Offset: 0x0001BC4C
	private void ApplyToggleDuckingToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.ToggleDucking = value;
		}
	}

	// Token: 0x060005A4 RID: 1444 RVA: 0x0001DA68 File Offset: 0x0001BC68
	private void ApplyAlwaysShowHolidayShopItemsToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.AlwaysShowHolidayShopItems = value;
			ComputerShopUI computerShopUI = Object.FindObjectOfType<ComputerShopUI>(true);
			if (computerShopUI != null)
			{
				computerShopUI.SetupCategories();
			}
		}
	}

	// Token: 0x060005A5 RID: 1445 RVA: 0x0001DAA3 File Offset: 0x0001BCA3
	private void ApplyForceUnlockedCursorToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.ForceUnlockedCursor = value;
		}
	}

	// Token: 0x060005A6 RID: 1446 RVA: 0x0001DABD File Offset: 0x0001BCBD
	private void ApplyAutoSaveEnabledToggle(bool value)
	{
		if (Singleton<AutoSaveManager>.Instance != null)
		{
			Singleton<AutoSaveManager>.Instance.AutoSaveEnabled = value;
		}
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x0001DAD7 File Offset: 0x0001BCD7
	private void ApplySaveFrequencySlider(float value)
	{
		if (Singleton<AutoSaveManager>.Instance != null)
		{
			Singleton<AutoSaveManager>.Instance.AutoSaveFrequency = value;
		}
	}

	// Token: 0x040006CC RID: 1740
	[SerializeField]
	private GameObject _generalPage;

	// Token: 0x040006CD RID: 1741
	[SerializeField]
	private GameObject _controlsPage;

	// Token: 0x040006CE RID: 1742
	[SerializeField]
	private GameObject _graphicsPage;

	// Token: 0x040006CF RID: 1743
	[SerializeField]
	private GameObject _accessibilityPage;

	// Token: 0x040006D0 RID: 1744
	[SerializeField]
	private GameObject _keybindsPage;

	// Token: 0x040006D1 RID: 1745
	[SerializeField]
	private SettingSlider _masterVolumeSlider;

	// Token: 0x040006D2 RID: 1746
	[SerializeField]
	private SettingSlider _mouseSensitivitySlider;

	// Token: 0x040006D3 RID: 1747
	[SerializeField]
	private SettingSlider _viewmodelBobScaleSlider;

	// Token: 0x040006D4 RID: 1748
	[SerializeField]
	private SettingSlider _cameraBobScaleSlider;

	// Token: 0x040006D5 RID: 1749
	[SerializeField]
	private SettingToggle _reverseHotbarScrollingToggle;

	// Token: 0x040006D6 RID: 1750
	[SerializeField]
	private SettingToggle _invertMouseXToggle;

	// Token: 0x040006D7 RID: 1751
	[SerializeField]
	private SettingToggle _invertMouseYToggle;

	// Token: 0x040006D8 RID: 1752
	[SerializeField]
	private SettingToggle _useProgrammerIconsToggle;

	// Token: 0x040006D9 RID: 1753
	[SerializeField]
	private SettingToggle _toggleDuckingToggle;

	// Token: 0x040006DA RID: 1754
	[SerializeField]
	private SettingToggle _alwaysShowHolidayShopItemsToggle;

	// Token: 0x040006DB RID: 1755
	[SerializeField]
	private SettingToggle _autoSaveEnabledToggle;

	// Token: 0x040006DC RID: 1756
	[SerializeField]
	private SettingSlider _autoSaveFrequencySlider;

	// Token: 0x040006DD RID: 1757
	[SerializeField]
	private SettingToggle _vSyncEnabledToggle;

	// Token: 0x040006DE RID: 1758
	[SerializeField]
	private SettingSlider _fpsLimitSlider;

	// Token: 0x040006DF RID: 1759
	[SerializeField]
	private SettingSlider _desiredFOVSlider;

	// Token: 0x040006E0 RID: 1760
	[SerializeField]
	private SettingToggle _forceUnlockedCursorToggle;

	// Token: 0x040006E1 RID: 1761
	[SerializeField]
	private SettingSlider _movingPhysicsObjectLimitSlider;
}
