using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerShopUI : MonoBehaviour
{
	public SoundDefinition PurchaseSound;

	public SoundDefinition AddSound;

	public SoundDefinition RemoveSound;

	public Transform ShopCategoryButtonsContainer;

	public Transform ShopItemsListContainer;

	public Transform ShopCartListContainer;

	public ShopCategoryButton ShopCategoryButtonPrefab;

	public GameObject ShopItemButtonPrefab;

	public ShopCartItemButton ShopCartButtonPrefab;

	[SerializeField]
	private TMP_Text _moneyCounter;

	[SerializeField]
	private TMP_Text _totalCartPriceText;

	[SerializeField]
	private Button _purchaseCartButton;

	public Color CanAffordMoneyColor;

	public Color CantAffortMoneyColor;

	private ShopCategory _selectedShopCategory;

	private HashSet<ShopCategoryButton> _categoryButtons = new HashSet<ShopCategoryButton>();

	private HashSet<ShopCartItemButton> _cartItems = new HashSet<ShopCartItemButton>();

	private HashSet<ShopCategoryButton> _hiddenCategoryButtons = new HashSet<ShopCategoryButton>();

	private PlayerInputActions _input;

	public int TotalCartPrice { get; private set; }

	private void OnEnable()
	{
		if (_selectedShopCategory == null)
		{
			SetupCategories();
		}
		else if (_hiddenCategoryButtons.Count > 0)
		{
			foreach (ShopCategoryButton item in _hiddenCategoryButtons.ToList())
			{
				if (item == null)
				{
					Debug.Log("Shop had a null hidden category button, removing it from the list. (this should never happen???)");
					_hiddenCategoryButtons.Remove(item);
				}
				else if (item.ShopCategory.DontShowIfAllItemsAreLocked && !item.ShopCategory.ShopItems.All((ShopItem item) => item.IsLocked))
				{
					item.gameObject.SetActive(value: true);
					_hiddenCategoryButtons.Remove(item);
				}
			}
		}
		RefreshCurrency();
		Singleton<EconomyManager>.Instance.ShopItemUnlocked += OnShopItemUnlocked;
	}

	private void Start()
	{
		ClearCart();
		_input = Singleton<KeybindManager>.Instance.Input;
	}

	private void OnDisable()
	{
		Singleton<EconomyManager>.Instance.ShopItemUnlocked -= OnShopItemUnlocked;
	}

	private void OnShopItemUnlocked(ShopItem shopItem)
	{
		RepopulateShopItemList();
	}

	public void RemoveButtonFromCart(ShopCartItemButton button)
	{
		if (_cartItems.Contains(button))
		{
			_cartItems.Remove(button);
			Object.Destroy(button.gameObject);
		}
	}

	private void ClearCart()
	{
		foreach (Transform item in ShopCartListContainer)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (ShopCartItemButton item2 in _cartItems.ToList())
		{
			Object.Destroy(item2.gameObject);
		}
		_cartItems.Clear();
	}

	private void RepopulateShopItemList()
	{
		if (_selectedShopCategory == null)
		{
			return;
		}
		foreach (Transform item in ShopItemsListContainer)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (ShopItem item2 in _selectedShopCategory.ShopItems.OrderByDescending((ShopItem item) => !item.IsLocked).ToList())
		{
			Object.Instantiate(ShopItemButtonPrefab, ShopItemsListContainer).GetComponent<ShopItemButton>().Initialize(item2, this);
		}
	}

	public void SetupCategories()
	{
		foreach (ShopCategoryButton item in _categoryButtons.ToList())
		{
			item.OnPressed -= OpenShopCategory;
			Object.Destroy(item.gameObject);
		}
		_categoryButtons.Clear();
		foreach (Transform item2 in ShopCategoryButtonsContainer)
		{
			Object.Destroy(item2.gameObject);
		}
		foreach (ShopCategory availableShopCategory in Singleton<EconomyManager>.Instance.GetAvailableShopCategories())
		{
			ShopCategoryButton shopCategoryButton = Object.Instantiate(ShopCategoryButtonPrefab, ShopCategoryButtonsContainer);
			shopCategoryButton.Initialize(availableShopCategory);
			shopCategoryButton.OnPressed += OpenShopCategory;
			_categoryButtons.Add(shopCategoryButton);
			if (availableShopCategory.DontShowIfAllItemsAreLocked && (!Singleton<GamemodeManager>.Instance.ShouldUseFreeShop() || availableShopCategory.IsTrophyCategory) && availableShopCategory.ShopItems.All((ShopItem item) => item.IsLocked))
			{
				shopCategoryButton.gameObject.SetActive(value: false);
				_hiddenCategoryButtons.Add(shopCategoryButton);
			}
		}
		OpenShopCategory(_categoryButtons.FirstOrDefault().ShopCategory);
	}

	public void OpenShopCategory(ShopCategory category)
	{
		_selectedShopCategory = category;
		foreach (ShopCategoryButton categoryButton in _categoryButtons)
		{
			categoryButton.SetSelected(categoryButton.ShopCategory == category);
		}
		RepopulateShopItemList();
	}

	public void Update()
	{
		RefreshCurrency();
		if (_input.Player.Interact.WasPressedThisFrame())
		{
			base.gameObject.SetActive(value: false);
		}
		if (_input.Player.Sprint.WasPressedThisFrame())
		{
			SetBuyMultiple(10);
		}
		if (_input.Player.Sprint.WasReleasedThisFrame())
		{
			SetBuyMultiple(1);
		}
	}

	public void AddToCart(ShopItem item, int quantity)
	{
		ShopCartItemButton shopCartItemButton = _cartItems.FirstOrDefault((ShopCartItemButton ci) => ci.ShopItem == item);
		if (shopCartItemButton != null)
		{
			shopCartItemButton.ChangeQuantity(shopCartItemButton.GetQuantity() + quantity);
			return;
		}
		ShopCartItemButton shopCartItemButton2 = Object.Instantiate(ShopCartButtonPrefab, ShopCartListContainer);
		shopCartItemButton2.Initialize(item, this, quantity);
		_cartItems.Add(shopCartItemButton2);
	}

	public bool CanAffordCart()
	{
		TotalCartPrice = _cartItems.Sum((ShopCartItemButton ci) => ci.ShopItem.GetPrice() * ci.GetQuantity());
		if (Singleton<GamemodeManager>.Instance.ShouldUseFreeShop())
		{
			return _cartItems.Count > 0;
		}
		if (Singleton<EconomyManager>.Instance.Money >= (float)TotalCartPrice)
		{
			return _cartItems.Count > 0;
		}
		return false;
	}

	public void PurchaseCart()
	{
		if (CanAffordCart())
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(PurchaseSound, Singleton<SoundManager>.Instance.PlayerTransform.position);
			bool flag = true;
			foreach (ShopCartItemButton item in _cartItems.ToList())
			{
				if (TrySpawnItem(item.ShopItem.Definition, item.GetQuantity()))
				{
					Singleton<EconomyManager>.Instance.AddMoney(-item.ShopItem.GetPrice() * item.GetQuantity());
					Singleton<EconomyManager>.Instance.ShopPurchases.AddPurchase(item.ShopItem.GetSavableObjectID(), item.GetQuantity());
					Object.Destroy(item.gameObject);
					_cartItems.Remove(item);
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
			foreach (ShopCategoryButton categoryButton in _categoryButtons)
			{
				categoryButton.RefreshUI();
			}
			if (Singleton<GamemodeManager>.Instance.ShouldUseFreeShop() && Singleton<EconomyManager>.Instance.Money < 0f)
			{
				Singleton<EconomyManager>.Instance.SetMoney(0f);
			}
		}
		else
		{
			Debug.Log("Not enough money to complete the purchase.");
		}
	}

	private bool TrySpawnItem(ShopItemDefinition item, int quantity)
	{
		Transform transform = ShopSpawnPoint.GetRandomItemSpawnPoint().transform;
		if (item.BuildingInventoryDefinition != null)
		{
			BuildingCrate buildingCrate = Object.Instantiate(item.BuildingInventoryDefinition.PackedPrefab ? item.BuildingInventoryDefinition.PackedPrefab : Singleton<BuildingManager>.Instance.BuildingCratePrefab, transform.position, transform.rotation);
			buildingCrate.Definition = item.BuildingInventoryDefinition;
			buildingCrate.Quantity = quantity;
			return buildingCrate != null;
		}
		if (item.PrefabToSpawn != null)
		{
			bool result = false;
			for (int i = 0; i < quantity; i++)
			{
				if (Object.Instantiate(item.PrefabToSpawn, transform.position + Random.insideUnitSphere * 0.5f, transform.rotation) != null)
				{
					result = true;
				}
				else
				{
					Debug.LogWarning("Failed to spawn extra instance of " + item.GetName());
				}
			}
			return result;
		}
		return false;
	}

	private void RefreshCurrency()
	{
		if (Singleton<GamemodeManager>.Instance.ShouldUseFreeShop())
		{
			_moneyCounter.text = "$Infinite";
		}
		else
		{
			_moneyCounter.text = $"${Singleton<EconomyManager>.Instance.Money:#,##0.00}";
		}
		if (CanAffordCart())
		{
			_purchaseCartButton.interactable = true;
			_totalCartPriceText.color = CanAffordMoneyColor;
		}
		else
		{
			_purchaseCartButton.interactable = false;
			if (_cartItems.Count == 0)
			{
				_totalCartPriceText.color = _purchaseCartButton.colors.disabledColor;
			}
			else
			{
				_totalCartPriceText.color = CantAffortMoneyColor;
			}
		}
		_totalCartPriceText.text = ((_cartItems.Count > 0) ? $"${TotalCartPrice:#,##0.00}" : "$0.00");
		RefreshButtons();
	}

	private void RefreshButtons()
	{
		foreach (Transform item in ShopItemsListContainer)
		{
			ShopItemButton component = item.GetComponent<ShopItemButton>();
			if (component != null)
			{
				component.UpdateUI();
			}
		}
	}

	private void SetBuyMultiple(int quantity)
	{
		foreach (Transform item in ShopItemsListContainer)
		{
			ShopItemButton component = item.GetComponent<ShopItemButton>();
			if (component != null && component.ShopItem.Definition.BuildingInventoryDefinition != null)
			{
				component.ChangeQuantity(quantity);
			}
		}
	}
}
