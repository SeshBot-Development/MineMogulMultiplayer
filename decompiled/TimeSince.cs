using System;
using UnityEngine;

public struct TimeSince : IEquatable<TimeSince>
{
	private float time;

	public float Absolute => time;

	public float Relative => this;

	public static implicit operator float(TimeSince ts)
	{
		return Time.time - ts.time;
	}

	public static implicit operator TimeSince(float ts)
	{
		return new TimeSince
		{
			time = Time.time - ts
		};
	}

	public static bool operator <(in TimeSince ts, float f)
	{
		return ts.Relative < f;
	}

	public static bool operator >(in TimeSince ts, float f)
	{
		return ts.Relative > f;
	}

	public static bool operator <=(in TimeSince ts, float f)
	{
		return ts.Relative <= f;
	}

	public static bool operator >=(in TimeSince ts, float f)
	{
		return ts.Relative >= f;
	}

	public static bool operator <(in TimeSince ts, int f)
	{
		return ts.Relative < (float)f;
	}

	public static bool operator >(in TimeSince ts, int f)
	{
		return ts.Relative > (float)f;
	}

	public static bool operator <=(in TimeSince ts, int f)
	{
		return ts.Relative <= (float)f;
	}

	public static bool operator >=(in TimeSince ts, int f)
	{
		return ts.Relative >= (float)f;
	}

	public override string ToString()
	{
		return $"{Relative}";
	}

	public static bool operator ==(TimeSince left, TimeSince right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TimeSince left, TimeSince right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is TimeSince o)
		{
			return Equals(o);
		}
		return false;
	}

	public readonly bool Equals(TimeSince o)
	{
		return time == o.time;
	}

	public override readonly int GetHashCode()
	{
		return time.GetHashCode();
	}
}
