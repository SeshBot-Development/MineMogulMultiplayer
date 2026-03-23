using System;
using UnityEngine;

// Token: 0x02000088 RID: 136
public class PlayerSpawnPoint : MonoBehaviour
{
	// Token: 0x060003C7 RID: 967 RVA: 0x00014980 File Offset: 0x00012B80
	public static PlayerSpawnPoint GetRandomPlayerSpawnPoint()
	{
		PlayerSpawnPoint[] array = Object.FindObjectsOfType<PlayerSpawnPoint>();
		int num = Random.Range(0, array.Length);
		return array[num];
	}
}
