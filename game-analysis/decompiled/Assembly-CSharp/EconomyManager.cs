using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000048 RID: 72
[DefaultExecutionOrder(-100)]
public class EconomyManager : global::Singleton<EconomyManager>
{
	// Token: 0x17000012 RID: 18
	// (get) Token: 0x060001E6 RID: 486 RVA: 0x00009FD7 File Offset: 0x000081D7
	// (set) Token: 0x060001E7 RID: 487 RVA: 0x00009FDF File Offset: 0x000081DF
	public HashSet<ShopItem> AllShopItems { get; private set; } = new HashSet<ShopItem>();

	// Token: 0x14000003 RID: 3
	// (add) Token: 0x060001E8 RID: 488 RVA: 0x00009FE8 File Offset: 0x000081E8
	// (remove) Token: 0x060001E9 RID: 489 RVA: 0x0000A020 File Offset: 0x00008220
	public event Action<ShopItem> ShopItemUnlocked;

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x060001EA RID: 490 RVA: 0x0000A058 File Offset: 0x00008258
	// (remove) Token: 0x060001EB RID: 491 RVA: 0x0000A090 File Offset: 0x00008290
	public event Action<float> OnMoneyUpdated;

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x060001EC RID: 492 RVA: 0x0000A0C8 File Offset: 0x000082C8
	// (remove) Token: 0x060001ED RID: 493 RVA: 0x0000A100 File Offset: 0x00008300
	public event Action ItemSold;

	// Token: 0x17000013 RID: 19
	// (get) Token: 0x060001EE RID: 494 RVA: 0x0000A135 File Offset: 0x00008335
	// (set) Token: 0x060001EF RID: 495 RVA: 0x0000A13D File Offset: 0x0000833D
	public float Money
	{
		get
		{
			return this._money;
		}
		private set
		{
			this._money = value;
			Action<float> onMoneyUpdated = this.OnMoneyUpdated;
			if (onMoneyUpdated == null)
			{
				return;
			}
			onMoneyUpdated(this._money);
		}
	}

	// Token: 0x060001F0 RID: 496 RVA: 0x0000A15C File Offset: 0x0000835C
	public static string GetFormattedMoneyString(float amount, bool includeDecimal)
	{
		if (!includeDecimal)
		{
			return string.Format("${0:#,##0.##}", amount);
		}
		return string.Format("${0:#,##0.00}", amount);
	}

	// Token: 0x060001F1 RID: 497 RVA: 0x0000A184 File Offset: 0x00008384
	public string GetColoredFormattedMoneyString(float amount, bool includeDecimal)
	{
		return string.Concat(new string[]
		{
			"<color=#",
			global::Singleton<UIManager>.Instance.MoneyTextColor.ToHexString(),
			">",
			EconomyManager.GetFormattedMoneyString(amount, includeDecimal),
			"</color>"
		});
	}

	// Token: 0x060001F2 RID: 498 RVA: 0x0000A1D0 File Offset: 0x000083D0
	public void DispatchOnItemSoldEvent()
	{
		Action itemSold = this.ItemSold;
		if (itemSold == null)
		{
			return;
		}
		itemSold();
	}

	// Token: 0x060001F3 RID: 499 RVA: 0x0000A1E2 File Offset: 0x000083E2
	public void AddMoney(float amount)
	{
		this.Money += amount;
	}

	// Token: 0x060001F4 RID: 500 RVA: 0x0000A1F2 File Offset: 0x000083F2
	public void SetMoney(float amount)
	{
		this.Money = amount;
	}

	// Token: 0x060001F5 RID: 501 RVA: 0x0000A1FB File Offset: 0x000083FB
	public bool CanAfford(float amount)
	{
		return this.Money >= amount;
	}

	// Token: 0x060001F6 RID: 502 RVA: 0x0000A20C File Offset: 0x0000840C
	public List<ShopCategory> GetAvailableShopCategories()
	{
		List<ShopCategory> list = new List<ShopCategory>();
		if (global::Singleton<SettingsManager>.Instance.AlwaysShowHolidayShopItems)
		{
			list.AddRange(this._allShopCategories);
		}
		else
		{
			list.AddRange(this._allShopCategories.Where((ShopCategory c) => !c.IsAnyHolidayCategory()));
		}
		if (global::Singleton<DebugManager>.Instance.DevModeEnabled || global::Singleton<DebugManager>.Instance.ShowDevTestShopItems)
		{
			list.AddRange(this._debugShopCategories);
		}
		return list;
	}

	// Token: 0x060001F7 RID: 503 RVA: 0x0000A290 File Offset: 0x00008490
	private void Start()
	{
		List<ShopCategory> list = new List<ShopCategory>(this._allShopCategories);
		list.AddRange(this._debugShopCategories);
		foreach (ShopCategory shopCategory in list)
		{
			foreach (ShopItemDefinition shopItemDefinition in shopCategory.ShopItemDefinitions)
			{
				if (this._allShopItemDefinitions.Contains(shopItemDefinition))
				{
					shopCategory.ShopItems.Add(this.GetShopItemFromDefinition(shopItemDefinition));
				}
				else
				{
					shopCategory.ShopItems.Add(new ShopItem(shopItemDefinition));
				}
			}
			this.AllShopItems.AddRange(shopCategory.ShopItems);
			this._allShopItemDefinitions.AddRange(shopCategory.ShopItemDefinitions);
		}
	}

	// Token: 0x060001F8 RID: 504 RVA: 0x0000A384 File Offset: 0x00008584
	public ShopItem GetShopItemFromSavableObjectID(SavableObjectID savableObjectID)
	{
		return this.AllShopItems.FirstOrDefault((ShopItem i) => i.GetSavableObjectID() == savableObjectID);
	}

	// Token: 0x060001F9 RID: 505 RVA: 0x0000A3B8 File Offset: 0x000085B8
	public ShopItem GetShopItemFromDefinition(ShopItemDefinition definition)
	{
		return this.AllShopItems.FirstOrDefault((ShopItem i) => i.Definition == definition);
	}

	// Token: 0x060001FA RID: 506 RVA: 0x0000A3EC File Offset: 0x000085EC
	public void UnlockShopItem(ShopItemDefinition definition)
	{
		ShopItem shopItem = this.AllShopItems.FirstOrDefault((ShopItem i) => i.Definition == definition);
		if (shopItem != null && shopItem.IsLocked)
		{
			shopItem.IsLocked = false;
			Action<ShopItem> shopItemUnlocked = this.ShopItemUnlocked;
			if (shopItemUnlocked == null)
			{
				return;
			}
			shopItemUnlocked(shopItem);
		}
	}

	// Token: 0x060001FB RID: 507 RVA: 0x0000A444 File Offset: 0x00008644
	public void UnlockAllShopItems()
	{
		foreach (ShopItem shopItem in this.AllShopItems)
		{
			shopItem.IsLocked = false;
		}
	}

	// Token: 0x060001FC RID: 508 RVA: 0x0000A498 File Offset: 0x00008698
	public float GetPriceOfBuildingDefinition(BuildingInventoryDefinition definition)
	{
		ShopItemDefinition shopItemDefinition = this._allShopItemDefinitions.FirstOrDefault((ShopItemDefinition s) => s.BuildingInventoryDefinition == definition);
		if (shopItemDefinition != null)
		{
			return (float)shopItemDefinition.Price;
		}
		Debug.Log("Trying to sell item, but couldn't find ShopItem for BuildingInventoryDefinition: " + definition.Name);
		return 10f;
	}

	// Token: 0x040001D6 RID: 470
	[FormerlySerializedAs("AllShopCategories")]
	[SerializeField]
	private List<ShopCategory> _allShopCategories;

	// Token: 0x040001D7 RID: 471
	[SerializeField]
	private List<ShopCategory> _debugShopCategories;

	// Token: 0x040001D9 RID: 473
	public ShopPurchases ShopPurchases = new ShopPurchases();

	// Token: 0x040001DA RID: 474
	private HashSet<ShopItemDefinition> _allShopItemDefinitions = new HashSet<ShopItemDefinition>();

	// Token: 0x040001DE RID: 478
	[SerializeField]
	private float _money;
}
