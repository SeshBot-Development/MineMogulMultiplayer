using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
	public Button Button;

	public Image ButtonImage;

	public TMP_Text ButtonText;

	public TMP_Text ItemNameText;

	public KeybindTokenText ItemDescriptionText;

	public TMP_Text ItemPriceText;

	public Image ItemIcon;

	public Color ButtonCanBuyColor;

	public Color ButtonCantBuyColor;

	public Color ButtonTextCanBuyColor;

	public Color ButtonTextCantBuyColor;

	public Color PriceCanBuyColor;

	public Color PriceCantBuyColor;

	public GameObject NewIcon;

	public Color ItemNameRegularColor;

	public Color ItemNameNewlyUnlockedColor;

	private ComputerShopUI _shopUI;

	private int _quantity = 1;

	private Tween _colorTween;

	public ShopItem ShopItem { get; private set; }

	public void Initialize(ShopItem shopItem, ComputerShopUI shopUI)
	{
		ShopItem = shopItem;
		ShopItemDefinition definition = shopItem.Definition;
		_shopUI = shopUI;
		ItemDescriptionText.SetText(shopItem.GetDescription());
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
		Button.onClick.AddListener(OnButtonClick);
		UpdateUI();
	}

	public void OnButtonClick()
	{
		_shopUI.AddToCart(ShopItem, _quantity);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_shopUI.AddSound, Singleton<SoundManager>.Instance.PlayerTransform.position);
		UpdateUI();
	}

	public void ChangeQuantity(int quantity)
	{
		_quantity = quantity;
		UpdateUI();
	}

	private void OnDisable()
	{
		_colorTween?.Kill();
	}

	public void UpdateUI()
	{
		if (_shopUI == null)
		{
			return;
		}
		float num = ShopItem.GetPrice() * _quantity;
		bool flag = Singleton<EconomyManager>.Instance.Money - (float)_shopUI.TotalCartPrice >= num && !ShopItem.IsLocked;
		if (Singleton<GamemodeManager>.Instance.ShouldUseFreeShop())
		{
			flag = true;
			if (ShopItem.IsLocked)
			{
				ShopItem.IsLocked = false;
			}
		}
		Button.interactable = flag;
		if (_quantity == 1)
		{
			ItemPriceText.text = $"${num}";
		}
		else
		{
			ItemPriceText.text = $"(x{_quantity}) ${num}";
		}
		ItemNameText.text = ShopItem.GetName();
		if (ShopItem.IsLocked)
		{
			if (ShopItem.Definition.IsLockedInDemo && Singleton<DemoManager>.Instance.IsDemoVersion)
			{
				ButtonText.text = "Locked in demo";
			}
			else
			{
				ButtonText.text = "Locked";
			}
			ButtonText.color = ButtonTextCantBuyColor;
			ButtonImage.color = ButtonCantBuyColor;
			ItemIcon.material = Singleton<UIManager>.Instance.GrayscaleImageMaterial;
		}
		else if (!flag)
		{
			ButtonText.text = "Can't Afford";
			ButtonText.color = ButtonTextCantBuyColor;
			ButtonImage.color = ButtonCantBuyColor;
			ItemIcon.material = null;
		}
		else
		{
			ButtonText.text = "Add to Cart";
			ButtonText.color = ButtonTextCanBuyColor;
			ButtonImage.color = ButtonCanBuyColor;
			ItemIcon.material = null;
		}
		bool flag2 = ShopItem.IsNewlyUnlocked();
		NewIcon.SetActive(flag2);
		if (flag2)
		{
			if (_colorTween == null)
			{
				_colorTween = ItemNameText.DOColor(ItemNameNewlyUnlockedColor, 2.5f).SetLoops(-1, LoopType.Yoyo);
			}
		}
		else
		{
			_colorTween?.Kill();
			ItemNameText.color = ItemNameRegularColor;
		}
	}
}
