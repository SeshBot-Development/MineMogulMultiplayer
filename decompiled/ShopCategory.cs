using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

[Serializable]
public class ShopCategory
{
	public string CategoryName;

	[FormerlySerializedAs("_shopItemDefinitions")]
	public List<ShopItemDefinition> ShopItemDefinitions;

	public List<ShopItem> ShopItems;

	public bool DontShowIfAllItemsAreLocked;

	public bool IsSandboxOnlyCategory;

	public bool IsTrophyCategory;

	public HolidayType HolidayType;

	public bool IsAnyHolidayCategory()
	{
		return HolidayType != HolidayType.None;
	}

	public bool ContainsNewItems()
	{
		if (Singleton<GamemodeManager>.Instance.ShouldUseFreeShop())
		{
			return false;
		}
		return ShopItems.Any((ShopItem item) => item.IsNewlyUnlocked());
	}
}
