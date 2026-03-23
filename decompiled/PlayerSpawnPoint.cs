using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
	public static PlayerSpawnPoint GetRandomPlayerSpawnPoint()
	{
		PlayerSpawnPoint[] array = Object.FindObjectsOfType<PlayerSpawnPoint>();
		int num = Random.Range(0, array.Length);
		return array[num];
	}
}
