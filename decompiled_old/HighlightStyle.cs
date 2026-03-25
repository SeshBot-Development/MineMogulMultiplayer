using System;
using UnityEngine;

[Serializable]
public struct HighlightStyle : IEquatable<HighlightStyle>
{
	public Color Color;

	[Range(0.5f, 8f)]
	public float RimPower;

	[Range(0f, 3f)]
	public float Intensity;

	public bool XrayThroughWalls;

	public static readonly HighlightStyle Default = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, xray: false);

	public HighlightStyle(Color color, float power, float intensity, bool xray)
	{
		Color = color;
		RimPower = power;
		Intensity = intensity;
		XrayThroughWalls = xray;
	}

	public bool Equals(HighlightStyle other)
	{
		if (Color.Equals(other.Color) && RimPower.Equals(other.RimPower) && Intensity.Equals(other.Intensity))
		{
			return XrayThroughWalls == other.XrayThroughWalls;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is HighlightStyle other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((Color.GetHashCode() * 397) ^ RimPower.GetHashCode()) * 397) ^ Intensity.GetHashCode()) * 397) ^ XrayThroughWalls.GetHashCode();
	}
}
