using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingManager : Singleton<BuildingManager>
{
	[FormerlySerializedAs("CollisionLayers")]
	public LayerMask BuildingPlacementCollisionLayers;

	public LayerMask ScaffoldingPlacementCollisionLayers;

	public LayerMask BuildingSupportsCollisionLayers;

	public LayerMask ScaffoldingSupportsCollisionLayers;

	public LayerMask CollisionLayersExcludeGround;

	public LayerMask BuildingObjectLayer;

	public LayerMask BuildingPlacementRaycastLayer;

	public Material GhostMaterial;

	public Material InvalidGhostMaterial;

	public Material RequirementGhostMaterial;

	public List<Material> MaterialsToNotReplaceOnBuildingGhost;

	public Material BuildingNodeGhost;

	public Material BuildingNodeGhost_WrongType;

	public Material GreenLightMaterial;

	public Material RedLightMaterial;

	public Material OrangeLightMaterial;

	public BuildingCrate BuildingCratePrefab;

	public ToolBuilder BuildingToolPrefab;

	public Interaction InteractionPack;

	public Interaction InteractionTake;

	private BuildingObject _ghostObject;

	private Vector3 _previousPosition;

	private bool _isEligibleForSnapping;

	public bool CurrentObjectIsSnapped;

	public Transform GhostObjectTransform => _ghostObject.transform;

	public BuildingObject GetGhostObject()
	{
		return _ghostObject;
	}

	public bool IsInBuildingMode()
	{
		return _ghostObject != null;
	}

	public CanPlaceBuilding CanPlaceObject(Vector3Int position, BuildingObject objPrefab, Quaternion rotation, bool requiresFlatGround = false, bool canBePlacedInTerrain = false, PlacementNodeRequirement placementNodeRequirement = PlacementNodeRequirement.None, ToolBuilder activeTool = null)
	{
		BuildingPlacementNode AttachedNode;
		return CanPlaceObject(position, objPrefab, rotation, requiresFlatGround, canBePlacedInTerrain, placementNodeRequirement, out AttachedNode, activeTool);
	}

	public LayerMask GetBuildingPlacementLayerMask(bool canPlaceInTerrain)
	{
		if (!canPlaceInTerrain)
		{
			return BuildingPlacementCollisionLayers;
		}
		return ScaffoldingPlacementCollisionLayers;
	}

	public CanPlaceBuilding CanPlaceObject(Vector3Int position, BuildingObject objPrefab, Quaternion rotation, bool requiresFlatGround, bool canBePlacedInTerrain, PlacementNodeRequirement placementNodeRequirement, out BuildingPlacementNode AttachedNode, ToolBuilder activeTool)
	{
		AttachedNode = null;
		if (_ghostObject == null)
		{
			return CanPlaceBuilding.Invalid;
		}
		LayerMask layerMask = ((placementNodeRequirement == PlacementNodeRequirement.None) ? GetBuildingPlacementLayerMask(canBePlacedInTerrain) : CollisionLayersExcludeGround);
		List<Collider> list = new List<Collider>();
		if (_ghostObject.BuildingPlacementColliderObject != null)
		{
			list.AddRange(_ghostObject.BuildingPlacementColliderObject.GetComponentsInChildren<Collider>());
		}
		if (list.Count == 0 && _ghostObject.PhysicalColliderObject != null)
		{
			list.AddRange(_ghostObject.PhysicalColliderObject.GetComponentsInChildren<Collider>());
		}
		foreach (Collider item in list)
		{
			if (!(item == null) && item is BoxCollider boxCollider)
			{
				Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center);
				Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale);
				Quaternion rotation2 = boxCollider.transform.rotation;
				if (Physics.OverlapBox(center, halfExtents, rotation2, layerMask, QueryTriggerInteraction.Ignore).Length != 0)
				{
					return CanPlaceBuilding.Invalid;
				}
			}
		}
		if (placementNodeRequirement != PlacementNodeRequirement.None)
		{
			List<BuildingPlacementNode> all = BuildingPlacementNode.All;
			BuildingPlacementNode buildingPlacementNode = null;
			float num = float.MaxValue;
			foreach (BuildingPlacementNode item2 in all)
			{
				if (item2.RequirementType == placementNodeRequirement && item2.AttachedBuildingObject == null)
				{
					float num2 = Vector3.Distance(position, item2.transform.position);
					if (num2 < 4f && num2 < num)
					{
						num = num2;
						buildingPlacementNode = item2;
					}
				}
			}
			if (buildingPlacementNode != null)
			{
				_ghostObject.transform.position = buildingPlacementNode.transform.position;
				_ghostObject.transform.rotation = buildingPlacementNode.transform.rotation;
				AttachedNode = buildingPlacementNode;
				Singleton<UIManager>.Instance.ShowBuildingInfo(AttachedNode.GetAttachmentText(objPrefab));
				return CanPlaceBuilding.Valid;
			}
			Singleton<UIManager>.Instance.HideBuildingInfo();
			return CanPlaceBuilding.RequirementsNotMet;
		}
		if (requiresFlatGround)
		{
			if (!Physics.Raycast(position + new Vector3(0.5f, 0.1f, 0.5f), Vector3.down, out var hitInfo, 1f, BuildingPlacementCollisionLayers))
			{
				return CanPlaceBuilding.RequirementsNotMet;
			}
			if (Vector3.Dot(hitInfo.normal, Vector3.up) < 0.9f)
			{
				return CanPlaceBuilding.RequirementsNotMet;
			}
			if (hitInfo.distance > 0.2f)
			{
				return CanPlaceBuilding.RequirementsNotMet;
			}
		}
		List<BuildingRotationInfo> list2 = new List<BuildingRotationInfo>();
		if (_isEligibleForSnapping && _ghostObject.ConveyorInputSnapPositions.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(_ghostObject.transform.position, 1.25f, BuildingObjectLayer);
			for (int i = 0; i < array.Length; i++)
			{
				BuildingObject componentInParent = array[i].GetComponentInParent<BuildingObject>();
				if (componentInParent != null && componentInParent != _ghostObject)
				{
					list2.AddRange(GetNearbySnapConnections(_ghostObject.transform, _ghostObject, componentInParent, activeTool.IsUsingMirroredVersion()));
				}
			}
			_isEligibleForSnapping = false;
		}
		if (list2.Count > 0)
		{
			List<IGrouping<BuildingRotationInfo, BuildingRotationInfo>> list3 = (from r in list2
				group r by r into g
				orderby g.Count() descending
				select g).ToList();
			BuildingRotationInfo buildingRotationInfo = ((list3[0].Count() != 1) ? list3[0].Key : list2[0]);
			rotation = buildingRotationInfo.Rotation;
			if (activeTool != null)
			{
				activeTool.IsUsingMirroredVersion();
				_ = buildingRotationInfo.IsMirroredMode;
				activeTool.CurrentRotation = buildingRotationInfo.Rotation;
			}
			CurrentObjectIsSnapped = true;
			_isEligibleForSnapping = false;
		}
		return CanPlaceBuilding.Valid;
	}

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
				foreach (Transform conveyorInputSnapPosition in building.ConveyorInputSnapPositions)
				{
					Vector3 localPosition = conveyorInputSnapPosition.localPosition;
					Vector3 a = matrix4x.MultiplyPoint3x4(localPosition);
					foreach (Transform conveyorOutputSnapPosition in neighbor.ConveyorOutputSnapPositions)
					{
						Vector3 position2 = conveyorOutputSnapPosition.position;
						if (Vector3.Distance(a, position2) < 0.25f)
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
				foreach (Transform conveyorOutputSnapPosition2 in building.ConveyorOutputSnapPositions)
				{
					Vector3 localPosition2 = conveyorOutputSnapPosition2.localPosition;
					Vector3 a2 = matrix4x2.MultiplyPoint3x4(localPosition2);
					foreach (Transform conveyorInputSnapPosition2 in neighbor.ConveyorInputSnapPositions)
					{
						Vector3 position3 = conveyorInputSnapPosition2.position;
						if (Vector3.Distance(a2, position3) < 0.25f)
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

	private void SetupGhostObject(Vector3Int position, BuildingObject prefab, Quaternion rotation)
	{
		if (_ghostObject == null)
		{
			_ghostObject = Object.Instantiate(prefab);
			_ghostObject.IsGhost = true;
			Collider[] componentsInChildren = _ghostObject.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.isTrigger)
				{
					Object.Destroy(collider.gameObject);
				}
			}
			MonoBehaviour[] componentsInChildren2 = _ghostObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
			foreach (MonoBehaviour monoBehaviour in componentsInChildren2)
			{
				if (!(monoBehaviour is BuildingObject))
				{
					monoBehaviour.enabled = false;
				}
			}
			AudioSource[] componentsInChildren3 = _ghostObject.GetComponentsInChildren<AudioSource>(includeInactive: true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
			int newLayer = LayerMask.NameToLayer("BuildingGhost");
			SetLayerRecursively(_ghostObject.gameObject, newLayer);
			Rigidbody[] componentsInChildren4 = _ghostObject.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < componentsInChildren4.Length; i++)
			{
				componentsInChildren4[i].isKinematic = true;
			}
			ParticleSystem[] componentsInChildren5 = _ghostObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren5.Length; i++)
			{
				componentsInChildren5[i].gameObject.SetActive(value: false);
			}
		}
		_ghostObject.transform.position = position + new Vector3(0.5f, 0f, 0.5f) + _ghostObject.BuildModePlacementOffset;
		_ghostObject.transform.rotation = rotation;
	}

	public void UpdateGhostObject(Vector3Int position, BuildingObject prefab, Quaternion rotation, ToolBuilder activeTool)
	{
		SetupGhostObject(position, prefab, rotation);
		if (_previousPosition != _ghostObject.transform.position)
		{
			_isEligibleForSnapping = true;
			CurrentObjectIsSnapped = false;
			_previousPosition = _ghostObject.transform.position;
		}
		Material material = GhostMaterial;
		switch (CanPlaceObject(position, prefab, rotation, prefab.RequiresFlatGround, activeTool.Definition.CanBePlacedInTerrain, prefab.PlacementNodeRequirement, activeTool))
		{
		case CanPlaceBuilding.Invalid:
			material = InvalidGhostMaterial;
			break;
		case CanPlaceBuilding.RequirementsNotMet:
			material = RequirementGhostMaterial;
			break;
		}
		if (_ghostObject == null)
		{
			SetupGhostObject(position, prefab, rotation);
		}
		Renderer[] componentsInChildren = _ghostObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (_ghostObject.ExtraGhostRenderers != null && renderer.transform.IsChildOf(_ghostObject.ExtraGhostRenderers.transform))
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (!MaterialsToNotReplaceOnBuildingGhost.Contains(sharedMaterials[j]))
				{
					sharedMaterials[j] = material;
				}
			}
			renderer.sharedMaterials = sharedMaterials;
		}
	}

	private void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = newLayer;
		foreach (Transform item in obj.transform)
		{
			if (!(item == null))
			{
				SetLayerRecursively(item.gameObject, newLayer);
			}
		}
	}

	private void OnDestroy()
	{
		if (_ghostObject != null)
		{
			Object.Destroy(_ghostObject.gameObject);
		}
	}

	public void CleanUpGhostObject()
	{
		_isEligibleForSnapping = true;
		CurrentObjectIsSnapped = false;
		if (_ghostObject != null)
		{
			Object.Destroy(_ghostObject.gameObject);
			_ghostObject = null;
		}
	}

	public Vector3Int GetClosestGridPosition(Vector3 worldPosition)
	{
		worldPosition -= new Vector3(0.5f, 0.4f, 0.5f);
		int x = Mathf.RoundToInt(worldPosition.x);
		int y = Mathf.RoundToInt(worldPosition.y);
		int z = Mathf.RoundToInt(worldPosition.z);
		return new Vector3Int(x, y, z);
	}

	private void DrawDebugBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
	{
		Vector3[] array = new Vector3[8];
		Vector3 vector = halfExtents;
		array[0] = center + rotation * new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z);
		array[1] = center + rotation * new Vector3(vector.x, 0f - vector.y, 0f - vector.z);
		array[2] = center + rotation * new Vector3(vector.x, 0f - vector.y, vector.z);
		array[3] = center + rotation * new Vector3(0f - vector.x, 0f - vector.y, vector.z);
		array[4] = center + rotation * new Vector3(0f - vector.x, vector.y, 0f - vector.z);
		array[5] = center + rotation * new Vector3(vector.x, vector.y, 0f - vector.z);
		array[6] = center + rotation * new Vector3(vector.x, vector.y, vector.z);
		array[7] = center + rotation * new Vector3(0f - vector.x, vector.y, vector.z);
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
}
