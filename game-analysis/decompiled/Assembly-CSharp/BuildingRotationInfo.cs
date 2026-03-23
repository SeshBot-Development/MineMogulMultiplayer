using System;
using UnityEngine;

// Token: 0x02000018 RID: 24
public struct BuildingRotationInfo : IEquatable<BuildingRotationInfo>
{
	// Token: 0x060000BA RID: 186 RVA: 0x00005306 File Offset: 0x00003506
	public bool Equals(BuildingRotationInfo other)
	{
		return this.Rotation.Equals(other.Rotation) && this.IsMirroredMode == other.IsMirroredMode;
	}

	// Token: 0x060000BB RID: 187 RVA: 0x0000532C File Offset: 0x0000352C
	public override bool Equals(object obj)
	{
		if (obj is BuildingRotationInfo)
		{
			BuildingRotationInfo buildingRotationInfo = (BuildingRotationInfo)obj;
			return this.Equals(buildingRotationInfo);
		}
		return false;
	}

	// Token: 0x060000BC RID: 188 RVA: 0x00005351 File Offset: 0x00003551
	public override int GetHashCode()
	{
		return (17 * 23 + this.Rotation.GetHashCode()) * 23 + this.IsMirroredMode.GetHashCode();
	}

	// Token: 0x060000BD RID: 189 RVA: 0x00005379 File Offset: 0x00003579
	public static bool operator ==(BuildingRotationInfo left, BuildingRotationInfo right)
	{
		return left.Equals(right);
	}

	// Token: 0x060000BE RID: 190 RVA: 0x00005383 File Offset: 0x00003583
	public static bool operator !=(BuildingRotationInfo left, BuildingRotationInfo right)
	{
		return !(left == right);
	}

	// Token: 0x040000B5 RID: 181
	public Quaternion Rotation;

	// Token: 0x040000B6 RID: 182
	public bool IsMirroredMode;
}
