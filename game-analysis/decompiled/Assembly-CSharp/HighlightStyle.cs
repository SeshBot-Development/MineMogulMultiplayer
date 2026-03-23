using System;
using UnityEngine;

// Token: 0x02000051 RID: 81
[Serializable]
public struct HighlightStyle : IEquatable<HighlightStyle>
{
	// Token: 0x0600021F RID: 543 RVA: 0x0000AD58 File Offset: 0x00008F58
	public HighlightStyle(Color color, float power, float intensity, bool xray)
	{
		this.Color = color;
		this.RimPower = power;
		this.Intensity = intensity;
		this.XrayThroughWalls = xray;
	}

	// Token: 0x06000220 RID: 544 RVA: 0x0000AD78 File Offset: 0x00008F78
	public bool Equals(HighlightStyle other)
	{
		return this.Color.Equals(other.Color) && this.RimPower.Equals(other.RimPower) && this.Intensity.Equals(other.Intensity) && this.XrayThroughWalls == other.XrayThroughWalls;
	}

	// Token: 0x06000221 RID: 545 RVA: 0x0000ADD0 File Offset: 0x00008FD0
	public override bool Equals(object obj)
	{
		if (obj is HighlightStyle)
		{
			HighlightStyle highlightStyle = (HighlightStyle)obj;
			return this.Equals(highlightStyle);
		}
		return false;
	}

	// Token: 0x06000222 RID: 546 RVA: 0x0000ADF8 File Offset: 0x00008FF8
	public override int GetHashCode()
	{
		return (((((this.Color.GetHashCode() * 397) ^ this.RimPower.GetHashCode()) * 397) ^ this.Intensity.GetHashCode()) * 397) ^ this.XrayThroughWalls.GetHashCode();
	}

	// Token: 0x04000202 RID: 514
	public Color Color;

	// Token: 0x04000203 RID: 515
	[Range(0.5f, 8f)]
	public float RimPower;

	// Token: 0x04000204 RID: 516
	[Range(0f, 3f)]
	public float Intensity;

	// Token: 0x04000205 RID: 517
	public bool XrayThroughWalls;

	// Token: 0x04000206 RID: 518
	public static readonly HighlightStyle Default = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, false);
}
