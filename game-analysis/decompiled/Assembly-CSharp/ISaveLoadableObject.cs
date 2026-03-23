using System;
using UnityEngine;

// Token: 0x020000B2 RID: 178
public interface ISaveLoadableObject
{
	// Token: 0x060004E6 RID: 1254
	bool ShouldBeSaved();

	// Token: 0x060004E7 RID: 1255
	SavableObjectID GetSavableObjectID();

	// Token: 0x060004E8 RID: 1256
	Vector3 GetPosition();

	// Token: 0x060004E9 RID: 1257
	Vector3 GetRotation();

	// Token: 0x060004EA RID: 1258
	void LoadFromSave(string customDataJson);

	// Token: 0x060004EB RID: 1259
	string GetCustomSaveData();

	// Token: 0x1700001B RID: 27
	// (get) Token: 0x060004EC RID: 1260
	// (set) Token: 0x060004ED RID: 1261
	bool HasBeenSaved { get; set; }
}
