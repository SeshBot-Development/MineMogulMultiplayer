using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000A7 RID: 167
[CreateAssetMenu(fileName = "New ShopItemResearchItem", menuName = "Research/ShopItemResearchItem")]
public class ShopItemResearchItemDefinition : ResearchItemDefinition
{
	// Token: 0x060004A4 RID: 1188 RVA: 0x00019248 File Offset: 0x00017448
	public override void OnResearched()
	{
		foreach (ShopItemDefinition shopItemDefinition in this.ShopItemDefinitions)
		{
			Singleton<EconomyManager>.Instance.UnlockShopItem(shopItemDefinition);
		}
	}

	// Token: 0x060004A5 RID: 1189 RVA: 0x000192A0 File Offset: 0x000174A0
	public override Sprite GetIcon()
	{
		return this.ShopItemDefinitions[0].GetIcon();
	}

	// Token: 0x060004A6 RID: 1190 RVA: 0x000192B3 File Offset: 0x000174B3
	public override string GetName()
	{
		if (string.IsNullOrEmpty(this._overrideDisplayName))
		{
			return this.ShopItemDefinitions[0].GetName();
		}
		return this._overrideDisplayName;
	}

	// Token: 0x060004A7 RID: 1191 RVA: 0x000192DA File Offset: 0x000174DA
	public override string GetDescription()
	{
		return this.ShopItemDefinitions[0].GetDescription();
	}

	// Token: 0x060004A8 RID: 1192 RVA: 0x000192F0 File Offset: 0x000174F0
	public override SavableObjectID GetSavableObjectID()
	{
		if (this.ShopItemDefinitions[0] != null)
		{
			ShopItem shopItemFromDefinition = Singleton<EconomyManager>.Instance.GetShopItemFromDefinition(this.ShopItemDefinitions[0]);
			if (shopItemFromDefinition != null)
			{
				return shopItemFromDefinition.GetSavableObjectID();
			}
		}
		return base.GetSavableObjectID();
	}

	// Token: 0x04000535 RID: 1333
	[SerializeField]
	private string _overrideDisplayName;

	// Token: 0x04000536 RID: 1334
	public List<ShopItemDefinition> ShopItemDefinitions;
}
