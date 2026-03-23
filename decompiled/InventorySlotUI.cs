using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
	public TMP_Text Text;

	public TMP_Text AmountText;

	public KeybindTokenText SlotNumberText;

	public Image Icon;

	public Image Background;

	public GameObject OrangeBarThing;

	public GameObject HideWhenDragged;

	public Color SelectedColor = new Color(0.4f, 0.8f, 0.6f, 0.2f);

	public Color NotSelectedColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);

	public Color HoveredColor = new Color(1f, 1f, 1f, 0.15f);

	private bool _isSelected;

	private bool _isHovered;

	public int SlotIndex;

	private PlayerInventory _inventory;

	private Canvas _canvas;

	private bool IsHotbarSlot()
	{
		return SlotIndex < 10;
	}

	private void Awake()
	{
		Text.text = "";
		AmountText.text = "";
		Icon.enabled = false;
		_inventory = Object.FindObjectOfType<PlayerInventory>();
		_canvas = GetComponentInParent<Canvas>();
		OrangeBarThing.SetActive(IsHotbarSlot());
		SlotNumberText.gameObject.SetActive(IsHotbarSlot());
	}

	private void Start()
	{
		if (IsHotbarSlot())
		{
			SlotNumberText.SetText($"[HotbarSlot{SlotIndex + 1}]");
		}
	}

	private void OnEnable()
	{
		_isHovered = false;
		UpdateBackgroundColor();
		if (IsHotbarSlot())
		{
			Singleton<InventoryUIManager>.Instance.InventoryOpened += OnInventoryOpened;
			Singleton<InventoryUIManager>.Instance.InventoryClosed += OnInventoryClosed;
		}
	}

	private void OnDisable()
	{
		HideWhenDragged.SetActive(value: true);
		if (IsHotbarSlot())
		{
			Singleton<InventoryUIManager>.Instance.InventoryOpened -= OnInventoryOpened;
			Singleton<InventoryUIManager>.Instance.InventoryClosed -= OnInventoryClosed;
		}
	}

	public void SetHighlighted(bool selected)
	{
		_isSelected = selected;
		Icon.color = new Color(1f, 1f, 1f, 0.9f);
		UpdateBackgroundColor();
	}

	public void OnInventoryOpened()
	{
		SlotNumberText.gameObject.SetActive(value: false);
	}

	public void OnInventoryClosed()
	{
		if (IsHotbarSlot())
		{
			SlotNumberText.gameObject.SetActive(value: true);
		}
	}

	private void UpdateBackgroundColor()
	{
		if (_isHovered)
		{
			Background.color = HoveredColor;
		}
		else if (_isSelected)
		{
			Background.color = SelectedColor;
		}
		else
		{
			Background.color = NotSelectedColor;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (Singleton<UIManager>.Instance.IsInInventory())
		{
			BaseHeldTool baseHeldTool = _inventory.Items[SlotIndex];
			if (!(baseHeldTool == null))
			{
				HideWhenDragged.SetActive(value: false);
				InventoryUIManager instance = Singleton<InventoryUIManager>.Instance;
				instance.DragGhostIcon.SetActive(value: true);
				instance.DragGhostImage.sprite = baseHeldTool.GetIcon();
				instance.DragGhostAmountText.text = ((baseHeldTool.Quantity > 1) ? baseHeldTool.Quantity.ToString() : "");
				instance.DragGhostIcon.transform.SetAsLastSibling();
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!Singleton<UIManager>.Instance.IsInInventory())
		{
			OnEndDrag(eventData);
		}
		else
		{
			Singleton<InventoryUIManager>.Instance.DragGhostIcon.transform.position = eventData.position;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		HideWhenDragged.SetActive(value: true);
		Singleton<InventoryUIManager>.Instance.DragGhostIcon.SetActive(value: false);
		if (eventData.pointerEnter == null)
		{
			_inventory.DropItemAtIndex(SlotIndex);
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		InventorySlotUI inventorySlotUI = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
		if (!(inventorySlotUI == null) && !(inventorySlotUI == this) && !(_inventory.Items[inventorySlotUI.SlotIndex] == null))
		{
			_inventory.SwapSlots(SlotIndex, inventorySlotUI.SlotIndex);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (Singleton<UIManager>.Instance.IsInInventory())
		{
			BaseHeldTool baseHeldTool = _inventory.Items[SlotIndex];
			Singleton<InventoryItemPreview>.Instance.StartPreview(baseHeldTool, isGeneratingIcons: false);
			Singleton<InventoryUIManager>.Instance.UpdateSelectedItemInfo(baseHeldTool);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Singleton<UIManager>.Instance.IsInInventory())
		{
			_isHovered = true;
			UpdateBackgroundColor();
			Singleton<SoundManager>.Instance.PlayUISound(Singleton<SoundManager>.Instance.Sound_UI_Inventory_Icon_Hover);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHovered = false;
		UpdateBackgroundColor();
	}
}
