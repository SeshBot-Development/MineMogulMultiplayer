using System;
using UnityEngine;

// Token: 0x020000C1 RID: 193
[Serializable]
public class SaveEntry
{
	// Token: 0x04000668 RID: 1640
	public SavableObjectID SavableObjectID;

	// Token: 0x04000669 RID: 1641
	public Vector3 Position;

	// Token: 0x0400066A RID: 1642
	public Vector3 Rotation;

	// Token: 0x0400066B RID: 1643
	public string CustomDataJson;
}
