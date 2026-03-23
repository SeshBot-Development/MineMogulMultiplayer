using System;
using UnityEngine;

// Token: 0x020000D3 RID: 211
[DefaultExecutionOrder(-1)]
public class SettingsManager : Singleton<SettingsManager>
{
	// Token: 0x06000588 RID: 1416 RVA: 0x0001D2B3 File Offset: 0x0001B4B3
	protected override void Awake()
	{
		base.Awake();
		if (Singleton<SettingsManager>.Instance != this)
		{
			return;
		}
		this.ApplySavedSettings();
	}

	// Token: 0x06000589 RID: 1417 RVA: 0x0001D2D0 File Offset: 0x0001B4D0
	public void ApplySavedSettings()
	{
		AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		this.MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
		this.CameraBobScale = PlayerPrefs.GetFloat("CameraBobScale", 1f);
		this.ViewmodelBobScale = PlayerPrefs.GetFloat("ViewmodelBobScale", 1f);
		this.UseReverseHotbarScrolling = PlayerPrefs.GetInt("ReverseHotbarScrolling", 0) > 0;
		this.InvertMouseX = PlayerPrefs.GetInt("InvertMouseX", 0) > 0;
		this.InvertMouseY = PlayerPrefs.GetInt("InvertMouseY", 0) > 0;
		this.UseProgrammerIcons = PlayerPrefs.GetInt("UseProgrammerIcons", 0) > 0;
		this.AlwaysShowHolidayShopItems = PlayerPrefs.GetInt("AlwaysShowHolidayShopItems", 0) > 0;
		this.ToggleDucking = PlayerPrefs.GetInt("ToggleDucking", 0) > 0;
		this.VSyncEnabled = PlayerPrefs.GetInt("VSyncEnabled", 1) > 0;
		this.FPSLimit = PlayerPrefs.GetInt("FPSLimit", 300);
		this.DesiredFOV = PlayerPrefs.GetFloat("DesiredFOV", 80f);
		this.ForceUnlockedCursor = PlayerPrefs.GetInt("ForceUnlockedCursor", 0) > 0;
		this.MovingPhysicsObjectLimit = PlayerPrefs.GetInt("MovingPhysicsObjectLimit", 2000);
		this.SetVsyncAndFPSLimit();
	}

	// Token: 0x0600058A RID: 1418 RVA: 0x0001D435 File Offset: 0x0001B635
	public void SetVsyncAndFPSLimit()
	{
		QualitySettings.vSyncCount = (this.VSyncEnabled ? 1 : 0);
		Application.targetFrameRate = this.FPSLimit;
	}

	// Token: 0x0600058B RID: 1419 RVA: 0x0001D453 File Offset: 0x0001B653
	public void SetPauseMenuFPSLimit()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = Math.Min(50, this.FPSLimit);
	}

	// Token: 0x0600058C RID: 1420 RVA: 0x0001D470 File Offset: 0x0001B670
	public static void SetDisplayMode(int index)
	{
		FullScreenMode fullScreenMode;
		switch (index)
		{
		case 0:
			fullScreenMode = FullScreenMode.Windowed;
			break;
		case 1:
			fullScreenMode = FullScreenMode.FullScreenWindow;
			break;
		case 2:
			fullScreenMode = FullScreenMode.ExclusiveFullScreen;
			break;
		default:
			fullScreenMode = FullScreenMode.Windowed;
			break;
		}
		FullScreenMode fullScreenMode2 = fullScreenMode;
		if (fullScreenMode2 == FullScreenMode.FullScreenWindow)
		{
			int systemWidth = Display.main.systemWidth;
			int systemHeight = Display.main.systemHeight;
			Screen.SetResolution(systemWidth, systemHeight, fullScreenMode2);
		}
		else
		{
			Screen.fullScreenMode = fullScreenMode2;
		}
		PlayerPrefs.SetInt("DisplayMode", index);
	}

	// Token: 0x0600058D RID: 1421 RVA: 0x0001D4D4 File Offset: 0x0001B6D4
	public static bool ShouldUseProgrammerIcons()
	{
		return Singleton<SettingsManager>.Instance != null && Singleton<SettingsManager>.Instance.UseProgrammerIcons;
	}

	// Token: 0x040006BE RID: 1726
	public float MouseSensitivity;

	// Token: 0x040006BF RID: 1727
	public bool UseReverseHotbarScrolling;

	// Token: 0x040006C0 RID: 1728
	public bool InvertMouseX;

	// Token: 0x040006C1 RID: 1729
	public bool InvertMouseY;

	// Token: 0x040006C2 RID: 1730
	public bool UseProgrammerIcons;

	// Token: 0x040006C3 RID: 1731
	public bool AlwaysShowHolidayShopItems;

	// Token: 0x040006C4 RID: 1732
	public bool ToggleDucking;

	// Token: 0x040006C5 RID: 1733
	public float CameraBobScale;

	// Token: 0x040006C6 RID: 1734
	public float ViewmodelBobScale;

	// Token: 0x040006C7 RID: 1735
	public bool VSyncEnabled;

	// Token: 0x040006C8 RID: 1736
	public int FPSLimit = 300;

	// Token: 0x040006C9 RID: 1737
	public bool ForceUnlockedCursor;

	// Token: 0x040006CA RID: 1738
	public float DesiredFOV = 80f;

	// Token: 0x040006CB RID: 1739
	public int MovingPhysicsObjectLimit = 2000;
}
