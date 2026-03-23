using System;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SettingsManager : Singleton<SettingsManager>
{
	public float MouseSensitivity;

	public bool UseReverseHotbarScrolling;

	public bool InvertMouseX;

	public bool InvertMouseY;

	public bool UseProgrammerIcons;

	public bool AlwaysShowHolidayShopItems;

	public bool ToggleDucking;

	public float CameraBobScale;

	public float ViewmodelBobScale;

	public bool VSyncEnabled;

	public int FPSLimit = 300;

	public bool ForceUnlockedCursor;

	public float DesiredFOV = 80f;

	public int MovingPhysicsObjectLimit = 2000;

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<SettingsManager>.Instance != this))
		{
			ApplySavedSettings();
		}
	}

	public void ApplySavedSettings()
	{
		AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
		CameraBobScale = PlayerPrefs.GetFloat("CameraBobScale", 1f);
		ViewmodelBobScale = PlayerPrefs.GetFloat("ViewmodelBobScale", 1f);
		UseReverseHotbarScrolling = PlayerPrefs.GetInt("ReverseHotbarScrolling", 0) > 0;
		InvertMouseX = PlayerPrefs.GetInt("InvertMouseX", 0) > 0;
		InvertMouseY = PlayerPrefs.GetInt("InvertMouseY", 0) > 0;
		UseProgrammerIcons = PlayerPrefs.GetInt("UseProgrammerIcons", 0) > 0;
		AlwaysShowHolidayShopItems = PlayerPrefs.GetInt("AlwaysShowHolidayShopItems", 0) > 0;
		ToggleDucking = PlayerPrefs.GetInt("ToggleDucking", 0) > 0;
		VSyncEnabled = PlayerPrefs.GetInt("VSyncEnabled", 1) > 0;
		FPSLimit = PlayerPrefs.GetInt("FPSLimit", 300);
		DesiredFOV = PlayerPrefs.GetFloat("DesiredFOV", 80f);
		ForceUnlockedCursor = PlayerPrefs.GetInt("ForceUnlockedCursor", 0) > 0;
		MovingPhysicsObjectLimit = PlayerPrefs.GetInt("MovingPhysicsObjectLimit", 2000);
		SetVsyncAndFPSLimit();
	}

	public void SetVsyncAndFPSLimit()
	{
		QualitySettings.vSyncCount = (VSyncEnabled ? 1 : 0);
		Application.targetFrameRate = FPSLimit;
	}

	public void SetPauseMenuFPSLimit()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = Math.Min(50, FPSLimit);
	}

	public static void SetDisplayMode(int index)
	{
		FullScreenMode fullScreenMode = index switch
		{
			0 => FullScreenMode.Windowed, 
			1 => FullScreenMode.FullScreenWindow, 
			2 => FullScreenMode.ExclusiveFullScreen, 
			_ => FullScreenMode.Windowed, 
		};
		if (fullScreenMode == FullScreenMode.FullScreenWindow)
		{
			int systemWidth = Display.main.systemWidth;
			int systemHeight = Display.main.systemHeight;
			Screen.SetResolution(systemWidth, systemHeight, fullScreenMode);
		}
		else
		{
			Screen.fullScreenMode = fullScreenMode;
		}
		PlayerPrefs.SetInt("DisplayMode", index);
	}

	public static bool ShouldUseProgrammerIcons()
	{
		if (!(Singleton<SettingsManager>.Instance != null))
		{
			return false;
		}
		return Singleton<SettingsManager>.Instance.UseProgrammerIcons;
	}
}
