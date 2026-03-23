using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000087 RID: 135
public class PlayerInventory : MonoBehaviour
{
	// Token: 0x060003B9 RID: 953 RVA: 0x00013C6C File Offset: 0x00011E6C
	private void Start()
	{
		if (!Singleton<DebugManager>.Instance.PlayerSpawnsWithItems)
		{
			this.ClearInventory();
		}
		this._mainInventoryPanel = Singleton<InventoryUIManager>.Instance.InventoryPanel;
		this._hotbarPanel = Singleton<InventoryUIManager>.Instance.HotbarPanel;
		this._inventoryItemsPanel = Singleton<InventoryUIManager>.Instance.InventorySlotsPanel;
		this._inventorySlotPrefab = Singleton<InventoryUIManager>.Instance.InventorySlotPrefab;
		this._playerController = base.GetComponent<PlayerController>();
		this._input = this._playerController.GetInputActions();
		foreach (BaseHeldTool baseHeldTool in this.Items)
		{
			if (!(baseHeldTool == null))
			{
				baseHeldTool.gameObject.SetActive(baseHeldTool == this.ActiveTool);
			}
		}
		this._mainInventoryPanel.SetActive(false);
		foreach (object obj in this._hotbarPanel.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (object obj2 in this._inventoryItemsPanel.transform)
		{
			Object.Destroy(((Transform)obj2).gameObject);
		}
		for (int i = 0; i < PlayerInventory._maxHotbarSize; i++)
		{
			InventorySlotUI component = Object.Instantiate<GameObject>(this._inventorySlotPrefab, this._hotbarPanel.transform).GetComponent<InventorySlotUI>();
			component.SlotIndex = i;
			this.InventorySlots.Add(component);
		}
		for (int j = PlayerInventory._maxHotbarSize; j < PlayerInventory._maxInventorySize + PlayerInventory._maxHotbarSize; j++)
		{
			InventorySlotUI component2 = Object.Instantiate<GameObject>(this._inventorySlotPrefab, this._inventoryItemsPanel.transform).GetComponent<InventorySlotUI>();
			component2.SlotIndex = j;
			this.InventorySlots.Add(component2);
		}
		this.UpdateUI();
	}

	// Token: 0x060003BA RID: 954 RVA: 0x00013E90 File Offset: 0x00012090
	public void DropItemAtIndex(int index)
	{
		if (index < 0 || index >= this.Items.Count)
		{
			return;
		}
		if (this.Items[index] == null)
		{
			return;
		}
		BaseHeldTool component = this.Items[index].GetComponent<BaseHeldTool>();
		if (component != null)
		{
			component.DropItem();
		}
	}

	// Token: 0x060003BB RID: 955 RVA: 0x00013EE8 File Offset: 0x000120E8
	public void ClearInventory()
	{
		for (int i = 0; i < this.Items.Count; i++)
		{
			if (!(this.Items[i] == null))
			{
				Object.Destroy(this.Items[i].gameObject);
				this.Items[i] = null;
			}
		}
	}

	// Token: 0x060003BC RID: 956 RVA: 0x00013F44 File Offset: 0x00012144
	private void Update()
	{
		if (!Singleton<UIManager>.Instance.IsInAnyMenuExceptInventory())
		{
			if (this._input.Player.HotbarSlot1.WasPressedThisFrame())
			{
				this.SwitchTool(0);
			}
			if (this._input.Player.HotbarSlot2.WasPressedThisFrame())
			{
				this.SwitchTool(1);
			}
			if (this._input.Player.HotbarSlot3.WasPressedThisFrame())
			{
				this.SwitchTool(2);
			}
			if (this._input.Player.HotbarSlot4.WasPressedThisFrame())
			{
				this.SwitchTool(3);
			}
			if (this._input.Player.HotbarSlot5.WasPressedThisFrame())
			{
				this.SwitchTool(4);
			}
			if (this._input.Player.HotbarSlot6.WasPressedThisFrame())
			{
				this.SwitchTool(5);
			}
			if (this._input.Player.HotbarSlot7.WasPressedThisFrame())
			{
				this.SwitchTool(6);
			}
			if (this._input.Player.HotbarSlot8.WasPressedThisFrame())
			{
				this.SwitchTool(7);
			}
			if (this._input.Player.HotbarSlot9.WasPressedThisFrame())
			{
				this.SwitchTool(8);
			}
			if (this._input.Player.HotbarSlot10.WasPressedThisFrame())
			{
				this.SwitchTool(9);
			}
		}
		if (!Singleton<UIManager>.Instance.IsInAnyMenu())
		{
			float num = (Singleton<SettingsManager>.Instance.UseReverseHotbarScrolling ? Input.GetAxis("Mouse ScrollWheel") : (-Input.GetAxis("Mouse ScrollWheel")));
			if (num != 0f && PlayerInventory._maxHotbarSize > 0)
			{
				int num2 = this.Items.IndexOf(this.ActiveTool);
				int num3 = -(int)Mathf.Sign(num);
				int num4 = num2;
				for (int i = 0; i < this.Items.Count; i++)
				{
					num4 = (num4 + num3 + PlayerInventory._maxHotbarSize) % PlayerInventory._maxHotbarSize;
					if (this.Items[num4] != null)
					{
						this.SwitchTool(num4);
						break;
					}
				}
			}
			if (this.ActiveTool != null)
			{
				if (this._input.Player.PrimaryAttack.WasPressedThisFrame())
				{
					BaseHeldTool component = this.ActiveTool.GetComponent<BaseHeldTool>();
					if (component != null)
					{
						component.PrimaryFire();
					}
				}
				if (this._input.Player.PrimaryAttack.IsPressed())
				{
					BaseHeldTool component2 = this.ActiveTool.GetComponent<BaseHeldTool>();
					if (component2 != null)
					{
						component2.PrimaryFireHeld();
					}
				}
				if (this._input.Player.SecondaryAttack.WasPressedThisFrame())
				{
					BaseHeldTool component3 = this.ActiveTool.GetComponent<BaseHeldTool>();
					if (component3 != null)
					{
						component3.SecondaryFire();
					}
				}
				if (this._input.Player.SecondaryAttack.IsPressed())
				{
					BaseHeldTool component4 = this.ActiveTool.GetComponent<BaseHeldTool>();
					if (component4 != null)
					{
						component4.SecondaryFireHeld();
					}
				}
				if (this._input.Player.RotateObject.WasPressedThisFrame())
				{
					BaseHeldTool component5 = this.ActiveTool.GetComponent<BaseHeldTool>();
					if (component5 != null)
					{
						component5.Reload();
					}
				}
				if (this._input.Player.DropTool.WasPressedThisFrame())
				{
					BaseHeldTool component6 = this.ActiveTool.GetComponent<BaseHeldTool>();
					Singleton<UIManager>.Instance.UpdateOnScreenControls(null);
					if (component6 != null)
					{
						component6.DropItem();
					}
				}
				if (this._input.Player.MirrorObject.WasPressedThisFrame())
				{
					BaseHeldTool component7 = this.ActiveTool.GetComponent<BaseHeldTool>();
					if (component7 != null)
					{
						component7.QButtonPressed();
					}
				}
			}
		}
		this.UpdateUI();
	}

	// Token: 0x060003BD RID: 957 RVA: 0x000142FA File Offset: 0x000124FA
	public void SwitchTool(BaseHeldTool tool)
	{
		if (tool == null)
		{
			return;
		}
		if (!this.Items.Contains(tool))
		{
			return;
		}
		this.SwitchTool(this.Items.IndexOf(tool));
	}

	// Token: 0x060003BE RID: 958 RVA: 0x00014328 File Offset: 0x00012528
	private void SwitchTool(int index)
	{
		if (this.ActiveTool != null)
		{
			this.ActiveTool.gameObject.SetActive(false);
		}
		if (this.ActiveTool == this.Items[index])
		{
			if (this.Items[index] == null)
			{
				return;
			}
			this.ActiveTool.gameObject.SetActive(false);
			this.ActiveTool = null;
		}
		else
		{
			this.ActiveTool = this.Items[index];
			if (this.Items[index] == null)
			{
				return;
			}
			BaseHeldTool component = this.ActiveTool.GetComponent<BaseHeldTool>();
			if (component != null)
			{
				component.Owner = this._playerController;
			}
			this.ActiveTool.gameObject.SetActive(true);
		}
		Singleton<UIManager>.Instance.UpdateOnScreenControls(this.ActiveTool);
	}

	// Token: 0x060003BF RID: 959 RVA: 0x00014408 File Offset: 0x00012608
	private void UpdateUI()
	{
		int num = 0;
		while (num < this.Items.Count && (num <= PlayerInventory._maxHotbarSize - 1 || Singleton<InventoryUIManager>.Instance.IsShowingInventory()))
		{
			if (this.Items[num] != null)
			{
				Sprite icon = this.Items[num].GetIcon();
				if (icon != null)
				{
					this.InventorySlots[num].Text.text = "";
					this.InventorySlots[num].Icon.enabled = true;
					this.InventorySlots[num].Icon.sprite = icon;
					this.InventorySlots[num].SetHighlighted(this.Items[num] == this.ActiveTool);
				}
				else
				{
					string name = this.Items[num].Name;
					this.InventorySlots[num].Text.text = name;
					this.InventorySlots[num].Icon.enabled = false;
					this.InventorySlots[num].SetHighlighted(this.Items[num] == this.ActiveTool);
				}
				if (this.Items[num].Quantity > 1)
				{
					this.InventorySlots[num].AmountText.text = this.Items[num].Quantity.ToString();
				}
				else
				{
					this.InventorySlots[num].AmountText.text = "";
				}
			}
			else
			{
				this.InventorySlots[num].Text.text = "";
				this.InventorySlots[num].AmountText.text = "";
				this.InventorySlots[num].Icon.enabled = false;
				this.InventorySlots[num].SetHighlighted(false);
			}
			num++;
		}
	}

	// Token: 0x060003C0 RID: 960 RVA: 0x00014618 File Offset: 0x00012818
	public bool TryAddToInventory(BaseHeldTool tool, int slotIndex = -1)
	{
		if (tool == null)
		{
			return false;
		}
		if (tool.gameObject.scene.rootCount == 0)
		{
			tool = Object.Instantiate<BaseHeldTool>(tool);
		}
		int num;
		if (slotIndex != -1)
		{
			num = slotIndex;
		}
		else
		{
			if (tool.MaxAmount > 0)
			{
				ToolBuilder toolBuilder = tool as ToolBuilder;
				if (toolBuilder != null)
				{
					for (int j = 0; j < this.Items.Count; j++)
					{
						if (this.Items[j] != null)
						{
							ToolBuilder toolBuilder2 = this.Items[j] as ToolBuilder;
							if (toolBuilder2 != null && toolBuilder2.Definition == toolBuilder.Definition && toolBuilder2.MaxAmount > toolBuilder2.Quantity)
							{
								int num2 = toolBuilder2.MaxAmount - toolBuilder2.Quantity;
								int num3 = Mathf.Min(toolBuilder.Quantity, num2);
								toolBuilder2.Quantity += num3;
								toolBuilder.Quantity -= num3;
								if (toolBuilder.Quantity <= 0)
								{
									Singleton<SoundManager>.Instance.PlayUISound(this.PickupItemSound, 1f);
									this.UpdateUI();
									Object.Destroy(toolBuilder.gameObject);
									return true;
								}
							}
						}
					}
				}
			}
			num = this.Items.FindIndex((BaseHeldTool i) => i == null);
		}
		if (num == -1)
		{
			return false;
		}
		if (num > 8)
		{
			Singleton<QuestManager>.Instance.TryGiveInventoryQuest();
		}
		tool.gameObject.SetActive(false);
		this.Items[num] = tool;
		if (tool.EquipWhenPickedUp && num < 10)
		{
			this.SwitchTool(num);
		}
		Singleton<SoundManager>.Instance.PlayUISound(this.PickupItemSound, 1f);
		this.UpdateUI();
		tool.Owner = this._playerController;
		return true;
	}

	// Token: 0x060003C1 RID: 961 RVA: 0x000147EC File Offset: 0x000129EC
	public int GetInventoryIndexForTool(BaseHeldTool tool)
	{
		return this.Items.FindIndex((BaseHeldTool i) => i == tool);
	}

	// Token: 0x060003C2 RID: 962 RVA: 0x00014820 File Offset: 0x00012A20
	public void RemoveFromInventory(BaseHeldTool tool, int Quantity = 1)
	{
		if (tool == null)
		{
			return;
		}
		int num = this.Items.IndexOf(tool);
		if (num != -1)
		{
			this.Items[num] = null;
		}
		if (this.ActiveTool == tool)
		{
			this.ActiveTool = null;
		}
		Singleton<SoundManager>.Instance.PlayUISound(this.DropItemSound, 1f);
		this.UpdateUI();
	}

	// Token: 0x060003C3 RID: 963 RVA: 0x00014888 File Offset: 0x00012A88
	public void SwapSlots(int indexA, int indexB)
	{
		if (indexA < 0 || indexB < 0 || indexA >= this.Items.Count || indexB >= this.Items.Count)
		{
			return;
		}
		BaseHeldTool baseHeldTool = this.Items[indexA];
		this.Items[indexA] = this.Items[indexB];
		this.Items[indexB] = baseHeldTool;
		if (this.ActiveTool != null)
		{
			if (this.ActiveTool == this.Items[indexB])
			{
				this.SwitchTool(indexA);
			}
			else if (this.ActiveTool == this.Items[indexA])
			{
				this.SwitchTool(indexB);
			}
		}
		this.UpdateUI();
	}

	// Token: 0x060003C4 RID: 964 RVA: 0x00014942 File Offset: 0x00012B42
	private IEnumerator UpdateUINextFrame()
	{
		yield return null;
		this.UpdateUI();
		yield break;
	}

	// Token: 0x040003F9 RID: 1017
	public List<BaseHeldTool> Items = new List<BaseHeldTool>();

	// Token: 0x040003FA RID: 1018
	public List<InventorySlotUI> InventorySlots = new List<InventorySlotUI>();

	// Token: 0x040003FB RID: 1019
	public BaseHeldTool ActiveTool;

	// Token: 0x040003FC RID: 1020
	public SoundDefinition PickupItemSound;

	// Token: 0x040003FD RID: 1021
	public SoundDefinition DropItemSound;

	// Token: 0x040003FE RID: 1022
	private PlayerController _playerController;

	// Token: 0x040003FF RID: 1023
	private static int _maxHotbarSize = 10;

	// Token: 0x04000400 RID: 1024
	private static int _maxInventorySize = 30;

	// Token: 0x04000401 RID: 1025
	private GameObject _mainInventoryPanel;

	// Token: 0x04000402 RID: 1026
	private GameObject _hotbarPanel;

	// Token: 0x04000403 RID: 1027
	private GameObject _inventoryItemsPanel;

	// Token: 0x04000404 RID: 1028
	private GameObject _inventorySlotPrefab;

	// Token: 0x04000405 RID: 1029
	private PlayerInputActions _input;
}
