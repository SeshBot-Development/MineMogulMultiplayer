using UnityEngine;

[CreateAssetMenu(fileName = "New ShopItem", menuName = "Shop/ShopItem")]
public class ShopItemDefinition : ScriptableObject
{
	public string Name;

	[TextArea]
	public string Description;

	public bool UseNameAndDescriptionOfBuildingDefinition;

	public int Price;

	public bool IsLockedByDefault;

	public bool IsLockedInDemo = true;

	public bool IsDummyItem;

	public GameObject PrefabToSpawn;

	public BuildingInventoryDefinition BuildingInventoryDefinition;

	public string GetName()
	{
		if (UseNameAndDescriptionOfBuildingDefinition)
		{
			if (BuildingInventoryDefinition != null)
			{
				return BuildingInventoryDefinition.Name;
			}
			if (PrefabToSpawn != null && PrefabToSpawn.TryGetComponent<BaseHeldTool>(out var component))
			{
				return component.Name;
			}
		}
		return Name;
	}

	public string GetDescription()
	{
		if (UseNameAndDescriptionOfBuildingDefinition)
		{
			if (BuildingInventoryDefinition != null)
			{
				return BuildingInventoryDefinition.Description;
			}
			if (PrefabToSpawn != null && PrefabToSpawn.TryGetComponent<BaseHeldTool>(out var component))
			{
				return component.Description;
			}
		}
		return Description;
	}

	public Sprite GetIcon()
	{
		if (BuildingInventoryDefinition != null)
		{
			return BuildingInventoryDefinition.GetIcon();
		}
		if (PrefabToSpawn != null)
		{
			IIconItem component = PrefabToSpawn.GetComponent<IIconItem>();
			if (component != null)
			{
				return component.GetIcon();
			}
		}
		return null;
	}

	public SavableObjectID GetSavableObjectID()
	{
		if (BuildingInventoryDefinition != null)
		{
			BuildingObject mainPrefab = BuildingInventoryDefinition.GetMainPrefab();
			if (mainPrefab != null)
			{
				return mainPrefab.GetSavableObjectID();
			}
		}
		if (PrefabToSpawn != null)
		{
			ISaveLoadableObject component = PrefabToSpawn.GetComponent<ISaveLoadableObject>();
			if (component != null)
			{
				return component.GetSavableObjectID();
			}
		}
		Debug.Log("Couldn't find a SavableObjectID for shop item: " + GetName());
		return SavableObjectID.INVALID;
	}
}
