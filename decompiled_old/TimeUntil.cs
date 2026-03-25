using System;
using UnityEngine;

public struct TimeUntil : IEquatable<TimeUntil>
{
	private float time;

	private float startTime;

	public float Absolute => time;

	public float Relative => this;

	public float Passed => Time.time - startTime;

	public float Fraction => Math.Clamp((Time.time - startTime) / (time - startTime), 0f, 1f);

	public static implicit operator bool(TimeUntil ts)
	{
		return Time.time >= ts.time;
	}

	public static implicit operator float(TimeUntil ts)
	{
		return ts.time - Time.time;
	}

	public static bool operator <(in TimeUntil ts, float f)
	{
		return ts.Relative < f;
	}

	public static bool operator >(in TimeUntil ts, float f)
	{
		return ts.Relative > f;
	}

	public static bool operator <=(in TimeUntil ts, float f)
	{
		return ts.Relative <= f;
	}

	public static bool operator >=(in TimeUntil ts, float f)
	{
		return ts.Relative >= f;
	}

	public static bool operator <(in TimeUntil ts, int f)
	{
		return ts.Relative < (float)f;
	}

	public static bool operator >(in TimeUntil ts, int f)
	{
		return ts.Relative > (float)f;
	}

	public static bool operator <=(in TimeUntil ts, int f)
	{
		return ts.Relative <= (float)f;
	}

	public static bool operator >=(in TimeUntil ts, int f)
	{
		return ts.Relative >= (float)f;
	}

	public static implicit operator TimeUntil(float ts)
	{
		return new TimeUntil
		{
			time = Time.time + ts,
			startTime = Time.time
		};
	}

	public override string ToString()
	{
		return $"{Relative}";
	}

	public static bool operator ==(TimeUntil left, TimeUntil right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TimeUntil left, TimeUntil right)
	{
		return !(left == right);
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is TimeUntil o)
		{
			return Equals(o);
		}
		return false;
	}

	public readonly bool Equals(TimeUntil o)
	{
		return time == o.time;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(time);
	}
}
