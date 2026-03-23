using System;

// Token: 0x020000C9 RID: 201
[Serializable]
public struct OrePieceKey : IEquatable<OrePieceKey>
{
	// Token: 0x06000553 RID: 1363 RVA: 0x0001C62B File Offset: 0x0001A82B
	public OrePieceKey(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		this.ResourceType = resourceType;
		this.PieceType = pieceType;
		this.IsPolished = isPolished;
	}

	// Token: 0x06000554 RID: 1364 RVA: 0x0001C642 File Offset: 0x0001A842
	public override int GetHashCode()
	{
		return ((17 * 31 + this.ResourceType.GetHashCode()) * 31 + this.PieceType.GetHashCode()) * 31 + this.IsPolished.GetHashCode();
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x0001C680 File Offset: 0x0001A880
	public override bool Equals(object obj)
	{
		if (obj is OrePieceKey)
		{
			OrePieceKey orePieceKey = (OrePieceKey)obj;
			return this.Equals(orePieceKey);
		}
		return false;
	}

	// Token: 0x06000556 RID: 1366 RVA: 0x0001C6A5 File Offset: 0x0001A8A5
	public bool Equals(OrePieceKey other)
	{
		return this.ResourceType == other.ResourceType && this.PieceType == other.PieceType && this.IsPolished == other.IsPolished;
	}

	// Token: 0x0400069D RID: 1693
	public ResourceType ResourceType;

	// Token: 0x0400069E RID: 1694
	public PieceType PieceType;

	// Token: 0x0400069F RID: 1695
	public bool IsPolished;
}
