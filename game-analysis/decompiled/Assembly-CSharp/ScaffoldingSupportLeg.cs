using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000CD RID: 205
public class ScaffoldingSupportLeg : BaseModularSupports
{
	// Token: 0x06000561 RID: 1377 RVA: 0x0001C7DC File Offset: 0x0001A9DC
	public override void SpawnSupports()
	{
		foreach (object obj in base.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		if (this._buildingObject != null && !this._buildingObject.BuildingSupportsEnabled)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 20f, Singleton<BuildingManager>.Instance.ScaffoldingSupportsCollisionLayers))
		{
			int num = Mathf.RoundToInt(raycastHit.distance / this.SupportSpacing) + 1;
			Vector3 position = base.transform.position;
			for (int i = 0; i < num; i++)
			{
				Object.Instantiate<GameObject>(this.SupportPrefab, position, base.transform.rotation, base.transform);
				position.y -= this.SupportSpacing;
			}
		}
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x0001C8E4 File Offset: 0x0001AAE4
	public override void RespawnSupports(bool RespawnNextFrame = false)
	{
		if (RespawnNextFrame)
		{
			base.StartCoroutine(this.DelayedRespawn());
			return;
		}
		this.RebuildSupports();
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x0001C8FD File Offset: 0x0001AAFD
	private void RebuildSupports()
	{
		if (this == null)
		{
			return;
		}
		this.SpawnSupports();
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x0001C90F File Offset: 0x0001AB0F
	private IEnumerator DelayedRespawn()
	{
		yield return new WaitForFixedUpdate();
		this.RebuildSupports();
		yield break;
	}

	// Token: 0x040006A5 RID: 1701
	public GameObject SupportPrefab;

	// Token: 0x040006A6 RID: 1702
	public float SupportSpacing = 1f;

	// Token: 0x040006A7 RID: 1703
	public int MaxSupports = 15;
}
