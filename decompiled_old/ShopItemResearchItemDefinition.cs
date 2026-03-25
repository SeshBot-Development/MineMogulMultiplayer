using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ShopItemResearchItem", menuName = "Research/ShopItemResearchItem")]
public class ShopItemResearchItemDefinition : ResearchItemDefinition
{
	[SerializeField]
	private string _overrideDisplayName;

	public List<ShopItemDefinition> ShopItemDefinitions;

	public override void OnResearched()
	{
		foreach (ShopItemDefinition shopItemDefinition in ShopItemDefinitions)
		{
			Singleton<EconomyManager>.Instance.UnlockShopItem(shopItemDefinition);
		}
	}

	public override Sprite GetIcon()
	{
		return ShopItemDefinitions[0].GetIcon();
	}

	public override string GetName()
	{
		if (string.IsNullOrEmpty(_overrideDisplayName))
		{
			return ShopItemDefinitions[0].GetName();
		}
		return _overrideDisplayName;
	}

	public override string GetDescription()
	{
		return ShopItemDefinitions[0].GetDescription();
	}

	public override SavableObjectID GetSavableObjectID()
	{
		if (ShopItemDefinitions[0] != null)
		{
			ShopItem shopItemFromDefinition = Singleton<EconomyManager>.Instance.GetShopItemFromDefinition(ShopItemDefinitions[0]);
			if (shopItemFromDefinition != null)
			{
				return shopItemFromDefinition.GetSavableObjectID();
			}
		}
		return base.GetSavableObjectID();
	}
}
