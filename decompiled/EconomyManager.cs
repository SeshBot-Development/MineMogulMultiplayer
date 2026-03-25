using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
public class EconomyManager : Singleton<EconomyManager>
{
	[FormerlySerializedAs("AllShopCategories")]
	[SerializeField]
	private List<ShopCategory> _allShopCategories;

	[SerializeField]
	private List<ShopCategory> _debugShopCategories;

	public ShopPurchases ShopPurchases = new ShopPurchases();

	private HashSet<ShopItemDefinition> _allShopItemDefinitions = new HashSet<ShopItemDefinition>();

	[SerializeField]
	private float _money;

	public HashSet<ShopItem> AllShopItems { get; private set; } = new HashSet<ShopItem>();

	public float Money
	{
		get
		{
			return _money;
		}
		private set
		{
			_money = value;
			this.OnMoneyUpdated?.Invoke(_money);
		}
	}

	public event Action<ShopItem> ShopItemUnlocked;

	public event Action<float> OnMoneyUpdated;

	public event Action ItemSold;

	public static string GetFormattedMoneyString(float amount, bool includeDecimal)
	{
		if (!includeDecimal)
		{
			return $"${amount:#,##0.##}";
		}
		return $"${amount:#,##0.00}";
	}

	public string GetColoredFormattedMoneyString(float amount, bool includeDecimal)
	{
		return "<color=#" + Singleton<UIManager>.Instance.MoneyTextColor.ToHexString() + ">" + GetFormattedMoneyString(amount, includeDecimal) + "</color>";
	}

	public void DispatchOnItemSoldEvent()
	{
		this.ItemSold?.Invoke();
	}

	public void AddMoney(float amount)
	{
		Money += amount;
	}

	public void SetMoney(float amount)
	{
		Money = amount;
	}

	public bool CanAfford(float amount)
	{
		return Money >= amount;
	}

	public List<ShopCategory> GetAvailableShopCategories()
	{
		List<ShopCategory> list = new List<ShopCategory>();
		bool flag = Singleton<DebugManager>.Instance.DevModeEnabled || Singleton<GamemodeManager>.Instance.ShouldShowSandboxShopTab();
		bool alwaysShowHolidayShopItems = Singleton<SettingsManager>.Instance.AlwaysShowHolidayShopItems;
		foreach (ShopCategory allShopCategory in _allShopCategories)
		{
			if ((alwaysShowHolidayShopItems || !allShopCategory.IsAnyHolidayCategory()) && (flag || !allShopCategory.IsSandboxOnlyCategory))
			{
				list.Add(allShopCategory);
			}
		}
		if (Singleton<DebugManager>.Instance.DevModeEnabled || Singleton<DebugManager>.Instance.ShowDevTestShopItems)
		{
			list.AddRange(_debugShopCategories);
		}
		return list;
	}

	private void Start()
	{
		List<ShopCategory> list = new List<ShopCategory>(_allShopCategories);
		list.AddRange(_debugShopCategories);
		foreach (ShopCategory item in list)
		{
			foreach (ShopItemDefinition shopItemDefinition in item.ShopItemDefinitions)
			{
				if (_allShopItemDefinitions.Contains(shopItemDefinition))
				{
					item.ShopItems.Add(GetShopItemFromDefinition(shopItemDefinition));
				}
				else
				{
					item.ShopItems.Add(new ShopItem(shopItemDefinition));
				}
			}
			AllShopItems.AddRange(item.ShopItems);
			_allShopItemDefinitions.AddRange(item.ShopItemDefinitions);
		}
	}

	public ShopItem GetShopItemFromSavableObjectID(SavableObjectID savableObjectID)
	{
		return AllShopItems.FirstOrDefault((ShopItem i) => i.GetSavableObjectID() == savableObjectID);
	}

	public ShopItem GetShopItemFromDefinition(ShopItemDefinition definition)
	{
		return AllShopItems.FirstOrDefault((ShopItem i) => i.Definition == definition);
	}

	public void UnlockShopItem(ShopItemDefinition definition)
	{
		ShopItem shopItem = AllShopItems.FirstOrDefault((ShopItem i) => i.Definition == definition);
		if (shopItem != null && (!shopItem.Definition.IsLockedInDemo || !Singleton<DemoManager>.Instance.IsDemoVersion) && shopItem.IsLocked)
		{
			shopItem.IsLocked = false;
			this.ShopItemUnlocked?.Invoke(shopItem);
		}
	}

	public void UnlockAllShopItems()
	{
		foreach (ShopItem allShopItem in AllShopItems)
		{
			if (!Singleton<DemoManager>.Instance.IsDemoVersion || !allShopItem.Definition.IsLockedInDemo)
			{
				allShopItem.IsLocked = false;
			}
		}
	}

	public float GetPriceOfBuildingDefinition(BuildingInventoryDefinition definition)
	{
		ShopItemDefinition shopItemDefinition = _allShopItemDefinitions.FirstOrDefault((ShopItemDefinition s) => s.BuildingInventoryDefinition == definition);
		if (shopItemDefinition != null)
		{
			return shopItemDefinition.Price;
		}
		Debug.Log("Trying to sell item, but couldn't find ShopItem for BuildingInventoryDefinition: " + definition.Name);
		return 10f;
	}
}
