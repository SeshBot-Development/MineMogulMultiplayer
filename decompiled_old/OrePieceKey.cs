using System;

[Serializable]
public struct OrePieceKey : IEquatable<OrePieceKey>
{
	public ResourceType ResourceType;

	public PieceType PieceType;

	public bool IsPolished;

	public OrePieceKey(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		ResourceType = resourceType;
		PieceType = pieceType;
		IsPolished = isPolished;
	}

	public override int GetHashCode()
	{
		return ((17 * 31 + ResourceType.GetHashCode()) * 31 + PieceType.GetHashCode()) * 31 + IsPolished.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is OrePieceKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(OrePieceKey other)
	{
		if (ResourceType == other.ResourceType && PieceType == other.PieceType)
		{
			return IsPolished == other.IsPolished;
		}
		return false;
	}
}
