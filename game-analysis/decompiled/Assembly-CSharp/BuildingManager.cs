using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000016 RID: 22
public class BuildingManager : Singleton<BuildingManager>
{
	// Token: 0x1700000D RID: 13
	// (get) Token: 0x060000AB RID: 171 RVA: 0x000045CC File Offset: 0x000027CC
	public Transform GhostObjectTransform
	{
		get
		{
			return this._ghostObject.transform;
		}
	}

	// Token: 0x060000AC RID: 172 RVA: 0x000045D9 File Offset: 0x000027D9
	public BuildingObject GetGhostObject()
	{
		return this._ghostObject;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x000045E1 File Offset: 0x000027E1
	public bool IsInBuildingMode()
	{
		return this._ghostObject != null;
	}

	// Token: 0x060000AE RID: 174 RVA: 0x000045F0 File Offset: 0x000027F0
	public CanPlaceBuilding CanPlaceObject(Vector3Int position, BuildingObject objPrefab, Quaternion rotation, bool requiresFlatGround = false, bool canBePlacedInTerrain = false, PlacementNodeRequirement placementNodeRequirement = PlacementNodeRequirement.None, ToolBuilder activeTool = null)
	{
		BuildingPlacementNode buildingPlacementNode;
		return this.CanPlaceObject(position, objPrefab, rotation, requiresFlatGround, canBePlacedInTerrain, placementNodeRequirement, out buildingPlacementNode, activeTool);
	}

	// Token: 0x060000AF RID: 175 RVA: 0x00004610 File Offset: 0x00002810
	public LayerMask GetBuildingPlacementLayerMask(bool canPlaceInTerrain)
	{
		if (!canPlaceInTerrain)
		{
			return this.BuildingPlacementCollisionLayers;
		}
		return this.ScaffoldingPlacementCollisionLayers;
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x00004624 File Offset: 0x00002824
	public CanPlaceBuilding CanPlaceObject(Vector3Int position, BuildingObject objPrefab, Quaternion rotation, bool requiresFlatGround, bool canBePlacedInTerrain, PlacementNodeRequirement placementNodeRequirement, out BuildingPlacementNode AttachedNode, ToolBuilder activeTool)
	{
		AttachedNode = null;
		if (this._ghostObject == null)
		{
			return CanPlaceBuilding.Invalid;
		}
		LayerMask layerMask = ((placementNodeRequirement == PlacementNodeRequirement.None) ? this.GetBuildingPlacementLayerMask(canBePlacedInTerrain) : this.CollisionLayersExcludeGround);
		List<Collider> list = new List<Collider>();
		if (this._ghostObject.BuildingPlacementColliderObject != null)
		{
			list.AddRange(this._ghostObject.BuildingPlacementColliderObject.GetComponentsInChildren<Collider>());
		}
		if (list.Count == 0 && this._ghostObject.PhysicalColliderObject != null)
		{
			list.AddRange(this._ghostObject.PhysicalColliderObject.GetComponentsInChildren<Collider>());
		}
		foreach (Collider collider in list)
		{
			if (!(collider == null))
			{
				BoxCollider boxCollider = collider as BoxCollider;
				if (boxCollider != null)
				{
					Vector3 vector = boxCollider.transform.TransformPoint(boxCollider.center);
					Vector3 vector2 = Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale);
					Quaternion rotation2 = boxCollider.transform.rotation;
					if (Physics.OverlapBox(vector, vector2, rotation2, layerMask, QueryTriggerInteraction.Ignore).Length != 0)
					{
						return CanPlaceBuilding.Invalid;
					}
				}
			}
		}
		if (placementNodeRequirement == PlacementNodeRequirement.None)
		{
			if (requiresFlatGround)
			{
				RaycastHit raycastHit;
				if (!Physics.Raycast(position + new Vector3(0.5f, 0.1f, 0.5f), Vector3.down, out raycastHit, 1f, this.BuildingPlacementCollisionLayers))
				{
					return CanPlaceBuilding.RequirementsNotMet;
				}
				if (Vector3.Dot(raycastHit.normal, Vector3.up) < 0.9f)
				{
					return CanPlaceBuilding.RequirementsNotMet;
				}
				if (raycastHit.distance > 0.2f)
				{
					return CanPlaceBuilding.RequirementsNotMet;
				}
			}
			List<BuildingRotationInfo> list2 = new List<BuildingRotationInfo>();
			if (this._isEligibleForSnapping && this._ghostObject.ConveyorInputSnapPositions.Count > 0)
			{
				Collider[] array = Physics.OverlapSphere(this._ghostObject.transform.position, 1.25f, this.BuildingObjectLayer);
				for (int i = 0; i < array.Length; i++)
				{
					BuildingObject componentInParent = array[i].GetComponentInParent<BuildingObject>();
					if (componentInParent != null && componentInParent != this._ghostObject)
					{
						list2.AddRange(this.GetNearbySnapConnections(this._ghostObject.transform, this._ghostObject, componentInParent, activeTool.IsUsingMirroredVersion()));
					}
				}
				this._isEligibleForSnapping = false;
			}
			if (list2.Count > 0)
			{
				List<IGrouping<BuildingRotationInfo, BuildingRotationInfo>> list3 = (from r in list2
					group r by r into g
					orderby g.Count<BuildingRotationInfo>() descending
					select g).ToList<IGrouping<BuildingRotationInfo, BuildingRotationInfo>>();
				BuildingRotationInfo buildingRotationInfo;
				if (list3[0].Count<BuildingRotationInfo>() == 1)
				{
					buildingRotationInfo = list2[0];
				}
				else
				{
					buildingRotationInfo = list3[0].Key;
				}
				rotation = buildingRotationInfo.Rotation;
				if (activeTool != null)
				{
					activeTool.IsUsingMirroredVersion();
					bool isMirroredMode = buildingRotationInfo.IsMirroredMode;
					activeTool.CurrentRotation = buildingRotationInfo.Rotation;
				}
				this.CurrentObjectIsSnapped = true;
				this._isEligibleForSnapping = false;
			}
			return CanPlaceBuilding.Valid;
		}
		List<BuildingPlacementNode> all = BuildingPlacementNode.All;
		BuildingPlacementNode buildingPlacementNode = null;
		float num = float.MaxValue;
		foreach (BuildingPlacementNode buildingPlacementNode2 in all)
		{
			if (buildingPlacementNode2.RequirementType == placementNodeRequirement && buildingPlacementNode2.AttachedBuildingObject == null)
			{
				float num2 = Vector3.Distance(position, buildingPlacementNode2.transform.position);
				if (num2 < 4f && num2 < num)
				{
					num = num2;
					buildingPlacementNode = buildingPlacementNode2;
				}
			}
		}
		if (buildingPlacementNode != null)
		{
			this._ghostObject.transform.position = buildingPlacementNode.transform.position;
			this._ghostObject.transform.rotation = buildingPlacementNode.transform.rotation;
			AttachedNode = buildingPlacementNode;
			Singleton<UIManager>.Instance.ShowBuildingInfo(AttachedNode.GetAttachmentText(objPrefab));
			return CanPlaceBuilding.Valid;
		}
		Singleton<UIManager>.Instance.HideBuildingInfo();
		return CanPlaceBuilding.RequirementsNotMet;
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x00004A5C File Offset: 0x00002C5C
	private List<BuildingRotationInfo> GetNearbySnapConnections(Transform ghostTransform, BuildingObject building, BuildingObject neighbor, bool isMirrored)
	{
		List<BuildingRotationInfo> list = new List<BuildingRotationInfo>();
		Vector3 position = ghostTransform.transform.position;
		if (neighbor.ConveyorOutputSnapPositions.Count > 0)
		{
			for (int i = 0; i < 4; i++)
			{
				Quaternion quaternion = Quaternion.Euler(0f, (float)i * 90f, 0f);
				Matrix4x4 matrix4x = Matrix4x4.TRS(position, quaternion, Vector3.one);
				foreach (Transform transform in building.ConveyorInputSnapPositions)
				{
					Vector3 localPosition = transform.localPosition;
					Vector3 vector = matrix4x.MultiplyPoint3x4(localPosition);
					foreach (Transform transform2 in neighbor.ConveyorOutputSnapPositions)
					{
						Vector3 position2 = transform2.position;
						if (Vector3.Distance(vector, position2) < 0.25f)
						{
							list.Add(new BuildingRotationInfo
							{
								Rotation = quaternion,
								IsMirroredMode = isMirrored
							});
						}
					}
				}
			}
		}
		if (neighbor.ConveyorInputSnapPositions.Count > 0)
		{
			for (int j = 0; j < 4; j++)
			{
				Quaternion quaternion2 = Quaternion.Euler(0f, (float)j * 90f, 0f);
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(position, quaternion2, Vector3.one);
				foreach (Transform transform3 in building.ConveyorOutputSnapPositions)
				{
					Vector3 localPosition2 = transform3.localPosition;
					Vector3 vector2 = matrix4x2.MultiplyPoint3x4(localPosition2);
					foreach (Transform transform4 in neighbor.ConveyorInputSnapPositions)
					{
						Vector3 position3 = transform4.position;
						if (Vector3.Distance(vector2, position3) < 0.25f)
						{
							list.Add(new BuildingRotationInfo
							{
								Rotation = quaternion2,
								IsMirroredMode = isMirrored
							});
						}
					}
				}
			}
		}
		return list;
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x00004CA8 File Offset: 0x00002EA8
	private void SetupGhostObject(Vector3Int position, BuildingObject prefab, Quaternion rotation)
	{
		if (this._ghostObject == null)
		{
			this._ghostObject = Object.Instantiate<BuildingObject>(prefab);
			this._ghostObject.IsGhost = true;
			foreach (Collider collider in this._ghostObject.GetComponentsInChildren<Collider>())
			{
				if (collider.isTrigger)
				{
					Object.Destroy(collider.gameObject);
				}
			}
			foreach (MonoBehaviour monoBehaviour in this._ghostObject.GetComponentsInChildren<MonoBehaviour>(true))
			{
				if (!(monoBehaviour is BuildingObject))
				{
					monoBehaviour.enabled = false;
				}
			}
			AudioSource[] componentsInChildren3 = this._ghostObject.GetComponentsInChildren<AudioSource>(true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
			int num = LayerMask.NameToLayer("BuildingGhost");
			this.SetLayerRecursively(this._ghostObject.gameObject, num);
			Rigidbody[] componentsInChildren4 = this._ghostObject.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < componentsInChildren4.Length; i++)
			{
				componentsInChildren4[i].isKinematic = true;
			}
			ParticleSystem[] componentsInChildren5 = this._ghostObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren5.Length; i++)
			{
				componentsInChildren5[i].gameObject.SetActive(false);
			}
		}
		this._ghostObject.transform.position = position + new Vector3(0.5f, 0f, 0.5f) + this._ghostObject.BuildModePlacementOffset;
		this._ghostObject.transform.rotation = rotation;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x00004E28 File Offset: 0x00003028
	public void UpdateGhostObject(Vector3Int position, BuildingObject prefab, Quaternion rotation, ToolBuilder activeTool)
	{
		this.SetupGhostObject(position, prefab, rotation);
		if (this._previousPosition != this._ghostObject.transform.position)
		{
			this._isEligibleForSnapping = true;
			this.CurrentObjectIsSnapped = false;
			this._previousPosition = this._ghostObject.transform.position;
		}
		Material material = this.GhostMaterial;
		CanPlaceBuilding canPlaceBuilding = this.CanPlaceObject(position, prefab, rotation, prefab.RequiresFlatGround, activeTool.Definition.CanBePlacedInTerrain, prefab.PlacementNodeRequirement, activeTool);
		if (canPlaceBuilding != CanPlaceBuilding.Invalid)
		{
			if (canPlaceBuilding == CanPlaceBuilding.RequirementsNotMet)
			{
				material = this.RequirementGhostMaterial;
			}
		}
		else
		{
			material = this.InvalidGhostMaterial;
		}
		if (this._ghostObject == null)
		{
			this.SetupGhostObject(position, prefab, rotation);
		}
		foreach (Renderer renderer in this._ghostObject.GetComponentsInChildren<Renderer>())
		{
			if (!(this._ghostObject.ExtraGhostRenderers != null) || !renderer.transform.IsChildOf(this._ghostObject.ExtraGhostRenderers.transform))
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					if (!this.MaterialsToNotReplaceOnBuildingGhost.Contains(sharedMaterials[j]))
					{
						sharedMaterials[j] = material;
					}
				}
				renderer.sharedMaterials = sharedMaterials;
			}
		}
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00004F68 File Offset: 0x00003168
	private void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = newLayer;
		foreach (object obj2 in obj.transform)
		{
			Transform transform = (Transform)obj2;
			if (!(transform == null))
			{
				this.SetLayerRecursively(transform.gameObject, newLayer);
			}
		}
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x00004FE4 File Offset: 0x000031E4
	private void OnDestroy()
	{
		if (this._ghostObject != null)
		{
			Object.Destroy(this._ghostObject.gameObject);
		}
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x00005004 File Offset: 0x00003204
	public void CleanUpGhostObject()
	{
		this._isEligibleForSnapping = true;
		this.CurrentObjectIsSnapped = false;
		if (this._ghostObject != null)
		{
			Object.Destroy(this._ghostObject.gameObject);
			this._ghostObject = null;
		}
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x0000503C File Offset: 0x0000323C
	public Vector3Int GetClosestGridPosition(Vector3 worldPosition)
	{
		worldPosition -= new Vector3(0.5f, 0.4f, 0.5f);
		int num = Mathf.RoundToInt(worldPosition.x);
		int num2 = Mathf.RoundToInt(worldPosition.y);
		int num3 = Mathf.RoundToInt(worldPosition.z);
		return new Vector3Int(num, num2, num3);
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x00005090 File Offset: 0x00003290
	private void DrawDebugBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
	{
		Vector3[] array = new Vector3[]
		{
			center + rotation * new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
			center + rotation * new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z),
			center + rotation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z),
			center + rotation * new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z),
			center + rotation * new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z),
			center + rotation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z),
			center + rotation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z),
			center + rotation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z)
		};
		Debug.DrawLine(array[0], array[1], color);
		Debug.DrawLine(array[1], array[2], color);
		Debug.DrawLine(array[2], array[3], color);
		Debug.DrawLine(array[3], array[0], color);
		Debug.DrawLine(array[4], array[5], color);
		Debug.DrawLine(array[5], array[6], color);
		Debug.DrawLine(array[6], array[7], color);
		Debug.DrawLine(array[7], array[4], color);
		Debug.DrawLine(array[0], array[4], color);
		Debug.DrawLine(array[1], array[5], color);
		Debug.DrawLine(array[2], array[6], color);
		Debug.DrawLine(array[3], array[7], color);
	}

	// Token: 0x04000099 RID: 153
	[FormerlySerializedAs("CollisionLayers")]
	public LayerMask BuildingPlacementCollisionLayers;

	// Token: 0x0400009A RID: 154
	public LayerMask ScaffoldingPlacementCollisionLayers;

	// Token: 0x0400009B RID: 155
	public LayerMask BuildingSupportsCollisionLayers;

	// Token: 0x0400009C RID: 156
	public LayerMask ScaffoldingSupportsCollisionLayers;

	// Token: 0x0400009D RID: 157
	public LayerMask CollisionLayersExcludeGround;

	// Token: 0x0400009E RID: 158
	public LayerMask BuildingObjectLayer;

	// Token: 0x0400009F RID: 159
	public LayerMask BuildingPlacementRaycastLayer;

	// Token: 0x040000A0 RID: 160
	public Material GhostMaterial;

	// Token: 0x040000A1 RID: 161
	public Material InvalidGhostMaterial;

	// Token: 0x040000A2 RID: 162
	public Material RequirementGhostMaterial;

	// Token: 0x040000A3 RID: 163
	public List<Material> MaterialsToNotReplaceOnBuildingGhost;

	// Token: 0x040000A4 RID: 164
	public Material BuildingNodeGhost;

	// Token: 0x040000A5 RID: 165
	public Material BuildingNodeGhost_WrongType;

	// Token: 0x040000A6 RID: 166
	public Material GreenLightMaterial;

	// Token: 0x040000A7 RID: 167
	public Material RedLightMaterial;

	// Token: 0x040000A8 RID: 168
	public Material OrangeLightMaterial;

	// Token: 0x040000A9 RID: 169
	public BuildingCrate BuildingCratePrefab;

	// Token: 0x040000AA RID: 170
	public ToolBuilder BuildingToolPrefab;

	// Token: 0x040000AB RID: 171
	public Interaction InteractionPack;

	// Token: 0x040000AC RID: 172
	public Interaction InteractionTake;

	// Token: 0x040000AD RID: 173
	private BuildingObject _ghostObject;

	// Token: 0x040000AE RID: 174
	private Vector3 _previousPosition;

	// Token: 0x040000AF RID: 175
	private bool _isEligibleForSnapping;

	// Token: 0x040000B0 RID: 176
	public bool CurrentObjectIsSnapped;
}
