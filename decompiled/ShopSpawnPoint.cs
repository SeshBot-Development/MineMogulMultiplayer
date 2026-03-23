using UnityEngine;

public class ShopSpawnPoint : MonoBehaviour
{
	public static ShopSpawnPoint GetRandomItemSpawnPoint()
	{
		ShopSpawnPoint[] array = Object.FindObjectsOfType<ShopSpawnPoint>();
		int num = Random.Range(0, array.Length);
		return array[num];
	}
}
