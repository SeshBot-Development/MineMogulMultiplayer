using System;

// Token: 0x020000B3 RID: 179
public interface ISaveLoadableBuildingObject : ISaveLoadableObject
{
	// Token: 0x060004EE RID: 1262
	bool GetBuildingSupportsEnabled();

	// Token: 0x060004EF RID: 1263
	void LoadBuildingSaveData(BuildingObjectEntry buildingObjectEntry);
}
