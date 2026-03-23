using System;
using UnityEngine;

// Token: 0x0200000B RID: 11
public class BaseModularSupports : MonoBehaviour
{
	// Token: 0x06000053 RID: 83 RVA: 0x000031CC File Offset: 0x000013CC
	protected virtual void Start()
	{
		this._buildingObject = base.GetComponentInParent<BuildingObject>();
		this.SpawnSupports();
	}

	// Token: 0x06000054 RID: 84 RVA: 0x000031E0 File Offset: 0x000013E0
	public virtual void SpawnSupports()
	{
	}

	// Token: 0x06000055 RID: 85 RVA: 0x000031E2 File Offset: 0x000013E2
	public virtual void RespawnSupports(bool RespawnNextFrame = false)
	{
	}

	// Token: 0x0400004C RID: 76
	protected BuildingObject _buildingObject;
}
