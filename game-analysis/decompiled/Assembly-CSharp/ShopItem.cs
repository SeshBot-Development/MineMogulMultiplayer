using System;
using UnityEngine;

// Token: 0x020000DC RID: 220
[Serializable]
public class ShopItem
{
	// Token: 0x060005EC RID: 1516 RVA: 0x0001F1A5 File Offset: 0x0001D3A5
	public ShopItem(ShopItemDefinition definition)
	{
		this.Definition = definition;
		this.IsLocked = this.Definition.IsLockedByDefault;
	}

	// Token: 0x060005ED RID: 1517 RVA: 0x0001F1C5 File Offset: 0x0001D3C5
	public int GetPrice()
	{
		return this.Definition.Price;
	}

	// Token: 0x060005EE RID: 1518 RVA: 0x0001F1D2 File Offset: 0x0001D3D2
	public string GetName()
	{
		return this.Definition.GetName();
	}

	// Token: 0x060005EF RID: 1519 RVA: 0x0001F1DF File Offset: 0x0001D3DF
	public string GetDescription()
	{
		return this.Definition.GetDescription();
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x0001F1EC File Offset: 0x0001D3EC
	public int GetAmountPurchased()
	{
		return Singleton<EconomyManager>.Instance.ShopPurchases.GetAmountPurchased(this.GetSavableObjectID());
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x0001F203 File Offset: 0x0001D403
	public bool IsNewlyUnlocked()
	{
		return !this.IsLocked && this.GetAmountPurchased() == 0 && this.Definition.IsLockedByDefault;
	}

	// Token: 0x060005F2 RID: 1522 RVA: 0x0001F222 File Offset: 0x0001D422
	public SavableObjectID GetSavableObjectID()
	{
		if (this.Definition == null)
		{
			Debug.Log("Couldn't find a SavableObjectID for shop item: " + this.GetName() + " because it's missing a definition");
			return SavableObjectID.INVALID;
		}
		return this.Definition.GetSavableObjectID();
	}

	// Token: 0x0400072E RID: 1838
	public ShopItemDefinition Definition;

	// Token: 0x0400072F RID: 1839
	public bool IsLocked;
}
