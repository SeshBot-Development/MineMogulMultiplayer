using System;
using UnityEngine;

public struct BuildingRotationInfo : IEquatable<BuildingRotationInfo>
{
	public Quaternion Rotation;

	public bool IsMirroredMode;

	public bool Equals(BuildingRotationInfo other)
	{
		if (Rotation.Equals(other.Rotation))
		{
			return IsMirroredMode == other.IsMirroredMode;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BuildingRotationInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 23 + Rotation.GetHashCode()) * 23 + IsMirroredMode.GetHashCode();
	}

	public static bool operator ==(BuildingRotationInfo left, BuildingRotationInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BuildingRotationInfo left, BuildingRotationInfo right)
	{
		return !(left == right);
	}
}
