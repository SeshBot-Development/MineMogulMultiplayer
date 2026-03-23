using System;
using UnityEngine;

// Token: 0x020000DB RID: 219
[CreateAssetMenu(fileName = "New ShopItem", menuName = "Shop/ShopItem")]
public class ShopItemDefinition : ScriptableObject
{
	// Token: 0x060005E7 RID: 1511 RVA: 0x0001F028 File Offset: 0x0001D228
	public string GetName()
	{
		if (this.UseNameAndDescriptionOfBuildingDefinition)
		{
			if (this.BuildingInventoryDefinition != null)
			{
				return this.BuildingInventoryDefinition.Name;
			}
			BaseHeldTool baseHeldTool;
			if (this.PrefabToSpawn != null && this.PrefabToSpawn.TryGetComponent<BaseHeldTool>(out baseHeldTool))
			{
				return baseHeldTool.Name;
			}
		}
		return this.Name;
	}

	// Token: 0x060005E8 RID: 1512 RVA: 0x0001F084 File Offset: 0x0001D284
	public string GetDescription()
	{
		if (this.UseNameAndDescriptionOfBuildingDefinition)
		{
			if (this.BuildingInventoryDefinition != null)
			{
				return this.BuildingInventoryDefinition.Description;
			}
			BaseHeldTool baseHeldTool;
			if (this.PrefabToSpawn != null && this.PrefabToSpawn.TryGetComponent<BaseHeldTool>(out baseHeldTool))
			{
				return baseHeldTool.Description;
			}
		}
		return this.Description;
	}

	// Token: 0x060005E9 RID: 1513 RVA: 0x0001F0E0 File Offset: 0x0001D2E0
	public Sprite GetIcon()
	{
		if (this.BuildingInventoryDefinition != null)
		{
			return this.BuildingInventoryDefinition.GetIcon();
		}
		if (this.PrefabToSpawn != null)
		{
			IIconItem component = this.PrefabToSpawn.GetComponent<IIconItem>();
			if (component != null)
			{
				return component.GetIcon();
			}
		}
		return null;
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x0001F12C File Offset: 0x0001D32C
	public SavableObjectID GetSavableObjectID()
	{
		if (this.BuildingInventoryDefinition != null)
		{
			BuildingObject mainPrefab = this.BuildingInventoryDefinition.GetMainPrefab();
			if (mainPrefab != null)
			{
				return mainPrefab.GetSavableObjectID();
			}
		}
		if (this.PrefabToSpawn != null)
		{
			ISaveLoadableObject component = this.PrefabToSpawn.GetComponent<ISaveLoadableObject>();
			if (component != null)
			{
				return component.GetSavableObjectID();
			}
		}
		Debug.Log("Couldn't find a SavableObjectID for shop item: " + this.GetName());
		return SavableObjectID.INVALID;
	}

	// Token: 0x04000726 RID: 1830
	public string Name;

	// Token: 0x04000727 RID: 1831
	[TextArea]
	public string Description;

	// Token: 0x04000728 RID: 1832
	public bool UseNameAndDescriptionOfBuildingDefinition;

	// Token: 0x04000729 RID: 1833
	public int Price;

	// Token: 0x0400072A RID: 1834
	public bool IsLockedByDefault;

	// Token: 0x0400072B RID: 1835
	public bool IsDummyItem;

	// Token: 0x0400072C RID: 1836
	public GameObject PrefabToSpawn;

	// Token: 0x0400072D RID: 1837
	public BuildingInventoryDefinition BuildingInventoryDefinition;
}
