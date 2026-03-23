using System;
using UnityEngine;

// Token: 0x020000FC RID: 252
public struct TimeSince : IEquatable<TimeSince>
{
	// Token: 0x060006AF RID: 1711 RVA: 0x00022CDC File Offset: 0x00020EDC
	public static implicit operator float(TimeSince ts)
	{
		return Time.time - ts.time;
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x00022CEC File Offset: 0x00020EEC
	public static implicit operator TimeSince(float ts)
	{
		return new TimeSince
		{
			time = Time.time - ts
		};
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x00022D10 File Offset: 0x00020F10
	public static bool operator <(in TimeSince ts, float f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative < f;
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x00022D30 File Offset: 0x00020F30
	public static bool operator >(in TimeSince ts, float f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative > f;
	}

	// Token: 0x060006B3 RID: 1715 RVA: 0x00022D50 File Offset: 0x00020F50
	public static bool operator <=(in TimeSince ts, float f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative <= f;
	}

	// Token: 0x060006B4 RID: 1716 RVA: 0x00022D74 File Offset: 0x00020F74
	public static bool operator >=(in TimeSince ts, float f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative >= f;
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x00022D98 File Offset: 0x00020F98
	public static bool operator <(in TimeSince ts, int f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative < (float)f;
	}

	// Token: 0x060006B6 RID: 1718 RVA: 0x00022DB8 File Offset: 0x00020FB8
	public static bool operator >(in TimeSince ts, int f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative > (float)f;
	}

	// Token: 0x060006B7 RID: 1719 RVA: 0x00022DD8 File Offset: 0x00020FD8
	public static bool operator <=(in TimeSince ts, int f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative <= (float)f;
	}

	// Token: 0x060006B8 RID: 1720 RVA: 0x00022DFC File Offset: 0x00020FFC
	public static bool operator >=(in TimeSince ts, int f)
	{
		TimeSince timeSince = ts;
		return timeSince.Relative >= (float)f;
	}

	// Token: 0x17000025 RID: 37
	// (get) Token: 0x060006B9 RID: 1721 RVA: 0x00022E1E File Offset: 0x0002101E
	public float Absolute
	{
		get
		{
			return this.time;
		}
	}

	// Token: 0x17000026 RID: 38
	// (get) Token: 0x060006BA RID: 1722 RVA: 0x00022E26 File Offset: 0x00021026
	public float Relative
	{
		get
		{
			return this;
		}
	}

	// Token: 0x060006BB RID: 1723 RVA: 0x00022E33 File Offset: 0x00021033
	public override string ToString()
	{
		return string.Format("{0}", this.Relative);
	}

	// Token: 0x060006BC RID: 1724 RVA: 0x00022E4A File Offset: 0x0002104A
	public static bool operator ==(TimeSince left, TimeSince right)
	{
		return left.Equals(right);
	}

	// Token: 0x060006BD RID: 1725 RVA: 0x00022E54 File Offset: 0x00021054
	public static bool operator !=(TimeSince left, TimeSince right)
	{
		return !(left == right);
	}

	// Token: 0x060006BE RID: 1726 RVA: 0x00022E60 File Offset: 0x00021060
	public override bool Equals(object obj)
	{
		if (obj is TimeSince)
		{
			TimeSince timeSince = (TimeSince)obj;
			return this.Equals(timeSince);
		}
		return false;
	}

	// Token: 0x060006BF RID: 1727 RVA: 0x00022E85 File Offset: 0x00021085
	public readonly bool Equals(TimeSince o)
	{
		return this.time == o.time;
	}

	// Token: 0x060006C0 RID: 1728 RVA: 0x00022E95 File Offset: 0x00021095
	public override readonly int GetHashCode()
	{
		return this.time.GetHashCode();
	}

	// Token: 0x040007E3 RID: 2019
	private float time;
}
