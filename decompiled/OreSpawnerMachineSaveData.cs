using System;

[Serializable]
public class OreSpawnerMachineSaveData
{
	public bool IsOn = true;

	public PieceType OrePieceType = PieceType.Ore;

	public ResourceType OreResourceType = ResourceType.Iron;

	public bool OreIsPolished;

	public float SpawnRate = 2f;
}
