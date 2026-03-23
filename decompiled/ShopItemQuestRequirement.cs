using System;

[Serializable]
public class ShopItemQuestRequirement : QuestRequirement
{
	public ShopItemQuestRequirementType SavableItemQuestRequirementType;

	public ShopItemDefinition ShopItemDefinition;

	public override string GetRequirementText()
	{
		string text = "";
		text = ((SavableItemQuestRequirementType != ShopItemQuestRequirementType.PurchaseItemFromShop) ? "Invalid Savable Item Requirement" : ("Purchase " + ShopItemDefinition.GetName() + " from the Computer Shop"));
		return Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(text);
	}

	public override bool IsCompleted()
	{
		return Singleton<EconomyManager>.Instance.ShopPurchases.GetAmountPurchased(ShopItemDefinition) > 0;
	}

	public override QuestRequirement Clone()
	{
		return new ShopItemQuestRequirement
		{
			SavableItemQuestRequirementType = SavableItemQuestRequirementType,
			ShopItemDefinition = ShopItemDefinition,
			IsHidden = IsHidden,
			UnlocksHiddenQuest = UnlocksHiddenQuest
		};
	}
}
