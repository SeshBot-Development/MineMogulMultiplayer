using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200005E RID: 94
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
	// Token: 0x0600024D RID: 589 RVA: 0x0000BB77 File Offset: 0x00009D77
	private bool IsHotbarSlot()
	{
		return this.SlotIndex < 10;
	}

	// Token: 0x0600024E RID: 590 RVA: 0x0000BB84 File Offset: 0x00009D84
	private void Awake()
	{
		this.Text.text = "";
		this.AmountText.text = "";
		this.Icon.enabled = false;
		this._inventory = Object.FindObjectOfType<PlayerInventory>();
		this._canvas = base.GetComponentInParent<Canvas>();
		this.OrangeBarThing.SetActive(this.IsHotbarSlot());
		this.SlotNumberText.gameObject.SetActive(this.IsHotbarSlot());
	}

	// Token: 0x0600024F RID: 591 RVA: 0x0000BBFB File Offset: 0x00009DFB
	private void Start()
	{
		if (this.IsHotbarSlot())
		{
			this.SlotNumberText.SetText(string.Format("[HotbarSlot{0}]", this.SlotIndex + 1));
		}
	}

	// Token: 0x06000250 RID: 592 RVA: 0x0000BC28 File Offset: 0x00009E28
	private void OnEnable()
	{
		this._isHovered = false;
		this.UpdateBackgroundColor();
		if (this.IsHotbarSlot())
		{
			Singleton<InventoryUIManager>.Instance.InventoryOpened += this.OnInventoryOpened;
			Singleton<InventoryUIManager>.Instance.InventoryClosed += this.OnInventoryClosed;
		}
	}

	// Token: 0x06000251 RID: 593 RVA: 0x0000BC78 File Offset: 0x00009E78
	private void OnDisable()
	{
		this.HideWhenDragged.SetActive(true);
		if (this.IsHotbarSlot())
		{
			Singleton<InventoryUIManager>.Instance.InventoryOpened -= this.OnInventoryOpened;
			Singleton<InventoryUIManager>.Instance.InventoryClosed -= this.OnInventoryClosed;
		}
	}

	// Token: 0x06000252 RID: 594 RVA: 0x0000BCC5 File Offset: 0x00009EC5
	public void SetHighlighted(bool selected)
	{
		this._isSelected = selected;
		this.Icon.color = new Color(1f, 1f, 1f, 0.9f);
		this.UpdateBackgroundColor();
	}

	// Token: 0x06000253 RID: 595 RVA: 0x0000BCF8 File Offset: 0x00009EF8
	public void OnInventoryOpened()
	{
		this.SlotNumberText.gameObject.SetActive(false);
	}

	// Token: 0x06000254 RID: 596 RVA: 0x0000BD0B File Offset: 0x00009F0B
	public void OnInventoryClosed()
	{
		if (this.IsHotbarSlot())
		{
			this.SlotNumberText.gameObject.SetActive(true);
		}
	}

	// Token: 0x06000255 RID: 597 RVA: 0x0000BD28 File Offset: 0x00009F28
	private void UpdateBackgroundColor()
	{
		if (this._isHovered)
		{
			this.Background.color = this.HoveredColor;
			return;
		}
		if (this._isSelected)
		{
			this.Background.color = this.SelectedColor;
			return;
		}
		this.Background.color = this.NotSelectedColor;
	}

	// Token: 0x06000256 RID: 598 RVA: 0x0000BD7C File Offset: 0x00009F7C
	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!Singleton<UIManager>.Instance.IsInInventory())
		{
			return;
		}
		BaseHeldTool baseHeldTool = this._inventory.Items[this.SlotIndex];
		if (baseHeldTool == null)
		{
			return;
		}
		this.HideWhenDragged.SetActive(false);
		InventoryUIManager instance = Singleton<InventoryUIManager>.Instance;
		instance.DragGhostIcon.SetActive(true);
		instance.DragGhostImage.sprite = baseHeldTool.GetIcon();
		instance.DragGhostAmountText.text = ((baseHeldTool.Quantity > 1) ? baseHeldTool.Quantity.ToString() : "");
		instance.DragGhostIcon.transform.SetAsLastSibling();
	}

	// Token: 0x06000257 RID: 599 RVA: 0x0000BE1A File Offset: 0x0000A01A
	public void OnDrag(PointerEventData eventData)
	{
		if (!Singleton<UIManager>.Instance.IsInInventory())
		{
			this.OnEndDrag(eventData);
			return;
		}
		Singleton<InventoryUIManager>.Instance.DragGhostIcon.transform.position = eventData.position;
	}

	// Token: 0x06000258 RID: 600 RVA: 0x0000BE4F File Offset: 0x0000A04F
	public void OnEndDrag(PointerEventData eventData)
	{
		this.HideWhenDragged.SetActive(true);
		Singleton<InventoryUIManager>.Instance.DragGhostIcon.SetActive(false);
		if (eventData.pointerEnter == null)
		{
			this._inventory.DropItemAtIndex(this.SlotIndex);
		}
	}

	// Token: 0x06000259 RID: 601 RVA: 0x0000BE8C File Offset: 0x0000A08C
	public void OnDrop(PointerEventData eventData)
	{
		GameObject pointerDrag = eventData.pointerDrag;
		InventorySlotUI inventorySlotUI = ((pointerDrag != null) ? pointerDrag.GetComponent<InventorySlotUI>() : null);
		if (inventorySlotUI == null || inventorySlotUI == this)
		{
			return;
		}
		if (this._inventory.Items[inventorySlotUI.SlotIndex] == null)
		{
			return;
		}
		this._inventory.SwapSlots(this.SlotIndex, inventorySlotUI.SlotIndex);
	}

	// Token: 0x0600025A RID: 602 RVA: 0x0000BEF8 File Offset: 0x0000A0F8
	public void OnPointerDown(PointerEventData eventData)
	{
		if (!Singleton<UIManager>.Instance.IsInInventory())
		{
			return;
		}
		BaseHeldTool baseHeldTool = this._inventory.Items[this.SlotIndex];
		Singleton<InventoryItemPreview>.Instance.StartPreview(baseHeldTool, false);
		Singleton<InventoryUIManager>.Instance.UpdateSelectedItemInfo(baseHeldTool);
	}

	// Token: 0x0600025B RID: 603 RVA: 0x0000BF40 File Offset: 0x0000A140
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!Singleton<UIManager>.Instance.IsInInventory())
		{
			return;
		}
		this._isHovered = true;
		this.UpdateBackgroundColor();
		Singleton<SoundManager>.Instance.PlayUISound(Singleton<SoundManager>.Instance.Sound_UI_Inventory_Icon_Hover, 1f);
	}

	// Token: 0x0600025C RID: 604 RVA: 0x0000BF75 File Offset: 0x0000A175
	public void OnPointerExit(PointerEventData eventData)
	{
		this._isHovered = false;
		this.UpdateBackgroundColor();
	}

	// Token: 0x0400021D RID: 541
	public TMP_Text Text;

	// Token: 0x0400021E RID: 542
	public TMP_Text AmountText;

	// Token: 0x0400021F RID: 543
	public KeybindTokenText SlotNumberText;

	// Token: 0x04000220 RID: 544
	public Image Icon;

	// Token: 0x04000221 RID: 545
	public Image Background;

	// Token: 0x04000222 RID: 546
	public GameObject OrangeBarThing;

	// Token: 0x04000223 RID: 547
	public GameObject HideWhenDragged;

	// Token: 0x04000224 RID: 548
	public Color SelectedColor = new Color(0.4f, 0.8f, 0.6f, 0.2f);

	// Token: 0x04000225 RID: 549
	public Color NotSelectedColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);

	// Token: 0x04000226 RID: 550
	public Color HoveredColor = new Color(1f, 1f, 1f, 0.15f);

	// Token: 0x04000227 RID: 551
	private bool _isSelected;

	// Token: 0x04000228 RID: 552
	private bool _isHovered;

	// Token: 0x04000229 RID: 553
	public int SlotIndex;

	// Token: 0x0400022A RID: 554
	private PlayerInventory _inventory;

	// Token: 0x0400022B RID: 555
	private Canvas _canvas;
}
