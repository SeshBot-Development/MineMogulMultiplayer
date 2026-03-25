using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Building Inventory Definition", menuName = "Building/BuildingInventoryDefinition")]
public class BuildingInventoryDefinition : ScriptableObject
{
	public string Name = "Unknown Item";

	[FormerlySerializedAs("Icon")]
	public Sprite ProgrammerInventoryIcon;

	public Sprite InventoryIcon;

	[TextArea]
	public string Description = "Placeholder Description!";

	public string QButtonFunction = "Mirror";

	public int MaxInventoryStackSize = 1;

	public List<BuildingObject> BuildingPrefabs;

	public BuildingCrate PackedPrefab;

	public bool UseReverseRotationDirection;

	public bool CanBePlacedInTerrain;

	public BuildingObject GetMainPrefab()
	{
		return BuildingPrefabs.FirstOrDefault();
	}

	public Sprite GetIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(ProgrammerInventoryIcon != null))
			{
				return InventoryIcon;
			}
			return ProgrammerInventoryIcon;
		}
		if (!(InventoryIcon != null))
		{
			return ProgrammerInventoryIcon;
		}
		return InventoryIcon;
	}
}
