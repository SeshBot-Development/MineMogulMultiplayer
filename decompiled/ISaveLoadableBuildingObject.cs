public interface ISaveLoadableBuildingObject : ISaveLoadableObject
{
	bool GetBuildingSupportsEnabled();

	void LoadBuildingSaveData(BuildingObjectEntry buildingObjectEntry);
}
