using System;
using UnityEngine;

[Serializable]
public class ShopItem
{
	public ShopItemDefinition Definition;

	public bool IsLocked;

	public ShopItem(ShopItemDefinition definition)
	{
		Definition = definition;
		if (Singleton<DemoManager>.Instance.IsDemoVersion && Definition.IsLockedInDemo)
		{
			IsLocked = true;
		}
		else
		{
			IsLocked = Definition.IsLockedByDefault;
		}
	}

	public int GetPrice()
	{
		return Definition.Price;
	}

	public string GetName()
	{
		return Definition.GetName();
	}

	public string GetDescription()
	{
		return Definition.GetDescription();
	}

	public int GetAmountPurchased()
	{
		return Singleton<EconomyManager>.Instance.ShopPurchases.GetAmountPurchased(GetSavableObjectID());
	}

	public bool IsNewlyUnlocked()
	{
		if (Singleton<GamemodeManager>.Instance.ShouldUseFreeShop())
		{
			return false;
		}
		if (!IsLocked && GetAmountPurchased() == 0)
		{
			return Definition.IsLockedByDefault;
		}
		return false;
	}

	public SavableObjectID GetSavableObjectID()
	{
		if (Definition == null)
		{
			Debug.Log("Couldn't find a SavableObjectID for shop item: " + GetName() + " because it's missing a definition");
			return SavableObjectID.INVALID;
		}
		return Definition.GetSavableObjectID();
	}
}
