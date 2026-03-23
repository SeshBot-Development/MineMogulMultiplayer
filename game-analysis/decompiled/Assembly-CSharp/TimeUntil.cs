using System;
using UnityEngine;

// Token: 0x020000FD RID: 253
public struct TimeUntil : IEquatable<TimeUntil>
{
	// Token: 0x060006C1 RID: 1729 RVA: 0x00022EA2 File Offset: 0x000210A2
	public static implicit operator bool(TimeUntil ts)
	{
		return Time.time >= ts.time;
	}

	// Token: 0x060006C2 RID: 1730 RVA: 0x00022EB4 File Offset: 0x000210B4
	public static implicit operator float(TimeUntil ts)
	{
		return ts.time - Time.time;
	}

	// Token: 0x060006C3 RID: 1731 RVA: 0x00022EC4 File Offset: 0x000210C4
	public static bool operator <(in TimeUntil ts, float f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative < f;
	}

	// Token: 0x060006C4 RID: 1732 RVA: 0x00022EE4 File Offset: 0x000210E4
	public static bool operator >(in TimeUntil ts, float f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative > f;
	}

	// Token: 0x060006C5 RID: 1733 RVA: 0x00022F04 File Offset: 0x00021104
	public static bool operator <=(in TimeUntil ts, float f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative <= f;
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x00022F28 File Offset: 0x00021128
	public static bool operator >=(in TimeUntil ts, float f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative >= f;
	}

	// Token: 0x060006C7 RID: 1735 RVA: 0x00022F4C File Offset: 0x0002114C
	public static bool operator <(in TimeUntil ts, int f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative < (float)f;
	}

	// Token: 0x060006C8 RID: 1736 RVA: 0x00022F6C File Offset: 0x0002116C
	public static bool operator >(in TimeUntil ts, int f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative > (float)f;
	}

	// Token: 0x060006C9 RID: 1737 RVA: 0x00022F8C File Offset: 0x0002118C
	public static bool operator <=(in TimeUntil ts, int f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative <= (float)f;
	}

	// Token: 0x060006CA RID: 1738 RVA: 0x00022FB0 File Offset: 0x000211B0
	public static bool operator >=(in TimeUntil ts, int f)
	{
		TimeUntil timeUntil = ts;
		return timeUntil.Relative >= (float)f;
	}

	// Token: 0x060006CB RID: 1739 RVA: 0x00022FD4 File Offset: 0x000211D4
	public static implicit operator TimeUntil(float ts)
	{
		return new TimeUntil
		{
			time = Time.time + ts,
			startTime = Time.time
		};
	}

	// Token: 0x17000027 RID: 39
	// (get) Token: 0x060006CC RID: 1740 RVA: 0x00023004 File Offset: 0x00021204
	public float Absolute
	{
		get
		{
			return this.time;
		}
	}

	// Token: 0x17000028 RID: 40
	// (get) Token: 0x060006CD RID: 1741 RVA: 0x0002300C File Offset: 0x0002120C
	public float Relative
	{
		get
		{
			return this;
		}
	}

	// Token: 0x17000029 RID: 41
	// (get) Token: 0x060006CE RID: 1742 RVA: 0x00023019 File Offset: 0x00021219
	public float Passed
	{
		get
		{
			return Time.time - this.startTime;
		}
	}

	// Token: 0x1700002A RID: 42
	// (get) Token: 0x060006CF RID: 1743 RVA: 0x00023027 File Offset: 0x00021227
	public float Fraction
	{
		get
		{
			return Math.Clamp((Time.time - this.startTime) / (this.time - this.startTime), 0f, 1f);
		}
	}

	// Token: 0x060006D0 RID: 1744 RVA: 0x00023052 File Offset: 0x00021252
	public override string ToString()
	{
		return string.Format("{0}", this.Relative);
	}

	// Token: 0x060006D1 RID: 1745 RVA: 0x00023069 File Offset: 0x00021269
	public static bool operator ==(TimeUntil left, TimeUntil right)
	{
		return left.Equals(right);
	}

	// Token: 0x060006D2 RID: 1746 RVA: 0x00023073 File Offset: 0x00021273
	public static bool operator !=(TimeUntil left, TimeUntil right)
	{
		return !(left == right);
	}

	// Token: 0x060006D3 RID: 1747 RVA: 0x00023080 File Offset: 0x00021280
	public override readonly bool Equals(object obj)
	{
		if (obj is TimeUntil)
		{
			TimeUntil timeUntil = (TimeUntil)obj;
			return this.Equals(timeUntil);
		}
		return false;
	}

	// Token: 0x060006D4 RID: 1748 RVA: 0x000230A5 File Offset: 0x000212A5
	public readonly bool Equals(TimeUntil o)
	{
		return this.time == o.time;
	}

	// Token: 0x060006D5 RID: 1749 RVA: 0x000230B5 File Offset: 0x000212B5
	public override readonly int GetHashCode()
	{
		return HashCode.Combine<float>(this.time);
	}

	// Token: 0x040007E4 RID: 2020
	private float time;

	// Token: 0x040007E5 RID: 2021
	private float startTime;
}
