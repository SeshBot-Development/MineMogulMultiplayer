using System;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject _generalPage;

	[SerializeField]
	private GameObject _controlsPage;

	[SerializeField]
	private GameObject _graphicsPage;

	[SerializeField]
	private GameObject _accessibilityPage;

	[SerializeField]
	private GameObject _keybindsPage;

	[SerializeField]
	private SettingSlider _masterVolumeSlider;

	[SerializeField]
	private SettingSlider _mouseSensitivitySlider;

	[SerializeField]
	private SettingSlider _viewmodelBobScaleSlider;

	[SerializeField]
	private SettingSlider _cameraBobScaleSlider;

	[SerializeField]
	private SettingToggle _reverseHotbarScrollingToggle;

	[SerializeField]
	private SettingToggle _invertMouseXToggle;

	[SerializeField]
	private SettingToggle _invertMouseYToggle;

	[SerializeField]
	private SettingToggle _useProgrammerIconsToggle;

	[SerializeField]
	private SettingToggle _toggleDuckingToggle;

	[SerializeField]
	private SettingToggle _alwaysShowHolidayShopItemsToggle;

	[SerializeField]
	private SettingToggle _autoSaveEnabledToggle;

	[SerializeField]
	private SettingSlider _autoSaveFrequencySlider;

	[SerializeField]
	private SettingToggle _vSyncEnabledToggle;

	[SerializeField]
	private SettingSlider _fpsLimitSlider;

	[SerializeField]
	private SettingSlider _desiredFOVSlider;

	[SerializeField]
	private SettingToggle _forceUnlockedCursorToggle;

	[SerializeField]
	private SettingSlider _movingPhysicsObjectLimitSlider;

	private void Start()
	{
		SettingSlider masterVolumeSlider = _masterVolumeSlider;
		masterVolumeSlider.onValueChanged = (Action<float>)Delegate.Combine(masterVolumeSlider.onValueChanged, new Action<float>(ApplyMasterVolume));
		SettingSlider mouseSensitivitySlider = _mouseSensitivitySlider;
		mouseSensitivitySlider.onValueChanged = (Action<float>)Delegate.Combine(mouseSensitivitySlider.onValueChanged, new Action<float>(ApplyMouseSensitivity));
		SettingSlider viewmodelBobScaleSlider = _viewmodelBobScaleSlider;
		viewmodelBobScaleSlider.onValueChanged = (Action<float>)Delegate.Combine(viewmodelBobScaleSlider.onValueChanged, new Action<float>(ApplyViewmodelBobScale));
		SettingSlider cameraBobScaleSlider = _cameraBobScaleSlider;
		cameraBobScaleSlider.onValueChanged = (Action<float>)Delegate.Combine(cameraBobScaleSlider.onValueChanged, new Action<float>(ApplyCameraBobScale));
		SettingToggle reverseHotbarScrollingToggle = _reverseHotbarScrollingToggle;
		reverseHotbarScrollingToggle.onValueChanged = (Action<bool>)Delegate.Combine(reverseHotbarScrollingToggle.onValueChanged, new Action<bool>(ApplyReverseHotbarScrollingToggle));
		SettingToggle invertMouseXToggle = _invertMouseXToggle;
		invertMouseXToggle.onValueChanged = (Action<bool>)Delegate.Combine(invertMouseXToggle.onValueChanged, new Action<bool>(ApplyInvertMouseXToggle));
		SettingToggle invertMouseYToggle = _invertMouseYToggle;
		invertMouseYToggle.onValueChanged = (Action<bool>)Delegate.Combine(invertMouseYToggle.onValueChanged, new Action<bool>(ApplyInvertMouseYToggle));
		SettingToggle useProgrammerIconsToggle = _useProgrammerIconsToggle;
		useProgrammerIconsToggle.onValueChanged = (Action<bool>)Delegate.Combine(useProgrammerIconsToggle.onValueChanged, new Action<bool>(ApplyUseProgrammerIconsToggle));
		SettingToggle toggleDuckingToggle = _toggleDuckingToggle;
		toggleDuckingToggle.onValueChanged = (Action<bool>)Delegate.Combine(toggleDuckingToggle.onValueChanged, new Action<bool>(ApplyToggleDuckingToggle));
		SettingToggle alwaysShowHolidayShopItemsToggle = _alwaysShowHolidayShopItemsToggle;
		alwaysShowHolidayShopItemsToggle.onValueChanged = (Action<bool>)Delegate.Combine(alwaysShowHolidayShopItemsToggle.onValueChanged, new Action<bool>(ApplyAlwaysShowHolidayShopItemsToggle));
		SettingToggle autoSaveEnabledToggle = _autoSaveEnabledToggle;
		autoSaveEnabledToggle.onValueChanged = (Action<bool>)Delegate.Combine(autoSaveEnabledToggle.onValueChanged, new Action<bool>(ApplyAutoSaveEnabledToggle));
		SettingSlider autoSaveFrequencySlider = _autoSaveFrequencySlider;
		autoSaveFrequencySlider.onValueChanged = (Action<float>)Delegate.Combine(autoSaveFrequencySlider.onValueChanged, new Action<float>(ApplySaveFrequencySlider));
		SettingToggle vSyncEnabledToggle = _vSyncEnabledToggle;
		vSyncEnabledToggle.onValueChanged = (Action<bool>)Delegate.Combine(vSyncEnabledToggle.onValueChanged, new Action<bool>(ApplyVSyncToggle));
		SettingSlider fpsLimitSlider = _fpsLimitSlider;
		fpsLimitSlider.onValueChanged = (Action<float>)Delegate.Combine(fpsLimitSlider.onValueChanged, new Action<float>(ApplyFPSLimit));
		SettingSlider desiredFOVSlider = _desiredFOVSlider;
		desiredFOVSlider.onValueChanged = (Action<float>)Delegate.Combine(desiredFOVSlider.onValueChanged, new Action<float>(ApplyDesiredFOV));
		SettingToggle forceUnlockedCursorToggle = _forceUnlockedCursorToggle;
		forceUnlockedCursorToggle.onValueChanged = (Action<bool>)Delegate.Combine(forceUnlockedCursorToggle.onValueChanged, new Action<bool>(ApplyForceUnlockedCursorToggle));
		SettingSlider movingPhysicsObjectLimitSlider = _movingPhysicsObjectLimitSlider;
		movingPhysicsObjectLimitSlider.onValueChanged = (Action<float>)Delegate.Combine(movingPhysicsObjectLimitSlider.onValueChanged, new Action<float>(ApplyMovingPhysicsObjectLimit));
	}

	public void OnEnable()
	{
		OnGeneralPagePressed();
	}

	public void OnDisable()
	{
		Singleton<KeybindManager>.Instance.SaveKeybindsIfChanged();
	}

	public void OnGeneralPagePressed()
	{
		_controlsPage.SetActive(value: false);
		_generalPage.SetActive(value: true);
		_graphicsPage.SetActive(value: false);
		_accessibilityPage.SetActive(value: false);
		_keybindsPage.SetActive(value: false);
	}

	public void OnControlsPagePressed()
	{
		_controlsPage.SetActive(value: true);
		_generalPage.SetActive(value: false);
		_graphicsPage.SetActive(value: false);
		_accessibilityPage.SetActive(value: false);
		_keybindsPage.SetActive(value: false);
	}

	public void OnGraphicsPagePressed()
	{
		_controlsPage.SetActive(value: false);
		_generalPage.SetActive(value: false);
		_graphicsPage.SetActive(value: true);
		_accessibilityPage.SetActive(value: false);
		_keybindsPage.SetActive(value: false);
	}

	public void OnAccessibilityPagePressed()
	{
		_controlsPage.SetActive(value: false);
		_generalPage.SetActive(value: false);
		_graphicsPage.SetActive(value: false);
		_accessibilityPage.SetActive(value: true);
		_keybindsPage.SetActive(value: false);
	}

	public void OnKeybindsPagePressed()
	{
		_controlsPage.SetActive(value: false);
		_generalPage.SetActive(value: false);
		_graphicsPage.SetActive(value: false);
		_accessibilityPage.SetActive(value: false);
		_keybindsPage.SetActive(value: true);
	}

	private void ApplyMasterVolume(float value)
	{
		AudioListener.volume = value;
	}

	private void ApplyFPSLimit(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.FPSLimit = (int)value;
			Singleton<SettingsManager>.Instance.ApplySavedSettings();
		}
	}

	private void ApplyMovingPhysicsObjectLimit(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.MovingPhysicsObjectLimit = (int)value;
			Singleton<SettingsManager>.Instance.ApplySavedSettings();
		}
	}

	private void ApplyVSyncToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.VSyncEnabled = value;
			Singleton<SettingsManager>.Instance.ApplySavedSettings();
		}
	}

	private void ApplyDesiredFOV(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.DesiredFOV = value;
		}
	}

	private void ApplyMouseSensitivity(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.MouseSensitivity = value;
		}
	}

	private void ApplyViewmodelBobScale(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.ViewmodelBobScale = value;
		}
	}

	private void ApplyCameraBobScale(float value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.CameraBobScale = value;
		}
	}

	private void ApplyReverseHotbarScrollingToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.UseReverseHotbarScrolling = value;
		}
	}

	private void ApplyInvertMouseXToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.InvertMouseX = value;
		}
	}

	private void ApplyInvertMouseYToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.InvertMouseY = value;
		}
	}

	private void ApplyUseProgrammerIconsToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.UseProgrammerIcons = value;
		}
	}

	private void ApplyToggleDuckingToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.ToggleDucking = value;
		}
	}

	private void ApplyAlwaysShowHolidayShopItemsToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.AlwaysShowHolidayShopItems = value;
			ComputerShopUI computerShopUI = UnityEngine.Object.FindObjectOfType<ComputerShopUI>(includeInactive: true);
			if (computerShopUI != null)
			{
				computerShopUI.SetupCategories();
			}
		}
	}

	private void ApplyForceUnlockedCursorToggle(bool value)
	{
		if (Singleton<SettingsManager>.Instance != null)
		{
			Singleton<SettingsManager>.Instance.ForceUnlockedCursor = value;
		}
	}

	private void ApplyAutoSaveEnabledToggle(bool value)
	{
		if (Singleton<AutoSaveManager>.Instance != null)
		{
			Singleton<AutoSaveManager>.Instance.AutoSaveEnabled = value;
		}
	}

	private void ApplySaveFrequencySlider(float value)
	{
		if (Singleton<AutoSaveManager>.Instance != null)
		{
			Singleton<AutoSaveManager>.Instance.AutoSaveFrequency = value;
		}
	}
}
