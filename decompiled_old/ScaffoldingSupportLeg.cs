using System.Collections;
using UnityEngine;

public class ScaffoldingSupportLeg : BaseModularSupports
{
	public GameObject SupportPrefab;

	public float SupportSpacing = 1f;

	public int MaxSupports = 15;

	public override void SpawnSupports()
	{
		foreach (Transform item in base.transform)
		{
			Object.Destroy(item.gameObject);
		}
		if ((!(_buildingObject != null) || _buildingObject.BuildingSupportsEnabled) && Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 20f, Singleton<BuildingManager>.Instance.ScaffoldingSupportsCollisionLayers))
		{
			int num = Mathf.RoundToInt(hitInfo.distance / SupportSpacing) + 1;
			Vector3 position = base.transform.position;
			for (int i = 0; i < num; i++)
			{
				Object.Instantiate(SupportPrefab, position, base.transform.rotation, base.transform);
				position.y -= SupportSpacing;
			}
		}
	}

	public override void RespawnSupports(bool RespawnNextFrame = false)
	{
		if (RespawnNextFrame)
		{
			StartCoroutine(DelayedRespawn());
		}
		else
		{
			RebuildSupports();
		}
	}

	private void RebuildSupports()
	{
		if (!(this == null))
		{
			SpawnSupports();
		}
	}

	private IEnumerator DelayedRespawn()
	{
		yield return new WaitForFixedUpdate();
		RebuildSupports();
	}
}
