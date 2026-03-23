using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000015 RID: 21
[CreateAssetMenu(fileName = "New Building Inventory Definition", menuName = "Building/BuildingInventoryDefinition")]
public class BuildingInventoryDefinition : ScriptableObject
{
	// Token: 0x060000A8 RID: 168 RVA: 0x0000454F File Offset: 0x0000274F
	public BuildingObject GetMainPrefab()
	{
		return this.BuildingPrefabs.FirstOrDefault<BuildingObject>();
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x0000455C File Offset: 0x0000275C
	public Sprite GetIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(this.ProgrammerInventoryIcon != null))
			{
				return this.InventoryIcon;
			}
			return this.ProgrammerInventoryIcon;
		}
		else
		{
			if (!(this.InventoryIcon != null))
			{
				return this.ProgrammerInventoryIcon;
			}
			return this.InventoryIcon;
		}
	}

	// Token: 0x0400008F RID: 143
	public string Name = "Unknown Item";

	// Token: 0x04000090 RID: 144
	[FormerlySerializedAs("Icon")]
	public Sprite ProgrammerInventoryIcon;

	// Token: 0x04000091 RID: 145
	public Sprite InventoryIcon;

	// Token: 0x04000092 RID: 146
	[TextArea]
	public string Description = "Placeholder Description!";

	// Token: 0x04000093 RID: 147
	public string QButtonFunction = "Mirror";

	// Token: 0x04000094 RID: 148
	public int MaxInventoryStackSize = 1;

	// Token: 0x04000095 RID: 149
	public List<BuildingObject> BuildingPrefabs;

	// Token: 0x04000096 RID: 150
	public BuildingCrate PackedPrefab;

	// Token: 0x04000097 RID: 151
	public bool UseReverseRotationDirection;

	// Token: 0x04000098 RID: 152
	public bool CanBePlacedInTerrain;
}
