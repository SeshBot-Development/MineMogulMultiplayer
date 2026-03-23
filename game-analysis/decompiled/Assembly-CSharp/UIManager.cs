using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F9 RID: 249
public class UIManager : Singleton<UIManager>
{
	// Token: 0x06000695 RID: 1685 RVA: 0x0002267C File Offset: 0x0002087C
	private void OnEnable()
	{
		this.HideBuildingInfo();
		this.UpdateOnScreenControls(null);
		this._versionNumberText.text = Singleton<VersionManager>.Instance.GetVersionTextWithoutLabel();
		this.EditTextPopup.gameObject.SetActive(false);
		this._input = Singleton<KeybindManager>.Instance.Input;
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x000226CC File Offset: 0x000208CC
	public bool IsInAnyMenu()
	{
		return this.IsInAnyMenuExceptInventory() || Singleton<InventoryUIManager>.Instance.InventoryPanel.activeSelf;
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x000226EC File Offset: 0x000208EC
	public bool IsInAnyMenuExceptInventory()
	{
		return this.ComputerShopUI.isActiveAndEnabled || this.ContractsTerminalUI.isActiveAndEnabled || this.InteractionWheelUI.isActiveAndEnabled || this.PauseMenu.isActiveAndEnabled || this.QuestTreeUI.isActiveAndEnabled || this.EditTextPopup.isActiveAndEnabled;
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x00022754 File Offset: 0x00020954
	public bool IsInComputerShop()
	{
		return this.ComputerShopUI.isActiveAndEnabled;
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x00022761 File Offset: 0x00020961
	public bool IsInContractsTerminal()
	{
		return this.ContractsTerminalUI.isActiveAndEnabled;
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x0002276E File Offset: 0x0002096E
	public bool IsInPauseMenu()
	{
		return this.PauseMenu.isActiveAndEnabled;
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x0002277B File Offset: 0x0002097B
	public bool IsInInventory()
	{
		return Singleton<InventoryUIManager>.Instance.InventoryPanel.activeSelf;
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x0002278C File Offset: 0x0002098C
	public bool IsInQuestTree()
	{
		return this.QuestTreeUI.gameObject.activeSelf;
	}

	// Token: 0x0600069D RID: 1693 RVA: 0x0002279E File Offset: 0x0002099E
	public bool IsInEditTextPopup()
	{
		return this.EditTextPopup.gameObject.activeSelf;
	}

	// Token: 0x0600069E RID: 1694 RVA: 0x000227B0 File Offset: 0x000209B0
	private void LateUpdate()
	{
		if (this._input.Player.ToggleHud.WasPressedThisFrame())
		{
			this.ToggleHud();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (this.IsInAnyMenu())
			{
				Singleton<InventoryUIManager>.Instance.ShowInventory(false);
				this.ShowPauseMenu(false);
			}
			else
			{
				this.ShowPauseMenu(true);
			}
		}
		if (!this.IsInEditTextPopup())
		{
			if (this.ComputerShopUI.isActiveAndEnabled && this._input.Player.Interact.WasPressedThisFrame())
			{
				this.InteractionWheelUI.CloseWheel();
			}
			if (!this.IsInComputerShop() && !this.IsInPauseMenu() && !this.IsInContractsTerminal())
			{
				if (this._input.Player.Inventory.WasPressedThisFrame())
				{
					Singleton<InventoryUIManager>.Instance.ToggleInventory();
					if (this.IsInInventory())
					{
						this.HudObject.SetActive(true);
					}
					else
					{
						this.HudObject.SetActive(!this.HudIsHidden);
					}
				}
				if (this._input.Player.QuestMenu.WasPressedThisFrame())
				{
					this.QuestTreeUI.gameObject.SetActive(!this.QuestTreeUI.gameObject.activeSelf);
					Singleton<InventoryUIManager>.Instance.ShowInventory(false);
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape) || this._input.Player.Inventory.WasPressedThisFrame())
		{
			this.ComputerShopUI.gameObject.SetActive(false);
			this.ContractsTerminalUI.gameObject.SetActive(false);
			this.InteractionWheelUI.CloseWheel();
			this.QuestTreeUI.gameObject.SetActive(false);
			this.EditTextPopup.gameObject.SetActive(false);
		}
		if (this.IsInAnyMenu())
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = (Singleton<SettingsManager>.Instance.ForceUnlockedCursor ? CursorLockMode.None : CursorLockMode.Locked);
			Cursor.visible = false;
		}
		if (this.IsInComputerShop() || this.IsInContractsTerminal() || this.IsInPauseMenu() || this.IsInInventory() || this.IsInQuestTree())
		{
			this.BackgroundBlur.SetActive(true);
			return;
		}
		this.BackgroundBlur.SetActive(false);
	}

	// Token: 0x0600069F RID: 1695 RVA: 0x000229E1 File Offset: 0x00020BE1
	public void ShowInfoMessagePopup(string header, string message)
	{
		this.PauseMenu.ShowInfoPopup(header, message);
	}

	// Token: 0x060006A0 RID: 1696 RVA: 0x000229F0 File Offset: 0x00020BF0
	public void ShowPauseMenu(bool show)
	{
		if (show)
		{
			this.HudObject.SetActive(false);
			this.PauseMenu.gameObject.SetActive(true);
			return;
		}
		this.HudObject.SetActive(!this.HudIsHidden);
		this.PauseMenu.gameObject.SetActive(false);
	}

	// Token: 0x060006A1 RID: 1697 RVA: 0x00022A43 File Offset: 0x00020C43
	public void StartEditingText(EditableSign editableSign)
	{
		this.EditTextPopup.StartEditingText(editableSign);
	}

	// Token: 0x060006A2 RID: 1698 RVA: 0x00022A51 File Offset: 0x00020C51
	public void ShowContractsTerminal()
	{
		this.ContractsTerminalUI.gameObject.SetActive(true);
	}

	// Token: 0x060006A3 RID: 1699 RVA: 0x00022A64 File Offset: 0x00020C64
	public void ShowAutoSavingWarning()
	{
		this.AutoSavingWarning.gameObject.SetActive(true);
	}

	// Token: 0x060006A4 RID: 1700 RVA: 0x00022A77 File Offset: 0x00020C77
	public void HideAutoSavingWarning()
	{
		this.AutoSavingWarning.OnSavingFinished();
	}

	// Token: 0x060006A5 RID: 1701 RVA: 0x00022A84 File Offset: 0x00020C84
	public void HideBuildingInfo()
	{
		if (this.BuildingInfo != null)
		{
			this.BuildingInfo.SetActive(false);
		}
	}

	// Token: 0x060006A6 RID: 1702 RVA: 0x00022AA0 File Offset: 0x00020CA0
	public void ShowBuildingInfo(string description)
	{
		this.BuildingInfo.SetActive(true);
		this._autominerResourceText.text = description;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.BuildingInfo.transform);
	}

	// Token: 0x060006A7 RID: 1703 RVA: 0x00022AD0 File Offset: 0x00020CD0
	public void UpdateOnScreenControls(BaseHeldTool tool = null)
	{
		string text = "";
		text = string.Concat(new string[]
		{
			text,
			"Grab Physics - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.Grab),
			"\nInteract - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.Interact),
			"\nOpen Inventory - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.Inventory),
			"\n"
		});
		if (tool != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text = text + "<size=120%>" + tool.GetObjectName() + "</size>\n";
			text += tool.GetControlsText();
		}
		this._controlsText.text = text;
	}

	// Token: 0x060006A8 RID: 1704 RVA: 0x00022B8D File Offset: 0x00020D8D
	public void ToggleHud()
	{
		this.HudIsHidden = !this.HudIsHidden;
		this.HudObject.SetActive(!this.HudIsHidden);
	}

	// Token: 0x040007D0 RID: 2000
	public Color MoneyTextColor;

	// Token: 0x040007D1 RID: 2001
	public Color ResearchTicketsTextColor;

	// Token: 0x040007D2 RID: 2002
	public Material GrayscaleImageMaterial;

	// Token: 0x040007D3 RID: 2003
	public ComputerShopUI ComputerShopUI;

	// Token: 0x040007D4 RID: 2004
	public ContractsTerminalUI ContractsTerminalUI;

	// Token: 0x040007D5 RID: 2005
	public InteractionWheelUI InteractionWheelUI;

	// Token: 0x040007D6 RID: 2006
	public QuestTreeUI QuestTreeUI;

	// Token: 0x040007D7 RID: 2007
	public PauseMenu PauseMenu;

	// Token: 0x040007D8 RID: 2008
	public GameObject BuildingInfo;

	// Token: 0x040007D9 RID: 2009
	public GameObject HudObject;

	// Token: 0x040007DA RID: 2010
	public AutoSavingWarning AutoSavingWarning;

	// Token: 0x040007DB RID: 2011
	public GameObject BackgroundBlur;

	// Token: 0x040007DC RID: 2012
	public EditTextPopup EditTextPopup;

	// Token: 0x040007DD RID: 2013
	public bool HudIsHidden;

	// Token: 0x040007DE RID: 2014
	[SerializeField]
	private TMP_Text _autominerResourceText;

	// Token: 0x040007DF RID: 2015
	[SerializeField]
	private TMP_Text _controlsText;

	// Token: 0x040007E0 RID: 2016
	[SerializeField]
	private TMP_Text _versionNumberText;

	// Token: 0x040007E1 RID: 2017
	private PlayerInputActions _input;
}
