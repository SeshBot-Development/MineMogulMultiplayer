using System;
using System.Collections.Generic;

[Serializable]
public class ShopPurchases
{
	public List<ShopObjectPurchaseEntry> Purchases = new List<ShopObjectPurchaseEntry>();

	public void AddPurchase(SavableObjectID objectID, int amount)
	{
		ShopObjectPurchaseEntry shopObjectPurchaseEntry = Purchases.Find((ShopObjectPurchaseEntry e) => e.SavableObjectID == objectID);
		if (shopObjectPurchaseEntry != null)
		{
			shopObjectPurchaseEntry.AmountPurchased += amount;
			return;
		}
		Purchases.Add(new ShopObjectPurchaseEntry
		{
			SavableObjectID = objectID,
			AmountPurchased = amount
		});
	}

	public int GetAmountPurchased(ShopItemDefinition shopItemDefinition)
	{
		return GetAmountPurchased(shopItemDefinition.GetSavableObjectID());
	}

	public int GetAmountPurchased(SavableObjectID objectID)
	{
		return Purchases.Find((ShopObjectPurchaseEntry e) => e.SavableObjectID == objectID)?.AmountPurchased ?? 0;
	}
}
