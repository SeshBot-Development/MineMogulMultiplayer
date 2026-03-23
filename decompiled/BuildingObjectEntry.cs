using System;
using UnityEngine;

[Serializable]
public class BuildingObjectEntry
{
	public SavableObjectID SavableObjectID;

	public Vector3 Position;

	public Vector3 Rotation;

	public string CustomDataJson;

	public bool BuildingSupportsEnable = true;
}
