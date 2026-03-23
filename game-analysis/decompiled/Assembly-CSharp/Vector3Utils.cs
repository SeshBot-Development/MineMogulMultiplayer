using System;
using UnityEngine;

// Token: 0x020000FF RID: 255
public static class Vector3Utils
{
	// Token: 0x060006DE RID: 1758 RVA: 0x0002317D File Offset: 0x0002137D
	public static bool IsValid(Vector3 v)
	{
		return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
	}
}
