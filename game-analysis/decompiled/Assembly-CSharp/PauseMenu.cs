using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Token: 0x02000080 RID: 128
public class PauseMenu : MonoBehaviour
{
	// Token: 0x06000361 RID: 865 RVA: 0x00010FB8 File Offset: 0x0000F1B8
	private void OnEnable()
	{
		this.QuitMenu.SetActive(false);
		this.ReturnToMainMenuMenu.SetActive(false);
		this.SettingsMenu.SetActive(false);
		this.MainUIPanel.SetActive(true);
		this.SaveGameMenu.SetActive(false);
		this.ClearAllOrePopup.SetActive(false);
		this.ErrorMessagePopup.gameObject.SetActive(false);
		this.InfoMessagePopup.gameObject.SetActive(false);
		this.VersionNumberText.text = Singleton<VersionManager>.Instance.GetFormattedVersionText();
		this.RefreshToggleHudText();
		this._originalTimeScale = Time.timeScale;
		if (this._originalTimeScale <= 0f)
		{
			this._originalTimeScale = 1f;
		}
		Time.timeScale = 0f;
		Singleton<SettingsManager>.Instance.SetPauseMenuFPSLimit();
		Singleton<GameManager>.Instance.OnGamePauseToggled(true);
		if (Singleton<SavingLoadingManager>.Instance.LastSaveTime != 0f)
		{
			this.LastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		}
	}

	// Token: 0x06000362 RID: 866 RVA: 0x000110BC File Offset: 0x0000F2BC
	private void Update()
	{
		if (Singleton<DebugManager>.Instance.DevModeEnabled)
		{
			this.TotalOrePiecesText.text = string.Format(" (Active Ore Physics Objects: {0}, Pooled: {1} )", OrePiece.AllOrePieces.Count.ToString(), Singleton<OrePiecePoolManager>.Instance.GetInactiveCount());
			return;
		}
		this.TotalOrePiecesText.text = " (Active Ore Physics Objects: " + OrePiece.AllOrePieces.Count.ToString() + " )";
	}

	// Token: 0x06000363 RID: 867 RVA: 0x00011138 File Offset: 0x0000F338
	private void OnDisable()
	{
		Time.timeScale = this._originalTimeScale;
		Singleton<SettingsManager>.Instance.SetVsyncAndFPSLimit();
		Singleton<GameManager>.Instance.OnGamePauseToggled(false);
	}

	// Token: 0x06000364 RID: 868 RVA: 0x0001115A File Offset: 0x0000F35A
	public void OnResumePressed()
	{
		Singleton<UIManager>.Instance.HudObject.SetActive(!Singleton<UIManager>.Instance.HudIsHidden);
		base.gameObject.SetActive(false);
	}

	// Token: 0x06000365 RID: 869 RVA: 0x00011184 File Offset: 0x0000F384
	public void DisableSubMenus()
	{
		this.QuitMenu.SetActive(false);
		this.ReturnToMainMenuMenu.SetActive(false);
		this.SettingsMenu.SetActive(false);
		this.SaveGameMenu.SetActive(false);
		this.ClearAllOrePopup.SetActive(false);
	}

	// Token: 0x06000366 RID: 870 RVA: 0x000111C4 File Offset: 0x0000F3C4
	public void OnClearOreMenuPressed()
	{
		bool activeSelf = this.ClearAllOrePopup.activeSelf;
		this.DisableSubMenus();
		this.ClearAllOrePopup.SetActive(!activeSelf);
	}

	// Token: 0x06000367 RID: 871 RVA: 0x000111F2 File Offset: 0x0000F3F2
	public void ShowErrorPopup(string message, string stackTrace)
	{
		Singleton<UIManager>.Instance.ShowPauseMenu(true);
		this.DisableSubMenus();
		this.MainUIPanel.SetActive(false);
		this.ErrorMessagePopup.ShowErrorPopup(message, stackTrace);
	}

	// Token: 0x06000368 RID: 872 RVA: 0x0001121E File Offset: 0x0000F41E
	public void HideErrorMessagePopup()
	{
		this.MainUIPanel.SetActive(true);
		this.ErrorMessagePopup.gameObject.SetActive(false);
	}

	// Token: 0x06000369 RID: 873 RVA: 0x0001123D File Offset: 0x0000F43D
	public void ShowInfoPopup(string header, string message)
	{
		Singleton<UIManager>.Instance.ShowPauseMenu(true);
		this.DisableSubMenus();
		this.MainUIPanel.SetActive(false);
		this.InfoMessagePopup.ShowInfoPopup(header, message);
	}

	// Token: 0x0600036A RID: 874 RVA: 0x00011269 File Offset: 0x0000F469
	public void HideInfoMessagePopup()
	{
		this.MainUIPanel.SetActive(true);
		this.InfoMessagePopup.gameObject.SetActive(false);
		Singleton<UIManager>.Instance.ShowPauseMenu(false);
	}

	// Token: 0x0600036B RID: 875 RVA: 0x00011293 File Offset: 0x0000F493
	public void OnClearAllPhysicsPressed()
	{
		Singleton<DebugManager>.Instance.ClearAllPhysicsOrePieces(true);
		this.ClearAllOrePopup.SetActive(false);
	}

	// Token: 0x0600036C RID: 876 RVA: 0x000112AC File Offset: 0x0000F4AC
	public void OnToggleHudPressed()
	{
		Singleton<UIManager>.Instance.ToggleHud();
		this.RefreshToggleHudText();
	}

	// Token: 0x0600036D RID: 877 RVA: 0x000112BE File Offset: 0x0000F4BE
	private void RefreshToggleHudText()
	{
		this.ToggleHudText.text = (Singleton<UIManager>.Instance.HudIsHidden ? "Enable Hud (Press P)" : "Disable Hud (Press P)");
	}

	// Token: 0x0600036E RID: 878 RVA: 0x000112E3 File Offset: 0x0000F4E3
	public void OnSavePressed()
	{
		Singleton<SavingLoadingManager>.Instance.SaveGameWithActiveSaveFileName();
		this.RefreshLastSaveTime();
		this.LastSaveTimeText.text = "Last Saved: Now";
	}

	// Token: 0x0600036F RID: 879 RVA: 0x00011305 File Offset: 0x0000F505
	public void OnOpenSaveMenuPressed()
	{
		this.DisableSubMenus();
		this.SaveGameMenu.SetActive(true);
		this.MainUIPanel.SetActive(false);
	}

	// Token: 0x06000370 RID: 880 RVA: 0x00011325 File Offset: 0x0000F525
	public void OnOpenSettingsPressed()
	{
		this.DisableSubMenus();
		this.SettingsMenu.SetActive(true);
		this.MainUIPanel.SetActive(false);
	}

	// Token: 0x06000371 RID: 881 RVA: 0x00011345 File Offset: 0x0000F545
	public void OnCloseSubMenusPressed()
	{
		this.DisableSubMenus();
		this.MainUIPanel.SetActive(true);
	}

	// Token: 0x06000372 RID: 882 RVA: 0x0001135C File Offset: 0x0000F55C
	public void OnQuitMenuPressed()
	{
		bool activeSelf = this.QuitMenu.activeSelf;
		this.DisableSubMenus();
		this.RefreshLastSaveTime();
		this.QuitMenu.SetActive(!activeSelf);
	}

	// Token: 0x06000373 RID: 883 RVA: 0x00011390 File Offset: 0x0000F590
	public void OnWishlistPressed()
	{
		Application.OpenURL("https://store.steampowered.com/app/3846120/MineMogul/");
	}

	// Token: 0x06000374 RID: 884 RVA: 0x0001139C File Offset: 0x0000F59C
	public void OnSteamDiscussionsPressed()
	{
		Application.OpenURL("https://steamcommunity.com/app/3846120/discussions/");
	}

	// Token: 0x06000375 RID: 885 RVA: 0x000113A8 File Offset: 0x0000F5A8
	public void OnDiscordPressed()
	{
		Application.OpenURL("https://discord.gg/F3cdWuTAEJ");
	}

	// Token: 0x06000376 RID: 886 RVA: 0x000113B4 File Offset: 0x0000F5B4
	public void OnMainMenuMenuPressed()
	{
		bool activeSelf = this.ReturnToMainMenuMenu.activeSelf;
		this.DisableSubMenus();
		this.RefreshLastSaveTime();
		this.ReturnToMainMenuMenu.SetActive(!activeSelf);
	}

	// Token: 0x06000377 RID: 887 RVA: 0x000113E8 File Offset: 0x0000F5E8
	public void RefreshLastSaveTime()
	{
		this.QuitWarningLastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		this.MainMenuWarningLastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
		this.LastSaveTimeText.text = "Last Saved: " + Singleton<SavingLoadingManager>.Instance.GetFormattedLastSaveTime();
	}

	// Token: 0x06000378 RID: 888 RVA: 0x00011454 File Offset: 0x0000F654
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

	// Token: 0x06000379 RID: 889 RVA: 0x0001149C File Offset: 0x0000F69C
	public void OnDontQuitPressed()
	{
		this.DisableSubMenus();
	}

	// Token: 0x0600037A RID: 890 RVA: 0x000114A4 File Offset: 0x0000F6A4
	public void OnReallyQuitPressed()
	{
		Application.Quit();
	}

	// Token: 0x0600037B RID: 891 RVA: 0x000114AC File Offset: 0x0000F6AC
	public void OnTurnOffAllAutoMinersPressed()
	{
		foreach (AutoMiner autoMiner in Object.FindObjectsOfType<AutoMiner>())
		{
			if (autoMiner.enabled)
			{
				autoMiner.Toggle(false);
			}
		}
	}

	// Token: 0x0600037C RID: 892 RVA: 0x000114E0 File Offset: 0x0000F6E0
	public void OnTurnOnAllAutoMinersPressed()
	{
		foreach (AutoMiner autoMiner in Object.FindObjectsOfType<AutoMiner>())
		{
			if (autoMiner.enabled)
			{
				autoMiner.Toggle(true);
			}
		}
	}

	// Token: 0x0600037D RID: 893 RVA: 0x00011514 File Offset: 0x0000F714
	public void OnRespawnPlayerPressed()
	{
		Object.FindObjectOfType<PlayerController>().RespawnPlayer();
	}

	// Token: 0x0400035F RID: 863
	public GameObject QuitMenu;

	// Token: 0x04000360 RID: 864
	public GameObject ReturnToMainMenuMenu;

	// Token: 0x04000361 RID: 865
	public GameObject MainUIPanel;

	// Token: 0x04000362 RID: 866
	public GameObject SettingsMenu;

	// Token: 0x04000363 RID: 867
	public GameObject ClearAllOrePopup;

	// Token: 0x04000364 RID: 868
	public TMP_Text TotalOrePiecesText;

	// Token: 0x04000365 RID: 869
	public TMP_Text VersionNumberText;

	// Token: 0x04000366 RID: 870
	public GameObject SaveGameMenu;

	// Token: 0x04000367 RID: 871
	public TMP_Text QuitWarningLastSaveTimeText;

	// Token: 0x04000368 RID: 872
	public TMP_Text MainMenuWarningLastSaveTimeText;

	// Token: 0x04000369 RID: 873
	public TMP_Text ToggleHudText;

	// Token: 0x0400036A RID: 874
	public TMP_Text LastSaveTimeText;

	// Token: 0x0400036B RID: 875
	public ErrorMessagePopup ErrorMessagePopup;

	// Token: 0x0400036C RID: 876
	public InfoMessagePopup InfoMessagePopup;

	// Token: 0x0400036D RID: 877
	private float _originalTimeScale = 1f;
}
