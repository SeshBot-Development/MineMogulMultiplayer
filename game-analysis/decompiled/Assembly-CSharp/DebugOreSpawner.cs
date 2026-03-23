using System;
using UnityEngine;

// Token: 0x0200003F RID: 63
public class DebugOreSpawner : MonoBehaviour
{
	// Token: 0x060001B1 RID: 433 RVA: 0x00009384 File Offset: 0x00007584
	private void Update()
	{
		if (!Singleton<DebugManager>.Instance.DevModeEnabled)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.I))
		{
			OrePiece component = this.PrefabToSpawn.GetComponent<OrePiece>();
			if (component != null)
			{
				Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(component, base.transform.position, Quaternion.identity, base.transform);
				return;
			}
			Object.Instantiate<GameObject>(this.PrefabToSpawn, base.transform);
		}
	}

	// Token: 0x0400019A RID: 410
	public GameObject PrefabToSpawn;
}
