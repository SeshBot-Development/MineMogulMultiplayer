using System;
using UnityEngine;

// Token: 0x020000B5 RID: 181
public interface ISaveLoadableStaticBreakable
{
	// Token: 0x060004F0 RID: 1264
	Vector3 GetPosition();

	// Token: 0x060004F1 RID: 1265
	void MarkStaticPositionAsBroken();

	// Token: 0x060004F2 RID: 1266
	void DestroyFromLoading();
}
