using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularBuildingSupports : BaseModularSupports
{
	public SupportType SupportType;

	public GameObject TopSupportPrefab;

	public GameObject MiddleSupportPrefab;

	public GameObject BottomCapPrefab;

	public float SupportSpacing = 1f;

	public int MaxSupports = 15;

	public Vector3 RaycastOffset = new Vector3(0f, 0.4f, 0f);

	public GameObject BottomToRollerPrefab;

	public GameObject MiddleToRollerPrefab;

	public GameObject BottomToConveyorPrefab;

	public GameObject MiddleToConveyorPrefab;

	public GameObject BottomToChutePrefab;

	public GameObject MiddleToChutePrefab;

	public GameObject BottomToWalledPrefab;

	public GameObject MiddleToWalledPrefab;

	public Vector3 TopSupportOffset = new Vector3(0f, 0f, 0f);

	public Vector3 MiddleSupportOffset = new Vector3(0f, 0f, 0f);

	public Vector3 BottomCapOffset = new Vector3(0f, 0f, 0f);

	public Vector3 MinBottomCapRotation = new Vector3(-0.1f, -1f, -0.1f);

	public Vector3 MaxBottomCapRotation = new Vector3(0.1f, 1f, 0.1f);

	public Vector3 MinBottomCapScale = new Vector3(0.95f, 0.95f, 0.95f);

	public Vector3 MaxBottomCapScale = new Vector3(1.05f, 1.05f, 1.05f);

	public Vector3 RotationOffset = new Vector3(0f, 0f, 0f);

	private List<GameObject> spawnedSupports = new List<GameObject>();

	public override void SpawnSupports()
	{
		if ((_buildingObject != null && !_buildingObject.BuildingSupportsEnabled) || MaxSupports <= 0)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation * Quaternion.Euler(RotationOffset);
		if (!Physics.Raycast(position + base.transform.rotation * RaycastOffset, Vector3.down, out var hitInfo, SupportSpacing * (float)MaxSupports, Singleton<BuildingManager>.Instance.BuildingSupportsCollisionLayers))
		{
			return;
		}
		int num = Mathf.RoundToInt((hitInfo.distance - RaycastOffset.y) / SupportSpacing) + 1;
		int num2 = Mathf.Min(num, MaxSupports);
		bool flag = TopSupportPrefab != null;
		ModularBuildingSupports componentInParent = hitInfo.collider.GetComponentInParent<ModularBuildingSupports>();
		if (componentInParent != null)
		{
			switch (componentInParent.SupportType)
			{
			case SupportType.Roller:
			{
				num2--;
				rotation = componentInParent.transform.rotation;
				if (num2 > 0)
				{
					if (MiddleToRollerPrefab == null)
					{
						return;
					}
					GameObject item5 = Object.Instantiate(MiddleToRollerPrefab, position - new Vector3(0f, num2, 0f), rotation);
					spawnedSupports.Add(item5);
					break;
				}
				if (BottomToRollerPrefab == null)
				{
					return;
				}
				flag = false;
				GameObject item6 = Object.Instantiate(BottomToRollerPrefab, position, rotation);
				spawnedSupports.Add(item6);
				break;
			}
			case SupportType.Conveyor:
			{
				num2--;
				rotation = componentInParent.transform.rotation;
				if (num2 > 0)
				{
					if (MiddleToConveyorPrefab == null)
					{
						return;
					}
					GameObject item3 = Object.Instantiate(MiddleToConveyorPrefab, position - new Vector3(0f, num2, 0f), rotation);
					spawnedSupports.Add(item3);
					break;
				}
				if (BottomToConveyorPrefab == null)
				{
					return;
				}
				flag = false;
				GameObject item4 = Object.Instantiate(BottomToConveyorPrefab, position, rotation);
				spawnedSupports.Add(item4);
				break;
			}
			case SupportType.Chute:
			{
				num2--;
				rotation = componentInParent.transform.rotation;
				if (num2 > 0)
				{
					if (MiddleToChutePrefab == null)
					{
						return;
					}
					GameObject item7 = Object.Instantiate(MiddleToChutePrefab, position - new Vector3(0f, num2, 0f), rotation);
					spawnedSupports.Add(item7);
					break;
				}
				if (BottomToChutePrefab == null)
				{
					return;
				}
				flag = false;
				GameObject item8 = Object.Instantiate(BottomToChutePrefab, position, rotation);
				spawnedSupports.Add(item8);
				break;
			}
			case SupportType.Walled:
			{
				num2--;
				rotation = componentInParent.transform.rotation;
				if (num2 > 0)
				{
					if (MiddleToWalledPrefab == null)
					{
						return;
					}
					GameObject item = Object.Instantiate(MiddleToWalledPrefab, position - new Vector3(0f, num2, 0f), rotation);
					spawnedSupports.Add(item);
					break;
				}
				if (BottomToWalledPrefab == null)
				{
					return;
				}
				flag = false;
				GameObject item2 = Object.Instantiate(BottomToWalledPrefab, position, rotation);
				spawnedSupports.Add(item2);
				break;
			}
			case SupportType.None:
				return;
			default:
				InstantiateBottomCapPrefab(hitInfo.point, rotation);
				break;
			case SupportType.Flat:
				break;
			}
		}
		else
		{
			BuildingObject componentInParent2 = hitInfo.collider.GetComponentInParent<BuildingObject>();
			if (componentInParent2 != null && componentInParent2.SupportType != SupportType.Flat)
			{
				return;
			}
			if (Mathf.RoundToInt(hitInfo.distance / SupportSpacing) + 1 > num)
			{
				num2++;
			}
			InstantiateBottomCapPrefab(hitInfo.point, rotation);
		}
		num2--;
		if (flag)
		{
			GameObject item9 = Object.Instantiate(TopSupportPrefab, position + base.transform.rotation * TopSupportOffset, rotation);
			spawnedSupports.Add(item9);
		}
		for (int i = 0; i < num2; i++)
		{
			position.y -= SupportSpacing;
			GameObject item10 = Object.Instantiate(MiddleSupportPrefab, position + base.transform.rotation * MiddleSupportOffset, rotation);
			spawnedSupports.Add(item10);
		}
		foreach (GameObject spawnedSupport in spawnedSupports)
		{
			spawnedSupport.transform.parent = base.transform;
		}
	}

	private void InstantiateBottomCapPrefab(Vector3 position, Quaternion rotation)
	{
		if (!(BottomCapPrefab == null))
		{
			Vector3 euler = new Vector3(Random.Range(MinBottomCapRotation.x, MaxBottomCapRotation.x), Random.Range(MinBottomCapRotation.y, MaxBottomCapRotation.y), Random.Range(MinBottomCapRotation.z, MaxBottomCapRotation.z));
			Vector3 localScale = new Vector3(Random.Range(MinBottomCapScale.x, MaxBottomCapScale.x), Random.Range(MinBottomCapScale.y, MaxBottomCapScale.y), Random.Range(MinBottomCapScale.z, MaxBottomCapScale.z));
			GameObject gameObject = Object.Instantiate(BottomCapPrefab, position + base.transform.rotation * BottomCapOffset, Quaternion.Euler(euler) * rotation);
			gameObject.transform.localScale = localScale;
			spawnedSupports.Add(gameObject);
		}
	}

	private void OnDestroy()
	{
		foreach (GameObject spawnedSupport in spawnedSupports)
		{
			if (spawnedSupport != null)
			{
				Object.Destroy(spawnedSupport);
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
		if (this == null)
		{
			return;
		}
		foreach (GameObject spawnedSupport in spawnedSupports)
		{
			if (spawnedSupport != null)
			{
				Object.Destroy(spawnedSupport);
			}
		}
		spawnedSupports.Clear();
		SpawnSupports();
	}

	private IEnumerator DelayedRespawn()
	{
		yield return new WaitForFixedUpdate();
		RebuildSupports();
	}
}
