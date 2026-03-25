using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCartItemButton : MonoBehaviour
{
	public TMP_Text ItemNameText;

	public TMP_Text ItemPriceText;

	public Image ItemIcon;

	public Color NewlyUnlockedBackgroundColor;

	[SerializeField]
	private TMP_InputField _quantityInputField;

	private ComputerShopUI _shopUI;

	private int _quantity = 1;

	public ShopItem ShopItem { get; private set; }

	public int GetQuantity()
	{
		return _quantity;
	}

	public void Initialize(ShopItem shopItem, ComputerShopUI shopUI, int quantity)
	{
		ShopItem = shopItem;
		ShopItemDefinition definition = shopItem.Definition;
		_shopUI = shopUI;
		_quantity = quantity;
		if (definition.BuildingInventoryDefinition != null)
		{
			ItemIcon.sprite = definition.BuildingInventoryDefinition.GetIcon();
		}
		else
		{
			IIconItem component = definition.PrefabToSpawn.GetComponent<IIconItem>();
			if (component != null)
			{
				ItemIcon.sprite = component.GetIcon();
			}
		}
		UpdateUI();
	}

	private void OnEnable()
	{
		_quantityInputField.onEndEdit.AddListener(OnInputSubmitted);
		UpdateUI();
	}

	private void OnDisable()
	{
		_quantityInputField.onEndEdit.RemoveListener(OnInputSubmitted);
	}

	private void OnInputSubmitted(string input)
	{
		if (int.TryParse(input, out var result))
		{
			ChangeQuantity(result);
		}
	}

	public void ChangeQuantity(int quantity)
	{
		int max = _quantity;
		if (Singleton<EconomyManager>.Instance != null && _shopUI != null && ShopItem != null)
		{
			max = Mathf.FloorToInt((Singleton<EconomyManager>.Instance.Money - (float)_shopUI.TotalCartPrice + (float)(ShopItem.GetPrice() * _quantity)) / (float)ShopItem.GetPrice());
			max = Mathf.Max(0, max);
		}
		int a = Mathf.Clamp(quantity, -1, max);
		a = ((!(ShopItem.Definition.BuildingInventoryDefinition != null)) ? Mathf.Min(a, 10) : Mathf.Min(a, ShopItem.Definition.BuildingInventoryDefinition.MaxInventoryStackSize));
		_quantity = a;
		if (_quantity > 0)
		{
			UpdateUI();
		}
		else
		{
			_shopUI.RemoveButtonFromCart(this);
		}
	}

	public void AddQuantity(int amount)
	{
		ChangeQuantity(_quantity + amount);
		if (amount > 0)
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(_shopUI.AddSound, Singleton<SoundManager>.Instance.PlayerTransform.position);
		}
		else
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(_shopUI.RemoveSound, Singleton<SoundManager>.Instance.PlayerTransform.position);
		}
	}

	public void RemoveFromCart()
	{
		ChangeQuantity(0);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_shopUI.RemoveSound, Singleton<SoundManager>.Instance.PlayerTransform.position);
	}

	public void UpdateUI()
	{
		if (!(_shopUI == null))
		{
			float num = ShopItem.GetPrice() * _quantity;
			ItemPriceText.text = $"${num}";
			ItemNameText.text = ShopItem.GetName();
			_quantityInputField.text = _quantity.ToString();
		}
	}
}
