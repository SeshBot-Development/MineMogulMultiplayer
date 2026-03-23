using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000067 RID: 103
public class MainMenu : MonoBehaviour
{
	// Token: 0x060002A9 RID: 681 RVA: 0x0000CDD0 File Offset: 0x0000AFD0
	private void OnEnable()
	{
		Color color = this.FadeOverlay.color;
		this.FadeOverlay.color = new Color(color.r, color.g, color.b, 0f);
		this.FadeOverlay.enabled = false;
		this.SettingsMenu.SetActive(false);
		this.MainUIPanel.SetActive(false);
		this.EarlyAccessPopup.SetActive(true);
		this.RoadmapPanel.SetActive(false);
		this.SaveGameMenu.SetActive(false);
		this.NewGameMenu.SetActive(false);
		TMP_Text versionNumberText = this.VersionNumberText;
		VersionManager instance = Singleton<VersionManager>.Instance;
		versionNumberText.text = ((instance != null) ? instance.GetFormattedVersionText() : null);
		this.LoadGameButton.interactable = SavingLoadingManager.HasAnySaveFiles();
		this.NewMapIcon.gameObject.SetActive(Singleton<MenuDataManager>.Instance.ShouldShowNewMapIcon());
		this.SteamNewsFetcher.ToggleNewsPanel(false);
		base.StartCoroutine(Singleton<MenuDataManager>.Instance.ShouldShowLatestSteamNewsPost(delegate(bool show)
		{
			if (show)
			{
				this.SteamNewsFetcher.ToggleNewsPanel(true);
			}
		}));
	}

	// Token: 0x060002AA RID: 682 RVA: 0x0000CED3 File Offset: 0x0000B0D3
	public void OnNewGamePressed()
	{
		this.NewGameMenu.SetActive(true);
		this.MainUIPanel.SetActive(false);
		this.NewMapIcon.gameObject.SetActive(false);
		Singleton<MenuDataManager>.Instance.HideNewMapIcon();
	}

	// Token: 0x060002AB RID: 683 RVA: 0x0000CF08 File Offset: 0x0000B108
	public void OnRoadmapPressed()
	{
		this.RoadmapPanel.SetActive(true);
		this.MainUIPanel.SetActive(false);
	}

	// Token: 0x060002AC RID: 684 RVA: 0x0000CF22 File Offset: 0x0000B122
	public void CloseRoadmap()
	{
		this.RoadmapPanel.SetActive(false);
		this.SettingsMenu.SetActive(false);
		this.MainUIPanel.SetActive(true);
	}

	// Token: 0x060002AD RID: 685 RVA: 0x0000CF48 File Offset: 0x0000B148
	public void OnDontStartNewGamePressed()
	{
		this.NewGameMenu.SetActive(false);
		this.MainUIPanel.SetActive(true);
	}

	// Token: 0x060002AE RID: 686 RVA: 0x0000CF62 File Offset: 0x0000B162
	public IEnumerator PlayAnimationThenLoadScene()
	{
		yield return base.StartCoroutine(this.PlayElevatorLowerAnimation());
		SceneManager.LoadScene("Gameplay");
		yield break;
	}

	// Token: 0x060002AF RID: 687 RVA: 0x0000CF71 File Offset: 0x0000B171
	public IEnumerator PlayElevatorLowerAnimation()
	{
		if (this._hasStartedElevatorAnimation)
		{
			yield break;
		}
		this._hasStartedElevatorAnimation = true;
		this.Elevator.DropElevator();
		MainMenuCameraShaker mainMenuCameraShaker = Object.FindObjectOfType<MainMenuCameraShaker>();
		if (mainMenuCameraShaker != null)
		{
			mainMenuCameraShaker.ApplyViewPunch(new Vector3(1.7f, 0.1f, 0.3f));
		}
		yield return new WaitForSeconds(1f);
		this.FadeOverlay.enabled = true;
		float fadeDuration = 0.5f;
		float elapsed = 0f;
		Color color = this.FadeOverlay.color;
		float startAlpha = color.a;
		float targetAlpha = 1f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / fadeDuration);
			float num2 = Mathf.Lerp(startAlpha, targetAlpha, num * num);
			this.FadeOverlay.color = new Color(color.r, color.g, color.b, num2);
			yield return null;
		}
		this.FadeOverlay.color = new Color(color.r, color.g, color.b, targetAlpha);
		yield return new WaitForSeconds(0.5f);
		yield break;
	}

	// Token: 0x060002B0 RID: 688 RVA: 0x0000CF80 File Offset: 0x0000B180
	public void OnOpenSaveMenuPressed()
	{
		this.NewGameMenu.SetActive(false);
		this.SaveGameMenu.SetActive(true);
		this.MainUIPanel.SetActive(false);
	}

	// Token: 0x060002B1 RID: 689 RVA: 0x0000CFA6 File Offset: 0x0000B1A6
	public void OnOpenSettingsPressed()
	{
		this.SettingsMenu.SetActive(true);
		this.MainUIPanel.SetActive(false);
		this.SaveGameMenu.SetActive(false);
		this.NewGameMenu.SetActive(false);
	}

	// Token: 0x060002B2 RID: 690 RVA: 0x0000CFD8 File Offset: 0x0000B1D8
	public void OnCloseSettingsPressed()
	{
		this.SettingsMenu.SetActive(false);
		this.MainUIPanel.SetActive(true);
	}

	// Token: 0x060002B3 RID: 691 RVA: 0x0000CFF2 File Offset: 0x0000B1F2
	public void OnCloseLoadMenuPressed()
	{
		this.SaveGameMenu.SetActive(false);
		this.MainUIPanel.SetActive(true);
	}

	// Token: 0x060002B4 RID: 692 RVA: 0x0000D00C File Offset: 0x0000B20C
	public void OnCloseEarlyAccessPopupPressed()
	{
		this.EarlyAccessPopup.SetActive(false);
		this.SettingsMenu.SetActive(false);
		this.MainUIPanel.SetActive(true);
	}

	// Token: 0x060002B5 RID: 693 RVA: 0x0000D032 File Offset: 0x0000B232
	public void OnQuitPressed()
	{
		Application.Quit();
	}

	// Token: 0x060002B6 RID: 694 RVA: 0x0000D039 File Offset: 0x0000B239
	public void OnWishlistPressed()
	{
		Application.OpenURL("https://store.steampowered.com/app/3846120/MineMogul/");
	}

	// Token: 0x060002B7 RID: 695 RVA: 0x0000D045 File Offset: 0x0000B245
	public void OnSteamDiscussionsPressed()
	{
		Application.OpenURL("https://steamcommunity.com/app/3846120/discussions/");
	}

	// Token: 0x060002B8 RID: 696 RVA: 0x0000D051 File Offset: 0x0000B251
	public void OnDiscordPressed()
	{
		Application.OpenURL("https://discord.gg/F3cdWuTAEJ");
	}

	// Token: 0x060002B9 RID: 697 RVA: 0x0000D060 File Offset: 0x0000B260
	public void OnFeedbackSurveyPressed()
	{
		string versionNumber = Singleton<VersionManager>.Instance.VersionNumber;
		string.Concat(new string[]
		{
			"Game Version: ",
			Singleton<VersionManager>.Instance.VersionNumber,
			"%0AOS: ",
			SystemInfo.operatingSystem,
			"%0ADevice Model: ",
			SystemInfo.deviceModel,
			"%0ACPU: ",
			SystemInfo.processorModel,
			"%0A",
			string.Format("GPU: {0} ({1} MB VRAM)%0A", SystemInfo.graphicsDeviceName, SystemInfo.graphicsMemorySize),
			string.Format("RAM: {0} MB%0A", SystemInfo.systemMemorySize),
			string.Format("Resolution: {0}x{1} @ {2}Hz%0A", Screen.currentResolution.width, Screen.currentResolution.height, Screen.currentResolution.refreshRate),
			string.Format("Graphics API: {0}%0A", SystemInfo.graphicsDeviceType),
			string.Format("Platform: {0}%0A", Application.platform),
			string.Format("Language: {0}", Application.systemLanguage)
		});
		string text = string.Concat(new string[]
		{
			"Game Version: ",
			Singleton<VersionManager>.Instance.VersionNumber,
			"%0AOS: ",
			SystemInfo.operatingSystem,
			"%0A",
			string.Format("Resolution: {0}x{1}%0A", Screen.currentResolution.width, Screen.currentResolution.height),
			string.Format("Graphics API: {0}%0A", SystemInfo.graphicsDeviceType),
			string.Format("Platform: {0}%0A", Application.platform),
			string.Format("Language: {0}", Application.systemLanguage)
		});
		Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLScqhhi5Z6H83GgzA9lLyBysm6zKLz1bVjF71J8jOQQQ8A03Dg/viewform?usp=pp_url&entry.1792332498=" + versionNumber + "&entry.840199481=" + text);
	}

	// Token: 0x0400027C RID: 636
	public GameObject SettingsMenu;

	// Token: 0x0400027D RID: 637
	public GameObject SaveGameMenu;

	// Token: 0x0400027E RID: 638
	public Button LoadGameButton;

	// Token: 0x0400027F RID: 639
	public GameObject MainUIPanel;

	// Token: 0x04000280 RID: 640
	public GameObject EarlyAccessPopup;

	// Token: 0x04000281 RID: 641
	public GameObject RoadmapPanel;

	// Token: 0x04000282 RID: 642
	public TMP_Text VersionNumberText;

	// Token: 0x04000283 RID: 643
	public MainMenuElevator Elevator;

	// Token: 0x04000284 RID: 644
	public Image FadeOverlay;

	// Token: 0x04000285 RID: 645
	public GameObject NewGameMenu;

	// Token: 0x04000286 RID: 646
	public SteamNewsFetcher SteamNewsFetcher;

	// Token: 0x04000287 RID: 647
	public GameObject NewMapIcon;

	// Token: 0x04000288 RID: 648
	private bool _hasStartedElevatorAnimation;
}
