using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000DA RID: 218
public class ShopItemButton : MonoBehaviour
{
	// Token: 0x17000023 RID: 35
	// (get) Token: 0x060005DF RID: 1503 RVA: 0x0001ECDF File Offset: 0x0001CEDF
	// (set) Token: 0x060005E0 RID: 1504 RVA: 0x0001ECE7 File Offset: 0x0001CEE7
	public ShopItem ShopItem { get; private set; }

	// Token: 0x060005E1 RID: 1505 RVA: 0x0001ECF0 File Offset: 0x0001CEF0
	public void Initialize(ShopItem shopItem, ComputerShopUI shopUI)
	{
		this.ShopItem = shopItem;
		ShopItemDefinition definition = shopItem.Definition;
		this._shopUI = shopUI;
		this.ItemDescriptionText.SetText(shopItem.GetDescription());
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
		this.Button.onClick.AddListener(new UnityAction(this.OnButtonClick));
		this.UpdateUI();
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x0001ED8C File Offset: 0x0001CF8C
	public void OnButtonClick()
	{
		this._shopUI.AddToCart(this.ShopItem, this._quantity);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._shopUI.AddSound, Singleton<SoundManager>.Instance.PlayerTransform.position, 1f, 1f, true, false);
		this.UpdateUI();
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x0001EDE6 File Offset: 0x0001CFE6
	public void ChangeQuantity(int quantity)
	{
		this._quantity = quantity;
		this.UpdateUI();
	}

	// Token: 0x060005E4 RID: 1508 RVA: 0x0001EDF5 File Offset: 0x0001CFF5
	private void OnDisable()
	{
		Tween colorTween = this._colorTween;
		if (colorTween == null)
		{
			return;
		}
		colorTween.Kill(false);
	}

	// Token: 0x060005E5 RID: 1509 RVA: 0x0001EE08 File Offset: 0x0001D008
	public void UpdateUI()
	{
		if (this._shopUI == null)
		{
			return;
		}
		float num = (float)(this.ShopItem.GetPrice() * this._quantity);
		bool flag = Singleton<EconomyManager>.Instance.Money - (float)this._shopUI.TotalCartPrice >= num && !this.ShopItem.IsLocked;
		this.Button.interactable = flag;
		if (this._quantity == 1)
		{
			this.ItemPriceText.text = string.Format("${0}", num);
		}
		else
		{
			this.ItemPriceText.text = string.Format("(x{0}) ${1}", this._quantity, num);
		}
		this.ItemNameText.text = this.ShopItem.GetName();
		if (this.ShopItem.IsLocked)
		{
			this.ButtonText.text = "Locked";
			this.ButtonText.color = this.ButtonTextCantBuyColor;
			this.ButtonImage.color = this.ButtonCantBuyColor;
			this.ItemIcon.material = Singleton<UIManager>.Instance.GrayscaleImageMaterial;
		}
		else if (!flag)
		{
			this.ButtonText.text = "Can't Afford";
			this.ButtonText.color = this.ButtonTextCantBuyColor;
			this.ButtonImage.color = this.ButtonCantBuyColor;
			this.ItemIcon.material = null;
		}
		else
		{
			this.ButtonText.text = "Add to Cart";
			this.ButtonText.color = this.ButtonTextCanBuyColor;
			this.ButtonImage.color = this.ButtonCanBuyColor;
			this.ItemIcon.material = null;
		}
		bool flag2 = this.ShopItem.IsNewlyUnlocked();
		this.NewIcon.SetActive(flag2);
		if (flag2)
		{
			if (this._colorTween == null)
			{
				this._colorTween = this.ItemNameText.DOColor(this.ItemNameNewlyUnlockedColor, 2.5f).SetLoops(-1, LoopType.Yoyo);
				return;
			}
		}
		else
		{
			Tween colorTween = this._colorTween;
			if (colorTween != null)
			{
				colorTween.Kill(false);
			}
			this.ItemNameText.color = this.ItemNameRegularColor;
		}
	}

	// Token: 0x04000712 RID: 1810
	public Button Button;

	// Token: 0x04000713 RID: 1811
	public Image ButtonImage;

	// Token: 0x04000714 RID: 1812
	public TMP_Text ButtonText;

	// Token: 0x04000715 RID: 1813
	public TMP_Text ItemNameText;

	// Token: 0x04000716 RID: 1814
	public KeybindTokenText ItemDescriptionText;

	// Token: 0x04000717 RID: 1815
	public TMP_Text ItemPriceText;

	// Token: 0x04000718 RID: 1816
	public Image ItemIcon;

	// Token: 0x04000719 RID: 1817
	public Color ButtonCanBuyColor;

	// Token: 0x0400071A RID: 1818
	public Color ButtonCantBuyColor;

	// Token: 0x0400071B RID: 1819
	public Color ButtonTextCanBuyColor;

	// Token: 0x0400071C RID: 1820
	public Color ButtonTextCantBuyColor;

	// Token: 0x0400071D RID: 1821
	public Color PriceCanBuyColor;

	// Token: 0x0400071E RID: 1822
	public Color PriceCantBuyColor;

	// Token: 0x0400071F RID: 1823
	public GameObject NewIcon;

	// Token: 0x04000720 RID: 1824
	public Color ItemNameRegularColor;

	// Token: 0x04000721 RID: 1825
	public Color ItemNameNewlyUnlockedColor;

	// Token: 0x04000723 RID: 1827
	private ComputerShopUI _shopUI;

	// Token: 0x04000724 RID: 1828
	private int _quantity = 1;

	// Token: 0x04000725 RID: 1829
	private Tween _colorTween;
}
