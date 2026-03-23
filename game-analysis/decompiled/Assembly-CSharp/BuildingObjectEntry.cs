using System;
using UnityEngine;

// Token: 0x020000C2 RID: 194
[Serializable]
public class BuildingObjectEntry
{
	// Token: 0x0400066C RID: 1644
	public SavableObjectID SavableObjectID;

	// Token: 0x0400066D RID: 1645
	public Vector3 Position;

	// Token: 0x0400066E RID: 1646
	public Vector3 Rotation;

	// Token: 0x0400066F RID: 1647
	public string CustomDataJson;

	// Token: 0x04000670 RID: 1648
	public bool BuildingSupportsEnable = true;
}
