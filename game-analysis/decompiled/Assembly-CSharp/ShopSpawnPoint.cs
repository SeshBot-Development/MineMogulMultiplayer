using System;
using UnityEngine;

// Token: 0x020000DD RID: 221
public class ShopSpawnPoint : MonoBehaviour
{
	// Token: 0x060005F3 RID: 1523 RVA: 0x0001F25C File Offset: 0x0001D45C
	public static ShopSpawnPoint GetRandomItemSpawnPoint()
	{
		ShopSpawnPoint[] array = Object.FindObjectsOfType<ShopSpawnPoint>();
		int num = Random.Range(0, array.Length);
		return array[num];
	}
}
