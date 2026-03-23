using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200006F RID: 111
public class ModularBuildingSupports : BaseModularSupports
{
	// Token: 0x060002F8 RID: 760 RVA: 0x0000E88C File Offset: 0x0000CA8C
	public override void SpawnSupports()
	{
		if (this._buildingObject != null && !this._buildingObject.BuildingSupportsEnabled)
		{
			return;
		}
		if (this.MaxSupports <= 0)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Quaternion quaternion = base.transform.rotation * Quaternion.Euler(this.RotationOffset);
		RaycastHit raycastHit;
		if (Physics.Raycast(position + base.transform.rotation * this.RaycastOffset, Vector3.down, out raycastHit, this.SupportSpacing * (float)this.MaxSupports, Singleton<BuildingManager>.Instance.BuildingSupportsCollisionLayers))
		{
			int num = Mathf.RoundToInt((raycastHit.distance - this.RaycastOffset.y) / this.SupportSpacing) + 1;
			int num2 = Mathf.Min(num, this.MaxSupports);
			bool flag = this.TopSupportPrefab != null;
			ModularBuildingSupports componentInParent = raycastHit.collider.GetComponentInParent<ModularBuildingSupports>();
			if (componentInParent != null)
			{
				switch (componentInParent.SupportType)
				{
				case SupportType.None:
					return;
				case SupportType.Conveyor:
					num2--;
					quaternion = componentInParent.transform.rotation;
					if (num2 > 0)
					{
						if (this.MiddleToConveyorPrefab == null)
						{
							return;
						}
						GameObject gameObject = Object.Instantiate<GameObject>(this.MiddleToConveyorPrefab, position - new Vector3(0f, (float)num2, 0f), quaternion);
						this.spawnedSupports.Add(gameObject);
					}
					else
					{
						if (this.BottomToConveyorPrefab == null)
						{
							return;
						}
						flag = false;
						GameObject gameObject2 = Object.Instantiate<GameObject>(this.BottomToConveyorPrefab, position, quaternion);
						this.spawnedSupports.Add(gameObject2);
					}
					break;
				case SupportType.Roller:
					num2--;
					quaternion = componentInParent.transform.rotation;
					if (num2 > 0)
					{
						if (this.MiddleToRollerPrefab == null)
						{
							return;
						}
						GameObject gameObject3 = Object.Instantiate<GameObject>(this.MiddleToRollerPrefab, position - new Vector3(0f, (float)num2, 0f), quaternion);
						this.spawnedSupports.Add(gameObject3);
					}
					else
					{
						if (this.BottomToRollerPrefab == null)
						{
							return;
						}
						flag = false;
						GameObject gameObject4 = Object.Instantiate<GameObject>(this.BottomToRollerPrefab, position, quaternion);
						this.spawnedSupports.Add(gameObject4);
					}
					break;
				case SupportType.Flat:
					break;
				case SupportType.Chute:
					num2--;
					quaternion = componentInParent.transform.rotation;
					if (num2 > 0)
					{
						if (this.MiddleToChutePrefab == null)
						{
							return;
						}
						GameObject gameObject5 = Object.Instantiate<GameObject>(this.MiddleToChutePrefab, position - new Vector3(0f, (float)num2, 0f), quaternion);
						this.spawnedSupports.Add(gameObject5);
					}
					else
					{
						if (this.BottomToChutePrefab == null)
						{
							return;
						}
						flag = false;
						GameObject gameObject6 = Object.Instantiate<GameObject>(this.BottomToChutePrefab, position, quaternion);
						this.spawnedSupports.Add(gameObject6);
					}
					break;
				case SupportType.Walled:
					num2--;
					quaternion = componentInParent.transform.rotation;
					if (num2 > 0)
					{
						if (this.MiddleToWalledPrefab == null)
						{
							return;
						}
						GameObject gameObject7 = Object.Instantiate<GameObject>(this.MiddleToWalledPrefab, position - new Vector3(0f, (float)num2, 0f), quaternion);
						this.spawnedSupports.Add(gameObject7);
					}
					else
					{
						if (this.BottomToWalledPrefab == null)
						{
							return;
						}
						flag = false;
						GameObject gameObject8 = Object.Instantiate<GameObject>(this.BottomToWalledPrefab, position, quaternion);
						this.spawnedSupports.Add(gameObject8);
					}
					break;
				default:
					this.InstantiateBottomCapPrefab(raycastHit.point, quaternion);
					break;
				}
			}
			else
			{
				BuildingObject componentInParent2 = raycastHit.collider.GetComponentInParent<BuildingObject>();
				if (componentInParent2 != null && componentInParent2.SupportType != SupportType.Flat)
				{
					return;
				}
				if (Mathf.RoundToInt(raycastHit.distance / this.SupportSpacing) + 1 > num)
				{
					num2++;
				}
				this.InstantiateBottomCapPrefab(raycastHit.point, quaternion);
			}
			num2--;
			if (flag)
			{
				GameObject gameObject9 = Object.Instantiate<GameObject>(this.TopSupportPrefab, position + base.transform.rotation * this.TopSupportOffset, quaternion);
				this.spawnedSupports.Add(gameObject9);
			}
			for (int i = 0; i < num2; i++)
			{
				position.y -= this.SupportSpacing;
				GameObject gameObject10 = Object.Instantiate<GameObject>(this.MiddleSupportPrefab, position + base.transform.rotation * this.MiddleSupportOffset, quaternion);
				this.spawnedSupports.Add(gameObject10);
			}
			foreach (GameObject gameObject11 in this.spawnedSupports)
			{
				gameObject11.transform.parent = base.transform;
			}
		}
	}

	// Token: 0x060002F9 RID: 761 RVA: 0x0000ED48 File Offset: 0x0000CF48
	private void InstantiateBottomCapPrefab(Vector3 position, Quaternion rotation)
	{
		if (this.BottomCapPrefab == null)
		{
			return;
		}
		Vector3 vector = new Vector3(Random.Range(this.MinBottomCapRotation.x, this.MaxBottomCapRotation.x), Random.Range(this.MinBottomCapRotation.y, this.MaxBottomCapRotation.y), Random.Range(this.MinBottomCapRotation.z, this.MaxBottomCapRotation.z));
		Vector3 vector2 = new Vector3(Random.Range(this.MinBottomCapScale.x, this.MaxBottomCapScale.x), Random.Range(this.MinBottomCapScale.y, this.MaxBottomCapScale.y), Random.Range(this.MinBottomCapScale.z, this.MaxBottomCapScale.z));
		GameObject gameObject = Object.Instantiate<GameObject>(this.BottomCapPrefab, position + base.transform.rotation * this.BottomCapOffset, Quaternion.Euler(vector) * rotation);
		gameObject.transform.localScale = vector2;
		this.spawnedSupports.Add(gameObject);
	}

	// Token: 0x060002FA RID: 762 RVA: 0x0000EE60 File Offset: 0x0000D060
	private void OnDestroy()
	{
		foreach (GameObject gameObject in this.spawnedSupports)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
	}

	// Token: 0x060002FB RID: 763 RVA: 0x0000EEBC File Offset: 0x0000D0BC
	public override void RespawnSupports(bool RespawnNextFrame = false)
	{
		if (RespawnNextFrame)
		{
			base.StartCoroutine(this.DelayedRespawn());
			return;
		}
		this.RebuildSupports();
	}

	// Token: 0x060002FC RID: 764 RVA: 0x0000EED8 File Offset: 0x0000D0D8
	private void RebuildSupports()
	{
		if (this == null)
		{
			return;
		}
		foreach (GameObject gameObject in this.spawnedSupports)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
		this.spawnedSupports.Clear();
		this.SpawnSupports();
	}

	// Token: 0x060002FD RID: 765 RVA: 0x0000EF50 File Offset: 0x0000D150
	private IEnumerator DelayedRespawn()
	{
		yield return new WaitForFixedUpdate();
		this.RebuildSupports();
		yield break;
	}

	// Token: 0x040002CA RID: 714
	public SupportType SupportType;

	// Token: 0x040002CB RID: 715
	public GameObject TopSupportPrefab;

	// Token: 0x040002CC RID: 716
	public GameObject MiddleSupportPrefab;

	// Token: 0x040002CD RID: 717
	public GameObject BottomCapPrefab;

	// Token: 0x040002CE RID: 718
	public float SupportSpacing = 1f;

	// Token: 0x040002CF RID: 719
	public int MaxSupports = 15;

	// Token: 0x040002D0 RID: 720
	public Vector3 RaycastOffset = new Vector3(0f, 0.4f, 0f);

	// Token: 0x040002D1 RID: 721
	public GameObject BottomToRollerPrefab;

	// Token: 0x040002D2 RID: 722
	public GameObject MiddleToRollerPrefab;

	// Token: 0x040002D3 RID: 723
	public GameObject BottomToConveyorPrefab;

	// Token: 0x040002D4 RID: 724
	public GameObject MiddleToConveyorPrefab;

	// Token: 0x040002D5 RID: 725
	public GameObject BottomToChutePrefab;

	// Token: 0x040002D6 RID: 726
	public GameObject MiddleToChutePrefab;

	// Token: 0x040002D7 RID: 727
	public GameObject BottomToWalledPrefab;

	// Token: 0x040002D8 RID: 728
	public GameObject MiddleToWalledPrefab;

	// Token: 0x040002D9 RID: 729
	public Vector3 TopSupportOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040002DA RID: 730
	public Vector3 MiddleSupportOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040002DB RID: 731
	public Vector3 BottomCapOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040002DC RID: 732
	public Vector3 MinBottomCapRotation = new Vector3(-0.1f, -1f, -0.1f);

	// Token: 0x040002DD RID: 733
	public Vector3 MaxBottomCapRotation = new Vector3(0.1f, 1f, 0.1f);

	// Token: 0x040002DE RID: 734
	public Vector3 MinBottomCapScale = new Vector3(0.95f, 0.95f, 0.95f);

	// Token: 0x040002DF RID: 735
	public Vector3 MaxBottomCapScale = new Vector3(1.05f, 1.05f, 1.05f);

	// Token: 0x040002E0 RID: 736
	public Vector3 RotationOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040002E1 RID: 737
	private List<GameObject> spawnedSupports = new List<GameObject>();
}
