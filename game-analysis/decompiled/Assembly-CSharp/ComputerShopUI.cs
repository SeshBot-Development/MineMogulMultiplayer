using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000D7 RID: 215
public class ComputerShopUI : MonoBehaviour
{
	// Token: 0x17000021 RID: 33
	// (get) Token: 0x060005B5 RID: 1461 RVA: 0x0001DD5C File Offset: 0x0001BF5C
	// (set) Token: 0x060005B6 RID: 1462 RVA: 0x0001DD64 File Offset: 0x0001BF64
	public int TotalCartPrice { get; private set; }

	// Token: 0x060005B7 RID: 1463 RVA: 0x0001DD70 File Offset: 0x0001BF70
	private void OnEnable()
	{
		if (this._selectedShopCategory == null)
		{
			this.SetupCategories();
		}
		else if (this._hiddenCategoryButtons.Count > 0)
		{
			foreach (ShopCategoryButton shopCategoryButton in this._hiddenCategoryButtons.ToList<ShopCategoryButton>())
			{
				if (shopCategoryButton == null)
				{
					Debug.Log("Shop had a null hidden category button, removing it from the list. (this should never happen???)");
					this._hiddenCategoryButtons.Remove(shopCategoryButton);
				}
				else if (shopCategoryButton.ShopCategory.DontShowIfAllItemsAreLocked)
				{
					if (!shopCategoryButton.ShopCategory.ShopItems.All((ShopItem item) => item.IsLocked))
					{
						shopCategoryButton.gameObject.SetActive(true);
						this._hiddenCategoryButtons.Remove(shopCategoryButton);
					}
				}
			}
		}
		this.RefreshCurrency();
		Singleton<EconomyManager>.Instance.ShopItemUnlocked += this.OnShopItemUnlocked;
	}

	// Token: 0x060005B8 RID: 1464 RVA: 0x0001DE80 File Offset: 0x0001C080
	private void Start()
	{
		this.ClearCart();
		this._input = Singleton<KeybindManager>.Instance.Input;
	}

	// Token: 0x060005B9 RID: 1465 RVA: 0x0001DE98 File Offset: 0x0001C098
	private void OnDisable()
	{
		Singleton<EconomyManager>.Instance.ShopItemUnlocked -= this.OnShopItemUnlocked;
	}

	// Token: 0x060005BA RID: 1466 RVA: 0x0001DEB0 File Offset: 0x0001C0B0
	private void OnShopItemUnlocked(ShopItem shopItem)
	{
		this.RepopulateShopItemList();
	}

	// Token: 0x060005BB RID: 1467 RVA: 0x0001DEB8 File Offset: 0x0001C0B8
	public void RemoveButtonFromCart(ShopCartItemButton button)
	{
		if (this._cartItems.Contains(button))
		{
			this._cartItems.Remove(button);
			Object.Destroy(button.gameObject);
		}
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x0001DEE0 File Offset: 0x0001C0E0
	private void ClearCart()
	{
		foreach (object obj in this.ShopCartListContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (ShopCartItemButton shopCartItemButton in this._cartItems.ToList<ShopCartItemButton>())
		{
			Object.Destroy(shopCartItemButton.gameObject);
		}
		this._cartItems.Clear();
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x0001DF90 File Offset: 0x0001C190
	private void RepopulateShopItemList()
	{
		if (this._selectedShopCategory == null)
		{
			return;
		}
		foreach (object obj in this.ShopItemsListContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (ShopItem shopItem in this._selectedShopCategory.ShopItems.OrderByDescending((ShopItem item) => !item.IsLocked).ToList<ShopItem>())
		{
			Object.Instantiate<GameObject>(this.ShopItemButtonPrefab, this.ShopItemsListContainer).GetComponent<ShopItemButton>().Initialize(shopItem, this);
		}
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x0001E07C File Offset: 0x0001C27C
	public void SetupCategories()
	{
		foreach (ShopCategoryButton shopCategoryButton in this._categoryButtons.ToList<ShopCategoryButton>())
		{
			shopCategoryButton.OnPressed -= this.OpenShopCategory;
			Object.Destroy(shopCategoryButton.gameObject);
		}
		this._categoryButtons.Clear();
		foreach (object obj in this.ShopCategoryButtonsContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (ShopCategory shopCategory in Singleton<EconomyManager>.Instance.GetAvailableShopCategories())
		{
			ShopCategoryButton shopCategoryButton2 = Object.Instantiate<ShopCategoryButton>(this.ShopCategoryButtonPrefab, this.ShopCategoryButtonsContainer);
			shopCategoryButton2.Initialize(shopCategory);
			shopCategoryButton2.OnPressed += this.OpenShopCategory;
			this._categoryButtons.Add(shopCategoryButton2);
			if (shopCategory.DontShowIfAllItemsAreLocked)
			{
				if (shopCategory.ShopItems.All((ShopItem item) => item.IsLocked))
				{
					shopCategoryButton2.gameObject.SetActive(false);
					this._hiddenCategoryButtons.Add(shopCategoryButton2);
				}
			}
		}
		this.OpenShopCategory(this._categoryButtons.FirstOrDefault<ShopCategoryButton>().ShopCategory);
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x0001E228 File Offset: 0x0001C428
	public void OpenShopCategory(ShopCategory category)
	{
		this._selectedShopCategory = category;
		foreach (ShopCategoryButton shopCategoryButton in this._categoryButtons)
		{
			shopCategoryButton.SetSelected(shopCategoryButton.ShopCategory == category);
		}
		this.RepopulateShopItemList();
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x0001E290 File Offset: 0x0001C490
	public void Update()
	{
		this.RefreshCurrency();
		if (this._input.Player.Interact.WasPressedThisFrame())
		{
			base.gameObject.SetActive(false);
		}
		if (this._input.Player.Sprint.WasPressedThisFrame())
		{
			this.SetBuyMultiple(10);
		}
		if (this._input.Player.Sprint.WasReleasedThisFrame())
		{
			this.SetBuyMultiple(1);
		}
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x0001E30C File Offset: 0x0001C50C
	public void AddToCart(ShopItem item, int quantity)
	{
		ShopCartItemButton shopCartItemButton = this._cartItems.FirstOrDefault((ShopCartItemButton ci) => ci.ShopItem == item);
		if (shopCartItemButton != null)
		{
			shopCartItemButton.ChangeQuantity(shopCartItemButton.GetQuantity() + quantity);
			return;
		}
		ShopCartItemButton shopCartItemButton2 = Object.Instantiate<ShopCartItemButton>(this.ShopCartButtonPrefab, this.ShopCartListContainer);
		shopCartItemButton2.Initialize(item, this, quantity);
		this._cartItems.Add(shopCartItemButton2);
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x0001E384 File Offset: 0x0001C584
	public bool CanAffordCart()
	{
		this.TotalCartPrice = this._cartItems.Sum((ShopCartItemButton ci) => ci.ShopItem.GetPrice() * ci.GetQuantity());
		return Singleton<EconomyManager>.Instance.Money >= (float)this.TotalCartPrice && this._cartItems.Count > 0;
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x0001E3E4 File Offset: 0x0001C5E4
	public void PurchaseCart()
	{
		if (this.CanAffordCart())
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.PurchaseSound, Singleton<SoundManager>.Instance.PlayerTransform.position, 1f, 1f, true, false);
			bool flag = true;
			foreach (ShopCartItemButton shopCartItemButton in this._cartItems.ToList<ShopCartItemButton>())
			{
				if (this.TrySpawnItem(shopCartItemButton.ShopItem.Definition, shopCartItemButton.GetQuantity()))
				{
					Singleton<EconomyManager>.Instance.AddMoney((float)(-(float)shopCartItemButton.ShopItem.GetPrice() * shopCartItemButton.GetQuantity()));
					Singleton<EconomyManager>.Instance.ShopPurchases.AddPurchase(shopCartItemButton.ShopItem.GetSavableObjectID(), shopCartItemButton.GetQuantity());
					Object.Destroy(shopCartItemButton.gameObject);
					this._cartItems.Remove(shopCartItemButton);
				}
				else
				{
					flag = false;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("Some items in the cart could not be purchased due to spawn failures.");
			}
			using (HashSet<ShopCategoryButton>.Enumerator enumerator2 = this._categoryButtons.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ShopCategoryButton shopCategoryButton = enumerator2.Current;
					shopCategoryButton.RefreshUI();
				}
				return;
			}
		}
		Debug.Log("Not enough money to complete the purchase.");
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x0001E53C File Offset: 0x0001C73C
	private bool TrySpawnItem(ShopItemDefinition item, int quantity)
	{
		Transform transform = ShopSpawnPoint.GetRandomItemSpawnPoint().transform;
		if (item.BuildingInventoryDefinition != null)
		{
			BuildingCrate buildingCrate = Object.Instantiate<BuildingCrate>(item.BuildingInventoryDefinition.PackedPrefab ? item.BuildingInventoryDefinition.PackedPrefab : Singleton<BuildingManager>.Instance.BuildingCratePrefab, transform.position, transform.rotation);
			buildingCrate.Definition = item.BuildingInventoryDefinition;
			buildingCrate.Quantity = quantity;
			return buildingCrate != null;
		}
		if (item.PrefabToSpawn != null)
		{
			bool flag = false;
			for (int i = 0; i < quantity; i++)
			{
				if (Object.Instantiate<GameObject>(item.PrefabToSpawn, transform.position + Random.insideUnitSphere * 0.5f, transform.rotation) != null)
				{
					flag = true;
				}
				else
				{
					Debug.LogWarning("Failed to spawn extra instance of " + item.GetName());
				}
			}
			return flag;
		}
		return false;
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x0001E624 File Offset: 0x0001C824
	private void RefreshCurrency()
	{
		this._moneyCounter.text = string.Format("${0:#,##0.00}", Singleton<EconomyManager>.Instance.Money);
		if (this.CanAffordCart())
		{
			this._purchaseCartButton.interactable = true;
			this._totalCartPriceText.color = this.CanAffordMoneyColor;
		}
		else
		{
			this._purchaseCartButton.interactable = false;
			if (this._cartItems.Count == 0)
			{
				this._totalCartPriceText.color = this._purchaseCartButton.colors.disabledColor;
			}
			else
			{
				this._totalCartPriceText.color = this.CantAffortMoneyColor;
			}
		}
		this._totalCartPriceText.text = ((this._cartItems.Count > 0) ? string.Format("${0:#,##0.00}", this.TotalCartPrice) : "$0.00");
		this.RefreshButtons();
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x0001E704 File Offset: 0x0001C904
	private void RefreshButtons()
	{
		foreach (object obj in this.ShopItemsListContainer)
		{
			ShopItemButton component = ((Transform)obj).GetComponent<ShopItemButton>();
			if (component != null)
			{
				component.UpdateUI();
			}
		}
	}

	// Token: 0x060005C7 RID: 1479 RVA: 0x0001E76C File Offset: 0x0001C96C
	private void SetBuyMultiple(int quantity)
	{
		foreach (object obj in this.ShopItemsListContainer)
		{
			ShopItemButton component = ((Transform)obj).GetComponent<ShopItemButton>();
			if (component != null && component.ShopItem.Definition.BuildingInventoryDefinition != null)
			{
				component.ChangeQuantity(quantity);
			}
		}
	}

	// Token: 0x040006ED RID: 1773
	public SoundDefinition PurchaseSound;

	// Token: 0x040006EE RID: 1774
	public SoundDefinition AddSound;

	// Token: 0x040006EF RID: 1775
	public SoundDefinition RemoveSound;

	// Token: 0x040006F0 RID: 1776
	public Transform ShopCategoryButtonsContainer;

	// Token: 0x040006F1 RID: 1777
	public Transform ShopItemsListContainer;

	// Token: 0x040006F2 RID: 1778
	public Transform ShopCartListContainer;

	// Token: 0x040006F3 RID: 1779
	public ShopCategoryButton ShopCategoryButtonPrefab;

	// Token: 0x040006F4 RID: 1780
	public GameObject ShopItemButtonPrefab;

	// Token: 0x040006F5 RID: 1781
	public ShopCartItemButton ShopCartButtonPrefab;

	// Token: 0x040006F6 RID: 1782
	[SerializeField]
	private TMP_Text _moneyCounter;

	// Token: 0x040006F7 RID: 1783
	[SerializeField]
	private TMP_Text _totalCartPriceText;

	// Token: 0x040006F8 RID: 1784
	[SerializeField]
	private Button _purchaseCartButton;

	// Token: 0x040006F9 RID: 1785
	public Color CanAffordMoneyColor;

	// Token: 0x040006FA RID: 1786
	public Color CantAffortMoneyColor;

	// Token: 0x040006FB RID: 1787
	private ShopCategory _selectedShopCategory;

	// Token: 0x040006FC RID: 1788
	private HashSet<ShopCategoryButton> _categoryButtons = new HashSet<ShopCategoryButton>();

	// Token: 0x040006FD RID: 1789
	private HashSet<ShopCartItemButton> _cartItems = new HashSet<ShopCartItemButton>();

	// Token: 0x040006FE RID: 1790
	private HashSet<ShopCategoryButton> _hiddenCategoryButtons = new HashSet<ShopCategoryButton>();

	// Token: 0x040006FF RID: 1791
	private PlayerInputActions _input;
}
