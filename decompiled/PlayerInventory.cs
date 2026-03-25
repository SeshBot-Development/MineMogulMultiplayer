using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
	public List<BaseHeldTool> Items = new List<BaseHeldTool>();

	public List<InventorySlotUI> InventorySlots = new List<InventorySlotUI>();

	public BaseHeldTool ActiveTool;

	public SoundDefinition PickupItemSound;

	public SoundDefinition DropItemSound;

	private PlayerController _playerController;

	private static int _maxHotbarSize = 10;

	private static int _maxInventorySize = 30;

	private GameObject _mainInventoryPanel;

	private GameObject _hotbarPanel;

	private GameObject _inventoryItemsPanel;

	private GameObject _inventorySlotPrefab;

	private PlayerInputActions _input;

	private void Start()
	{
		if (!Singleton<DebugManager>.Instance.PlayerSpawnsWithItems)
		{
			ClearInventory();
		}
		_mainInventoryPanel = Singleton<InventoryUIManager>.Instance.InventoryPanel;
		_hotbarPanel = Singleton<InventoryUIManager>.Instance.HotbarPanel;
		_inventoryItemsPanel = Singleton<InventoryUIManager>.Instance.InventorySlotsPanel;
		_inventorySlotPrefab = Singleton<InventoryUIManager>.Instance.InventorySlotPrefab;
		_playerController = GetComponent<PlayerController>();
		_input = _playerController.GetInputActions();
		foreach (BaseHeldTool item in Items)
		{
			if (!(item == null))
			{
				item.gameObject.SetActive(item == ActiveTool);
			}
		}
		_mainInventoryPanel.SetActive(value: false);
		foreach (Transform item2 in _hotbarPanel.transform)
		{
			Object.Destroy(item2.gameObject);
		}
		foreach (Transform item3 in _inventoryItemsPanel.transform)
		{
			Object.Destroy(item3.gameObject);
		}
		for (int i = 0; i < _maxHotbarSize; i++)
		{
			InventorySlotUI component = Object.Instantiate(_inventorySlotPrefab, _hotbarPanel.transform).GetComponent<InventorySlotUI>();
			component.SlotIndex = i;
			InventorySlots.Add(component);
		}
		for (int j = _maxHotbarSize; j < _maxInventorySize + _maxHotbarSize; j++)
		{
			InventorySlotUI component2 = Object.Instantiate(_inventorySlotPrefab, _inventoryItemsPanel.transform).GetComponent<InventorySlotUI>();
			component2.SlotIndex = j;
			InventorySlots.Add(component2);
		}
		UpdateUI();
	}

	public void DropItemAtIndex(int index)
	{
		if (index >= 0 && index < Items.Count && !(Items[index] == null))
		{
			BaseHeldTool component = Items[index].GetComponent<BaseHeldTool>();
			if (component != null)
			{
				component.DropItem();
			}
		}
	}

	public void ClearInventory()
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (!(Items[i] == null))
			{
				Object.Destroy(Items[i].gameObject);
				Items[i] = null;
			}
		}
	}

	private void Update()
	{
		if (!Singleton<UIManager>.Instance.IsInAnyMenuExceptInventory())
		{
			if (_input.Player.HotbarSlot1.WasPressedThisFrame())
			{
				SwitchTool(0);
			}
			if (_input.Player.HotbarSlot2.WasPressedThisFrame())
			{
				SwitchTool(1);
			}
			if (_input.Player.HotbarSlot3.WasPressedThisFrame())
			{
				SwitchTool(2);
			}
			if (_input.Player.HotbarSlot4.WasPressedThisFrame())
			{
				SwitchTool(3);
			}
			if (_input.Player.HotbarSlot5.WasPressedThisFrame())
			{
				SwitchTool(4);
			}
			if (_input.Player.HotbarSlot6.WasPressedThisFrame())
			{
				SwitchTool(5);
			}
			if (_input.Player.HotbarSlot7.WasPressedThisFrame())
			{
				SwitchTool(6);
			}
			if (_input.Player.HotbarSlot8.WasPressedThisFrame())
			{
				SwitchTool(7);
			}
			if (_input.Player.HotbarSlot9.WasPressedThisFrame())
			{
				SwitchTool(8);
			}
			if (_input.Player.HotbarSlot10.WasPressedThisFrame())
			{
				SwitchTool(9);
			}
		}
		if (!Singleton<UIManager>.Instance.IsInAnyMenu())
		{
			float num = (Singleton<SettingsManager>.Instance.UseReverseHotbarScrolling ? Input.GetAxis("Mouse ScrollWheel") : (0f - Input.GetAxis("Mouse ScrollWheel")));
			if (num != 0f && _maxHotbarSize > 0)
			{
				int num2 = Items.IndexOf(ActiveTool);
				int num3 = -(int)Mathf.Sign(num);
				int num4 = num2;
				for (int i = 0; i < Items.Count; i++)
				{
					num4 = (num4 + num3 + _maxHotbarSize) % _maxHotbarSize;
					if (Items[num4] != null)
					{
						SwitchTool(num4);
						break;
					}
				}
			}
			if (ActiveTool != null)
			{
				if (_input.Player.PrimaryAttack.WasPressedThisFrame())
				{
					BaseHeldTool component = ActiveTool.GetComponent<BaseHeldTool>();
					if (component != null)
					{
						component.PrimaryFire();
					}
				}
				if (_input.Player.PrimaryAttack.IsPressed())
				{
					BaseHeldTool component2 = ActiveTool.GetComponent<BaseHeldTool>();
					if (component2 != null)
					{
						component2.PrimaryFireHeld();
					}
				}
				if (_input.Player.SecondaryAttack.WasPressedThisFrame())
				{
					BaseHeldTool component3 = ActiveTool.GetComponent<BaseHeldTool>();
					if (component3 != null)
					{
						component3.SecondaryFire();
					}
				}
				if (_input.Player.SecondaryAttack.IsPressed())
				{
					BaseHeldTool component4 = ActiveTool.GetComponent<BaseHeldTool>();
					if (component4 != null)
					{
						component4.SecondaryFireHeld();
					}
				}
				if (_input.Player.RotateObject.WasPressedThisFrame())
				{
					BaseHeldTool component5 = ActiveTool.GetComponent<BaseHeldTool>();
					if (component5 != null)
					{
						component5.Reload();
					}
				}
				if (_input.Player.DropTool.WasPressedThisFrame())
				{
					BaseHeldTool component6 = ActiveTool.GetComponent<BaseHeldTool>();
					Singleton<UIManager>.Instance.UpdateOnScreenControls();
					if (component6 != null)
					{
						component6.DropItem();
					}
				}
				if (_input.Player.MirrorObject.WasPressedThisFrame())
				{
					BaseHeldTool component7 = ActiveTool.GetComponent<BaseHeldTool>();
					if (component7 != null)
					{
						component7.QButtonPressed();
					}
				}
			}
		}
		UpdateUI();
	}

	public void SwitchTool(BaseHeldTool tool)
	{
		if (!(tool == null) && Items.Contains(tool))
		{
			SwitchTool(Items.IndexOf(tool));
		}
	}

	public int GetActiveToolSlotIndex()
	{
		return Items.IndexOf(ActiveTool);
	}

	public void SwitchTool(int index)
	{
		if (ActiveTool != null)
		{
			ActiveTool.gameObject.SetActive(value: false);
		}
		if (index < 0 || index > Items.Count - 1)
		{
			index = 0;
		}
		if (ActiveTool == Items[index])
		{
			if (Items[index] == null)
			{
				return;
			}
			ActiveTool.gameObject.SetActive(value: false);
			ActiveTool = null;
		}
		else
		{
			ActiveTool = Items[index];
			if (Items[index] == null)
			{
				return;
			}
			BaseHeldTool component = ActiveTool.GetComponent<BaseHeldTool>();
			if (component != null)
			{
				component.Owner = _playerController;
			}
			ActiveTool.gameObject.SetActive(value: true);
		}
		Singleton<UIManager>.Instance.UpdateOnScreenControls(ActiveTool);
	}

	private void UpdateUI()
	{
		for (int i = 0; i < Items.Count && (i <= _maxHotbarSize - 1 || Singleton<InventoryUIManager>.Instance.IsShowingInventory()); i++)
		{
			if (Items[i] != null)
			{
				Sprite icon = Items[i].GetIcon();
				if (icon != null)
				{
					InventorySlots[i].Text.text = "";
					InventorySlots[i].Icon.enabled = true;
					InventorySlots[i].Icon.sprite = icon;
					InventorySlots[i].SetHighlighted(Items[i] == ActiveTool);
				}
				else
				{
					string text = Items[i].Name;
					InventorySlots[i].Text.text = text;
					InventorySlots[i].Icon.enabled = false;
					InventorySlots[i].SetHighlighted(Items[i] == ActiveTool);
				}
				if (Items[i].Quantity > 1)
				{
					InventorySlots[i].AmountText.text = Items[i].Quantity.ToString();
				}
				else
				{
					InventorySlots[i].AmountText.text = "";
				}
			}
			else
			{
				InventorySlots[i].Text.text = "";
				InventorySlots[i].AmountText.text = "";
				InventorySlots[i].Icon.enabled = false;
				InventorySlots[i].SetHighlighted(selected: false);
			}
		}
	}

	public bool TryAddToInventory(BaseHeldTool tool, int slotIndex = -1)
	{
		if (tool == null)
		{
			return false;
		}
		if (tool.gameObject.scene.rootCount == 0)
		{
			tool = Object.Instantiate(tool);
		}
		int num = -1;
		if (slotIndex != -1)
		{
			num = slotIndex;
		}
		else
		{
			if (tool.MaxAmount > 0 && tool is ToolBuilder toolBuilder)
			{
				for (int i = 0; i < Items.Count; i++)
				{
					if (Items[i] != null && Items[i] is ToolBuilder toolBuilder2 && toolBuilder2.Definition == toolBuilder.Definition && toolBuilder2.MaxAmount > toolBuilder2.Quantity)
					{
						int b = toolBuilder2.MaxAmount - toolBuilder2.Quantity;
						int num2 = Mathf.Min(toolBuilder.Quantity, b);
						toolBuilder2.Quantity += num2;
						toolBuilder.Quantity -= num2;
						if (toolBuilder.Quantity <= 0)
						{
							Singleton<SoundManager>.Instance.PlayUISound(PickupItemSound);
							UpdateUI();
							Object.Destroy(toolBuilder.gameObject);
							return true;
						}
					}
				}
			}
			num = Items.FindIndex((BaseHeldTool baseHeldTool) => baseHeldTool == null);
		}
		if (num == -1)
		{
			return false;
		}
		if (num > 8)
		{
			Singleton<QuestManager>.Instance.TryGiveInventoryQuest();
		}
		tool.gameObject.SetActive(value: false);
		Items[num] = tool;
		if (tool.EquipWhenPickedUp && num < 10)
		{
			SwitchTool(num);
		}
		Singleton<SoundManager>.Instance.PlayUISound(PickupItemSound);
		UpdateUI();
		tool.Owner = _playerController;
		return true;
	}

	public int GetInventoryIndexForTool(BaseHeldTool tool)
	{
		return Items.FindIndex((BaseHeldTool i) => i == tool);
	}

	public void RemoveFromInventory(BaseHeldTool tool, int Quantity = 1)
	{
		if (!(tool == null))
		{
			int num = Items.IndexOf(tool);
			if (num != -1)
			{
				Items[num] = null;
			}
			if (ActiveTool == tool)
			{
				ActiveTool = null;
			}
			Singleton<SoundManager>.Instance.PlayUISound(DropItemSound);
			UpdateUI();
		}
	}

	public void SwapSlots(int indexA, int indexB)
	{
		if (indexA < 0 || indexB < 0 || indexA >= Items.Count || indexB >= Items.Count)
		{
			return;
		}
		BaseHeldTool value = Items[indexA];
		Items[indexA] = Items[indexB];
		Items[indexB] = value;
		if (ActiveTool != null)
		{
			if (ActiveTool == Items[indexB])
			{
				SwitchTool(indexA);
			}
			else if (ActiveTool == Items[indexA])
			{
				SwitchTool(indexB);
			}
		}
		UpdateUI();
	}

	private IEnumerator UpdateUINextFrame()
	{
		yield return null;
		UpdateUI();
	}
}
