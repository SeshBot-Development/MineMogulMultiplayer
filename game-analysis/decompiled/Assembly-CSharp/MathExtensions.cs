using System;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x0200006E RID: 110
public static class MathExtensions
{
	// Token: 0x060002F5 RID: 757 RVA: 0x0000E81D File Offset: 0x0000CA1D
	public static Vector3 RoundVector3(Vector3 v, int decimals = 2)
	{
		return new Vector3((float)Math.Round((double)v.x, decimals), (float)Math.Round((double)v.y, decimals), (float)Math.Round((double)v.z, decimals));
	}

	// Token: 0x060002F6 RID: 758 RVA: 0x0000E84E File Offset: 0x0000CA4E
	public static Vector3 TruncateVector3(Vector3 position)
	{
		return new Vector3(MathExtensions.<TruncateVector3>g__Truncate|1_0(position.x), MathExtensions.<TruncateVector3>g__Truncate|1_0(position.y), MathExtensions.<TruncateVector3>g__Truncate|1_0(position.z));
	}

	// Token: 0x060002F7 RID: 759 RVA: 0x0000E876 File Offset: 0x0000CA76
	[CompilerGenerated]
	internal static float <TruncateVector3>g__Truncate|1_0(float value)
	{
		return Mathf.Floor(value * 100f) / 100f;
	}
}
