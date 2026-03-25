using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	public GameObject QuitMenu;

	public GameObject ReturnToMainMenuMenu;

	public GameObject MainUIPanel;

	public GameObject SettingsMenu;

	public GameObject ClearAllOrePopup;

	public TMP_Text TotalOrePiecesText;

	public TMP_Text VersionNumberText;

	public GameObject SaveGameMenu;

	public TMP_Text QuitWarningLastSaveTimeText;

	public TMP_Text MainMenuWarningLastSaveTimeText;

	public TMP_Text ToggleHudText;

	public TMP_Text LastSaveTimeText;

	public ErrorMessagePopup ErrorMessagePopup;

	public InfoMessagePopup InfoMessagePopup;

	public GameObject DemoLogo;

	public GameObject FullGameLogo;

	private float _originalTimeScale = 1f;

	private void OnEnable()
	{
		QuitMenu.SetActive(value: false);
		ReturnToMainMenuMenu.SetActive(value: false);
		SettingsMenu.SetActive(value: false);
		MainUIPanel.SetActive(value: true);
		SaveGameMenu.SetActive(value: false);
		ClearAllOrePopup.SetActive(value: false);
		ErrorMessagePopup.gameObject.SetActive(value: false);
		InfoMessagePopup.gameObject.SetActive(value: false);
		DemoLogo.SetActive(Singleton<DemoManager>.Instance.IsDemoVersion);
		FullGameLogo.SetActive(!Singleton<DemoManager>.Instance.IsDemoVersion);
		VersionNumberText.text = Singleton<VersionManager>.Instance.GetFormattedVersionText();
		RefreshToggleHudText();
		_originalTimeScale = Time.timeScale;
		if (_originalTimeScale <= 0f)
		{
			_originalTimeScale = 1f;
		}
		Time.timeScale = 0f;
		Singleton<SettingsManager>.Instance.SetPauseMenuFPSLimit();
		Singleton<GameManager>.Instance.OnGamePauseToggled(isPaused: true);
		if (Singleton<SavingLoadingManager>.Instance.LastSaveTime != 0f)
		{
			LastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		}
	}

	private void Update()
	{
		if (Singleton<DebugManager>.Instance.DevModeEnabled)
		{
			TotalOrePiecesText.text = $" (Active Ore Physics Objects: {OrePiece.AllOrePieces.Count.ToString()}, Pooled: {Singleton<OrePiecePoolManager>.Instance.GetInactiveCount()} )";
		}
		else
		{
			TotalOrePiecesText.text = " (Active Ore Physics Objects: " + OrePiece.AllOrePieces.Count + " )";
		}
	}

	private void OnDisable()
	{
		Time.timeScale = _originalTimeScale;
		Singleton<SettingsManager>.Instance.SetVsyncAndFPSLimit();
		Singleton<GameManager>.Instance.OnGamePauseToggled(isPaused: false);
	}

	public void OnResumePressed()
	{
		Singleton<UIManager>.Instance.HudObject.SetActive(!Singleton<UIManager>.Instance.HudIsHidden);
		base.gameObject.SetActive(value: false);
	}

	public void DisableSubMenus()
	{
		QuitMenu.SetActive(value: false);
		ReturnToMainMenuMenu.SetActive(value: false);
		SettingsMenu.SetActive(value: false);
		SaveGameMenu.SetActive(value: false);
		ClearAllOrePopup.SetActive(value: false);
	}

	public void OnClearOreMenuPressed()
	{
		bool activeSelf = ClearAllOrePopup.activeSelf;
		DisableSubMenus();
		ClearAllOrePopup.SetActive(!activeSelf);
	}

	public void ShowErrorPopup(string message, string stackTrace)
	{
		Singleton<UIManager>.Instance.ShowPauseMenu(show: true);
		DisableSubMenus();
		MainUIPanel.SetActive(value: false);
		ErrorMessagePopup.ShowErrorPopup(message, stackTrace);
	}

	public void HideErrorMessagePopup()
	{
		MainUIPanel.SetActive(value: true);
		ErrorMessagePopup.gameObject.SetActive(value: false);
	}

	public void ShowInfoPopup(string header, string message)
	{
		Singleton<UIManager>.Instance.ShowPauseMenu(show: true);
		DisableSubMenus();
		MainUIPanel.SetActive(value: false);
		InfoMessagePopup.ShowInfoPopup(header, message);
	}

	public void HideInfoMessagePopup()
	{
		MainUIPanel.SetActive(value: true);
		InfoMessagePopup.gameObject.SetActive(value: false);
		Singleton<UIManager>.Instance.ShowPauseMenu(show: false);
	}

	public void OnClearAllPhysicsPressed()
	{
		Singleton<DebugManager>.Instance.ClearAllPhysicsOrePieces();
		ClearAllOrePopup.SetActive(value: false);
	}

	public void OnToggleHudPressed()
	{
		Singleton<UIManager>.Instance.ToggleHud();
		RefreshToggleHudText();
	}

	private void RefreshToggleHudText()
	{
		ToggleHudText.text = (Singleton<UIManager>.Instance.HudIsHidden ? "Enable Hud (Press P)" : "Disable Hud (Press P)");
	}

	public void OnSavePressed()
	{
		Singleton<SavingLoadingManager>.Instance.SaveGameWithActiveSaveFileName();
		RefreshLastSaveTime();
		LastSaveTimeText.text = "Last Saved: Now";
	}

	public void OnOpenSaveMenuPressed()
	{
		DisableSubMenus();
		SaveGameMenu.SetActive(value: true);
		MainUIPanel.SetActive(value: false);
	}

	public void OnOpenSettingsPressed()
	{
		DisableSubMenus();
		SettingsMenu.SetActive(value: true);
		MainUIPanel.SetActive(value: false);
	}

	public void OnCloseSubMenusPressed()
	{
		DisableSubMenus();
		MainUIPanel.SetActive(value: true);
	}

	public void OnQuitMenuPressed()
	{
		bool activeSelf = QuitMenu.activeSelf;
		DisableSubMenus();
		RefreshLastSaveTime();
		QuitMenu.SetActive(!activeSelf);
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

	public void OnMainMenuMenuPressed()
	{
		bool activeSelf = ReturnToMainMenuMenu.activeSelf;
		DisableSubMenus();
		RefreshLastSaveTime();
		ReturnToMainMenuMenu.SetActive(!activeSelf);
	}

	public void RefreshLastSaveTime()
	{
		QuitWarningLastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		MainMenuWarningLastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		LastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
	}

	public void OnReturnToMainMenuPressed()
	{
		Time.timeScale = 1f;
		PlayerInput[] array = Resources.FindObjectsOfTypeAll<PlayerInput>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DeactivateInput();
		}
		Debug.Log("Returning to main menu...");
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}

	public void OnDontQuitPressed()
	{
		DisableSubMenus();
	}

	public void OnReallyQuitPressed()
	{
		Application.Quit();
	}

	public void OnTurnOffAllAutoMinersPressed()
	{
		AutoMiner[] array = Object.FindObjectsOfType<AutoMiner>();
		foreach (AutoMiner autoMiner in array)
		{
			if (autoMiner.enabled)
			{
				autoMiner.Toggle(on: false);
			}
		}
	}

	public void OnTurnOnAllAutoMinersPressed()
	{
		AutoMiner[] array = Object.FindObjectsOfType<AutoMiner>();
		foreach (AutoMiner autoMiner in array)
		{
			if (autoMiner.enabled)
			{
				autoMiner.Toggle(on: true);
			}
		}
	}

	public void OnRespawnPlayerPressed()
	{
		Object.FindObjectOfType<PlayerController>().RespawnPlayer();
	}
}
