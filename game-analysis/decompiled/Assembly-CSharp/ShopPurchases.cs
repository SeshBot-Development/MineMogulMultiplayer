using System;
using System.Collections.Generic;

// Token: 0x020000CA RID: 202
[Serializable]
public class ShopPurchases
{
	// Token: 0x06000557 RID: 1367 RVA: 0x0001C6D4 File Offset: 0x0001A8D4
	public void AddPurchase(SavableObjectID objectID, int amount)
	{
		ShopObjectPurchaseEntry shopObjectPurchaseEntry = this.Purchases.Find((ShopObjectPurchaseEntry e) => e.SavableObjectID == objectID);
		if (shopObjectPurchaseEntry != null)
		{
			shopObjectPurchaseEntry.AmountPurchased += amount;
			return;
		}
		this.Purchases.Add(new ShopObjectPurchaseEntry
		{
			SavableObjectID = objectID,
			AmountPurchased = amount
		});
	}

	// Token: 0x06000558 RID: 1368 RVA: 0x0001C73B File Offset: 0x0001A93B
	public int GetAmountPurchased(ShopItemDefinition shopItemDefinition)
	{
		return this.GetAmountPurchased(shopItemDefinition.GetSavableObjectID());
	}

	// Token: 0x06000559 RID: 1369 RVA: 0x0001C74C File Offset: 0x0001A94C
	public int GetAmountPurchased(SavableObjectID objectID)
	{
		ShopObjectPurchaseEntry shopObjectPurchaseEntry = this.Purchases.Find((ShopObjectPurchaseEntry e) => e.SavableObjectID == objectID);
		if (shopObjectPurchaseEntry == null)
		{
			return 0;
		}
		return shopObjectPurchaseEntry.AmountPurchased;
	}

	// Token: 0x040006A0 RID: 1696
	public List<ShopObjectPurchaseEntry> Purchases = new List<ShopObjectPurchaseEntry>();
}
