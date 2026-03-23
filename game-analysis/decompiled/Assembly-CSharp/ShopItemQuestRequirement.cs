using System;

// Token: 0x0200008F RID: 143
[Serializable]
public class ShopItemQuestRequirement : QuestRequirement
{
	// Token: 0x060003ED RID: 1005 RVA: 0x000156C8 File Offset: 0x000138C8
	public override string GetRequirementText()
	{
		string text;
		if (this.SavableItemQuestRequirementType == ShopItemQuestRequirementType.PurchaseItemFromShop)
		{
			text = "Purchase " + this.ShopItemDefinition.GetName() + " from the Computer Shop";
		}
		else
		{
			text = "Invalid Savable Item Requirement";
		}
		return Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(text);
	}

	// Token: 0x060003EE RID: 1006 RVA: 0x00015712 File Offset: 0x00013912
	public override bool IsCompleted()
	{
		return Singleton<EconomyManager>.Instance.ShopPurchases.GetAmountPurchased(this.ShopItemDefinition) > 0;
	}

	// Token: 0x060003EF RID: 1007 RVA: 0x0001572C File Offset: 0x0001392C
	public override QuestRequirement Clone()
	{
		return new ShopItemQuestRequirement
		{
			SavableItemQuestRequirementType = this.SavableItemQuestRequirementType,
			ShopItemDefinition = this.ShopItemDefinition,
			IsHidden = this.IsHidden,
			UnlocksHiddenQuest = this.UnlocksHiddenQuest
		};
	}

	// Token: 0x04000440 RID: 1088
	public ShopItemQuestRequirementType SavableItemQuestRequirementType;

	// Token: 0x04000441 RID: 1089
	public ShopItemDefinition ShopItemDefinition;
}
