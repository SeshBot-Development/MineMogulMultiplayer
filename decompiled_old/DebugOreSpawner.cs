using UnityEngine;

public class DebugOreSpawner : MonoBehaviour
{
	public GameObject PrefabToSpawn;

	private void Update()
	{
		if (Singleton<DebugManager>.Instance.DevModeEnabled && Input.GetKeyDown(KeyCode.I))
		{
			OrePiece component = PrefabToSpawn.GetComponent<OrePiece>();
			if (component != null)
			{
				Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(component, base.transform.position, Quaternion.identity, base.transform);
			}
			else
			{
				Object.Instantiate(PrefabToSpawn, base.transform);
			}
		}
	}
}
