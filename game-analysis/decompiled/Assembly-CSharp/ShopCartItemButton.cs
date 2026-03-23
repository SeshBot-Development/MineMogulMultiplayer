using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000D8 RID: 216
public class ShopCartItemButton : MonoBehaviour
{
	// Token: 0x17000022 RID: 34
	// (get) Token: 0x060005C9 RID: 1481 RVA: 0x0001E815 File Offset: 0x0001CA15
	// (set) Token: 0x060005CA RID: 1482 RVA: 0x0001E81D File Offset: 0x0001CA1D
	public ShopItem ShopItem { get; private set; }

	// Token: 0x060005CB RID: 1483 RVA: 0x0001E826 File Offset: 0x0001CA26
	public int GetQuantity()
	{
		return this._quantity;
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x0001E830 File Offset: 0x0001CA30
	public void Initialize(ShopItem shopItem, ComputerShopUI shopUI, int quantity)
	{
		this.ShopItem = shopItem;
		ShopItemDefinition definition = shopItem.Definition;
		this._shopUI = shopUI;
		this._quantity = quantity;
		if (definition.BuildingInventoryDefinition != null)
		{
			this.ItemIcon.sprite = definition.BuildingInventoryDefinition.GetIcon();
		}
		else
		{
			IIconItem component = definition.PrefabToSpawn.GetComponent<IIconItem>();
			if (component != null)
			{
				this.ItemIcon.sprite = component.GetIcon();
			}
		}
		this.UpdateUI();
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x0001E8A5 File Offset: 0x0001CAA5
	private void OnEnable()
	{
		this._quantityInputField.onEndEdit.AddListener(new UnityAction<string>(this.OnInputSubmitted));
		this.UpdateUI();
	}

	// Token: 0x060005CE RID: 1486 RVA: 0x0001E8C9 File Offset: 0x0001CAC9
	private void OnDisable()
	{
		this._quantityInputField.onEndEdit.RemoveListener(new UnityAction<string>(this.OnInputSubmitted));
	}

	// Token: 0x060005CF RID: 1487 RVA: 0x0001E8E8 File Offset: 0x0001CAE8
	private void OnInputSubmitted(string input)
	{
		int num;
		if (int.TryParse(input, out num))
		{
			this.ChangeQuantity(num);
		}
	}

	// Token: 0x060005D0 RID: 1488 RVA: 0x0001E908 File Offset: 0x0001CB08
	public void ChangeQuantity(int quantity)
	{
		int num = this._quantity;
		if (Singleton<EconomyManager>.Instance != null && this._shopUI != null && this.ShopItem != null)
		{
			num = Mathf.FloorToInt((Singleton<EconomyManager>.Instance.Money - (float)this._shopUI.TotalCartPrice + (float)(this.ShopItem.GetPrice() * this._quantity)) / (float)this.ShopItem.GetPrice());
			num = Mathf.Max(0, num);
		}
		int num2 = Mathf.Clamp(quantity, -1, num);
		if (this.ShopItem.Definition.BuildingInventoryDefinition != null)
		{
			num2 = Mathf.Min(num2, this.ShopItem.Definition.BuildingInventoryDefinition.MaxInventoryStackSize);
		}
		else
		{
			num2 = Mathf.Min(num2, 10);
		}
		this._quantity = num2;
		if (this._quantity > 0)
		{
			this.UpdateUI();
			return;
		}
		this._shopUI.RemoveButtonFromCart(this);
	}

	// Token: 0x060005D1 RID: 1489 RVA: 0x0001E9F0 File Offset: 0x0001CBF0
	public void AddQuantity(int amount)
	{
		this.ChangeQuantity(this._quantity + amount);
		if (amount > 0)
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._shopUI.AddSound, Singleton<SoundManager>.Instance.PlayerTransform.position, 1f, 1f, true, false);
			return;
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._shopUI.RemoveSound, Singleton<SoundManager>.Instance.PlayerTransform.position, 1f, 1f, true, false);
	}

	// Token: 0x060005D2 RID: 1490 RVA: 0x0001EA70 File Offset: 0x0001CC70
	public void RemoveFromCart()
	{
		this.ChangeQuantity(0);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._shopUI.RemoveSound, Singleton<SoundManager>.Instance.PlayerTransform.position, 1f, 1f, true, false);
	}

	// Token: 0x060005D3 RID: 1491 RVA: 0x0001EAAC File Offset: 0x0001CCAC
	public void UpdateUI()
	{
		if (this._shopUI == null)
		{
			return;
		}
		float num = (float)(this.ShopItem.GetPrice() * this._quantity);
		this.ItemPriceText.text = string.Format("${0}", num);
		this.ItemNameText.text = this.ShopItem.GetName();
		this._quantityInputField.text = this._quantity.ToString();
	}

	// Token: 0x04000700 RID: 1792
	public TMP_Text ItemNameText;

	// Token: 0x04000701 RID: 1793
	public TMP_Text ItemPriceText;

	// Token: 0x04000702 RID: 1794
	public Image ItemIcon;

	// Token: 0x04000703 RID: 1795
	public Color NewlyUnlockedBackgroundColor;

	// Token: 0x04000704 RID: 1796
	[SerializeField]
	private TMP_InputField _quantityInputField;

	// Token: 0x04000706 RID: 1798
	private ComputerShopUI _shopUI;

	// Token: 0x04000707 RID: 1799
	private int _quantity = 1;
}
