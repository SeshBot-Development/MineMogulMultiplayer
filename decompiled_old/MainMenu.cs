using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public GameObject SettingsMenu;

	public GameObject SaveGameMenu;

	public Button LoadGameButton;

	public GameObject MainUIPanel;

	public GameObject EarlyAccessPopup;

	public GameObject RoadmapPanel;

	public TMP_Text VersionNumberText;

	public MainMenuElevator Elevator;

	public Image FadeOverlay;

	public GameObject NewGameMenu;

	public SteamNewsFetcher SteamNewsFetcher;

	public GameObject NewMapIcon;

	public GameObject DemoLogo;

	public GameObject FullGameLogo;

	private bool _hasStartedElevatorAnimation;

	private void OnEnable()
	{
		Color color = FadeOverlay.color;
		FadeOverlay.color = new Color(color.r, color.g, color.b, 0f);
		FadeOverlay.enabled = false;
		SettingsMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: false);
		EarlyAccessPopup.SetActive(value: true);
		RoadmapPanel.SetActive(value: false);
		SaveGameMenu.SetActive(value: false);
		NewGameMenu.SetActive(value: false);
		VersionNumberText.text = Singleton<VersionManager>.Instance?.GetFormattedVersionText();
		DemoLogo.SetActive(Singleton<DemoManager>.Instance.IsDemoVersion);
		FullGameLogo.SetActive(!Singleton<DemoManager>.Instance.IsDemoVersion);
		LoadGameButton.interactable = SavingLoadingManager.HasAnySaveFiles();
		NewMapIcon.gameObject.SetActive(Singleton<MenuDataManager>.Instance.ShouldShowNewMapIcon());
		SteamNewsFetcher.ToggleNewsPanel(enabled: false);
		StartCoroutine(Singleton<MenuDataManager>.Instance.ShouldShowLatestSteamNewsPost(delegate(bool show)
		{
			if (show)
			{
				SteamNewsFetcher.ToggleNewsPanel(enabled: true);
			}
		}));
	}

	public void OnNewGamePressed()
	{
		NewGameMenu.SetActive(value: true);
		MainUIPanel.SetActive(value: false);
		NewMapIcon.gameObject.SetActive(value: false);
		Singleton<MenuDataManager>.Instance.HideNewMapIcon();
	}

	public void OnRoadmapPressed()
	{
		RoadmapPanel.SetActive(value: true);
		MainUIPanel.SetActive(value: false);
	}

	public void CloseRoadmap()
	{
		RoadmapPanel.SetActive(value: false);
		SettingsMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: true);
	}

	public void OnDontStartNewGamePressed()
	{
		NewGameMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: true);
	}

	public IEnumerator PlayAnimationThenLoadScene()
	{
		yield return StartCoroutine(PlayElevatorLowerAnimation());
		SceneManager.LoadScene("Gameplay");
	}

	public IEnumerator PlayElevatorLowerAnimation()
	{
		if (!_hasStartedElevatorAnimation)
		{
			_hasStartedElevatorAnimation = true;
			Elevator.DropElevator();
			Object.FindObjectOfType<MainMenuCameraShaker>()?.ApplyViewPunch(new Vector3(1.7f, 0.1f, 0.3f));
			yield return new WaitForSeconds(1f);
			FadeOverlay.enabled = true;
			float fadeDuration = 0.5f;
			float elapsed = 0f;
			Color color = FadeOverlay.color;
			float startAlpha = color.a;
			float targetAlpha = 1f;
			while (elapsed < fadeDuration)
			{
				elapsed += Time.deltaTime;
				float num = Mathf.Clamp01(elapsed / fadeDuration);
				float a = Mathf.Lerp(startAlpha, targetAlpha, num * num);
				FadeOverlay.color = new Color(color.r, color.g, color.b, a);
				yield return null;
			}
			FadeOverlay.color = new Color(color.r, color.g, color.b, targetAlpha);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void OnOpenSaveMenuPressed()
	{
		NewGameMenu.SetActive(value: false);
		SaveGameMenu.SetActive(value: true);
		MainUIPanel.SetActive(value: false);
	}

	public void OnOpenSettingsPressed()
	{
		SettingsMenu.SetActive(value: true);
		MainUIPanel.SetActive(value: false);
		SaveGameMenu.SetActive(value: false);
		NewGameMenu.SetActive(value: false);
	}

	public void OnCloseSettingsPressed()
	{
		SettingsMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: true);
	}

	public void OnCloseLoadMenuPressed()
	{
		SaveGameMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: true);
	}

	public void OnCloseEarlyAccessPopupPressed()
	{
		EarlyAccessPopup.SetActive(value: false);
		SettingsMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: true);
	}

	public void OnQuitPressed()
	{
		Application.Quit();
	}

	public void OnWishlistPressed()
	{
		Application.OpenURL("https://store.steampowered.com/app/3846120/MineMogul/");
	}

	public void OnSteamDiscussionsPressed()
	{
		Application.OpenURL("https://steamcommunity.com/app/3846120/discussions/");
	}

	public void OnDiscordPressed()
	{
		Application.OpenURL("https://discord.gg/F3cdWuTAEJ");
	}

	public void OnFeedbackSurveyPressed()
	{
		string versionNumber = Singleton<VersionManager>.Instance.VersionNumber;
		_ = "Game Version: " + Singleton<VersionManager>.Instance.VersionNumber + "%0AOS: " + SystemInfo.operatingSystem + "%0ADevice Model: " + SystemInfo.deviceModel + "%0ACPU: " + SystemInfo.processorModel + "%0A" + $"GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB VRAM)%0A" + $"RAM: {SystemInfo.systemMemorySize} MB%0A" + $"Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height} @ {Screen.currentResolution.refreshRate}Hz%0A" + $"Graphics API: {SystemInfo.graphicsDeviceType}%0A" + $"Platform: {Application.platform}%0A" + $"Language: {Application.systemLanguage}";
		string text = "Game Version: " + Singleton<VersionManager>.Instance.VersionNumber + "%0AOS: " + SystemInfo.operatingSystem + "%0A" + $"Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}%0A" + $"Graphics API: {SystemInfo.graphicsDeviceType}%0A" + $"Platform: {Application.platform}%0A" + $"Language: {Application.systemLanguage}";
		Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLScqhhi5Z6H83GgzA9lLyBysm6zKLz1bVjF71J8jOQQQ8A03Dg/viewform?usp=pp_url&entry.1792332498=" + versionNumber + "&entry.840199481=" + text);
	}
}
