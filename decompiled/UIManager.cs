using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	public Color MoneyTextColor;

	public Color ResearchTicketsTextColor;

	public Material GrayscaleImageMaterial;

	public ComputerShopUI ComputerShopUI;

	public ContractsTerminalUI ContractsTerminalUI;

	public InteractionWheelUI InteractionWheelUI;

	public QuestTreeUI QuestTreeUI;

	public PauseMenu PauseMenu;

	public GameObject BuildingInfo;

	public GameObject HudObject;

	public AutoSavingWarning AutoSavingWarning;

	public GameObject BackgroundBlur;

	public EditTextPopup EditTextPopup;

	public OreSpawnerSelectOreUI OreSpawnerSelectOreUI;

	public bool HudIsHidden;

	[SerializeField]
	private TMP_Text _autominerResourceText;

	[SerializeField]
	private TMP_Text _controlsText;

	[SerializeField]
	private TMP_Text _versionNumberText;

	private PlayerInputActions _input;

	private void OnEnable()
	{
		HideBuildingInfo();
		UpdateOnScreenControls();
		_versionNumberText.text = Singleton<VersionManager>.Instance.GetVersionTextWithoutLabel();
		EditTextPopup.gameObject.SetActive(value: false);
		OreSpawnerSelectOreUI.gameObject.SetActive(value: false);
		_input = Singleton<KeybindManager>.Instance.Input;
	}

	public bool IsInAnyMenu()
	{
		if (IsInAnyMenuExceptInventory())
		{
			return true;
		}
		if (Singleton<InventoryUIManager>.Instance.InventoryPanel.activeSelf)
		{
			return true;
		}
		return false;
	}

	public bool IsInAnyMenuExceptInventory()
	{
		if (ComputerShopUI.isActiveAndEnabled)
		{
			return true;
		}
		if (ContractsTerminalUI.isActiveAndEnabled)
		{
			return true;
		}
		if (InteractionWheelUI.isActiveAndEnabled)
		{
			return true;
		}
		if (PauseMenu.isActiveAndEnabled)
		{
			return true;
		}
		if (QuestTreeUI.isActiveAndEnabled)
		{
			return true;
		}
		if (EditTextPopup.isActiveAndEnabled)
		{
			return true;
		}
		if (OreSpawnerSelectOreUI.isActiveAndEnabled)
		{
			return true;
		}
		return false;
	}

	public bool IsInComputerShop()
	{
		return ComputerShopUI.isActiveAndEnabled;
	}

	public bool IsInContractsTerminal()
	{
		return ContractsTerminalUI.isActiveAndEnabled;
	}

	public bool IsInPauseMenu()
	{
		return PauseMenu.isActiveAndEnabled;
	}

	public bool IsInInventory()
	{
		return Singleton<InventoryUIManager>.Instance.InventoryPanel.activeSelf;
	}

	public bool IsInQuestTree()
	{
		return QuestTreeUI.gameObject.activeSelf;
	}

	public bool IsInEditTextPopup()
	{
		return EditTextPopup.gameObject.activeSelf;
	}

	public bool IsInOreSpawnerSelectOreUI()
	{
		return OreSpawnerSelectOreUI.gameObject.activeSelf;
	}

	private void LateUpdate()
	{
		if (_input.Player.ToggleHud.WasPressedThisFrame())
		{
			ToggleHud();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (IsInAnyMenu())
			{
				Singleton<InventoryUIManager>.Instance.ShowInventory(show: false);
				ShowPauseMenu(show: false);
			}
			else
			{
				ShowPauseMenu(show: true);
			}
		}
		if (!IsInEditTextPopup())
		{
			if (ComputerShopUI.isActiveAndEnabled && _input.Player.Interact.WasPressedThisFrame())
			{
				InteractionWheelUI.CloseWheel();
			}
			if (!IsInComputerShop() && !IsInPauseMenu() && !IsInContractsTerminal())
			{
				if (_input.Player.Inventory.WasPressedThisFrame())
				{
					Singleton<InventoryUIManager>.Instance.ToggleInventory();
					if (IsInInventory())
					{
						HudObject.SetActive(value: true);
					}
					else
					{
						HudObject.SetActive(!HudIsHidden);
					}
				}
				if (_input.Player.QuestMenu.WasPressedThisFrame())
				{
					QuestTreeUI.gameObject.SetActive(!QuestTreeUI.gameObject.activeSelf);
					Singleton<InventoryUIManager>.Instance.ShowInventory(show: false);
				}
			}
		}
		if (IsInOreSpawnerSelectOreUI() && _input.Player.MirrorObject.WasPressedThisFrame() && Cursor.lockState == CursorLockMode.None)
		{
			OreSpawnerSelectOreUI.gameObject.SetActive(value: false);
		}
		if (Input.GetKeyDown(KeyCode.Escape) || _input.Player.Inventory.WasPressedThisFrame())
		{
			ComputerShopUI.gameObject.SetActive(value: false);
			ContractsTerminalUI.gameObject.SetActive(value: false);
			InteractionWheelUI.CloseWheel();
			QuestTreeUI.gameObject.SetActive(value: false);
			EditTextPopup.gameObject.SetActive(value: false);
			OreSpawnerSelectOreUI.gameObject.SetActive(value: false);
		}
		if (IsInAnyMenu())
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = ((!Singleton<SettingsManager>.Instance.ForceUnlockedCursor) ? CursorLockMode.Locked : CursorLockMode.None);
			Cursor.visible = false;
		}
		if (IsInComputerShop() || IsInContractsTerminal() || IsInPauseMenu() || IsInInventory() || IsInQuestTree())
		{
			BackgroundBlur.SetActive(value: true);
		}
		else
		{
			BackgroundBlur.SetActive(value: false);
		}
	}

	public void ShowInfoMessagePopup(string header, string message)
	{
		PauseMenu.ShowInfoPopup(header, message);
	}

	public void ShowPauseMenu(bool show)
	{
		if (show)
		{
			HudObject.SetActive(value: false);
			PauseMenu.gameObject.SetActive(value: true);
		}
		else
		{
			HudObject.SetActive(!HudIsHidden);
			PauseMenu.gameObject.SetActive(value: false);
		}
	}

	public void ShowOreSpawnerSelectOreUI(ToolDebugSpawnTool spawnerTool)
	{
		OreSpawnerSelectOreUI.StartSelectingOre(spawnerTool);
	}

	public void ShowOreSpawnerSelectOreUI(OreSpawnerMacine oreSpawnerMachine)
	{
		OreSpawnerSelectOreUI.StartSelectingOreAndSpawnRate(oreSpawnerMachine);
	}

	public void StartEditingText(EditableSign editableSign)
	{
		EditTextPopup.StartEditingText(editableSign);
	}

	public void ShowContractsTerminal()
	{
		ContractsTerminalUI.gameObject.SetActive(value: true);
	}

	public void ShowAutoSavingWarning()
	{
		AutoSavingWarning.gameObject.SetActive(value: true);
	}

	public void HideAutoSavingWarning()
	{
		AutoSavingWarning.OnSavingFinished();
	}

	public void HideBuildingInfo()
	{
		if (BuildingInfo != null)
		{
			BuildingInfo.SetActive(value: false);
		}
	}

	public void ShowBuildingInfo(string description)
	{
		BuildingInfo.SetActive(value: true);
		_autominerResourceText.text = description;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)BuildingInfo.transform);
	}

	public void UpdateOnScreenControls(BaseHeldTool tool = null)
	{
		string text = "";
		text = text + "Grab Physics - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.Grab) + "\nInteract - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.Interact) + "\nOpen Inventory - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.Inventory) + "\n";
		if (tool != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text = text + "<size=120%>" + tool.GetObjectName() + "</size>\n";
			text += tool.GetControlsText();
		}
		_controlsText.text = text;
	}

	public void ToggleHud()
	{
		HudIsHidden = !HudIsHidden;
		HudObject.SetActive(!HudIsHidden);
	}
}
