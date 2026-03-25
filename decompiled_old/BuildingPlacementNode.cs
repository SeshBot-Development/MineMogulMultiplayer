using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementNode : MonoBehaviour
{
	public PlacementNodeRequirement RequirementType;

	public GameObject GhostPrefab;

	public AutoMinerResourceDefinition AutoMinerResourceDefinition;

	public static List<BuildingPlacementNode> All = new List<BuildingPlacementNode>();

	private Material _lastUsedGhostMaterial;

	public BuildingObject AttachedBuildingObject { get; private set; }

	private void OnEnable()
	{
		All.Add(this);
	}

	private void OnDisable()
	{
		All.Remove(this);
	}

	private void Start()
	{
		ShowGhost(show: false);
	}

	public ResourceType GetPrimaryResourceType()
	{
		if (AutoMinerResourceDefinition != null)
		{
			return AutoMinerResourceDefinition.GetPrimaryResourceType();
		}
		return ResourceType.INVALID;
	}

	public void AttachBuilding(BuildingObject attachedBuildingObject)
	{
		AttachedBuildingObject = attachedBuildingObject;
		AutoMiner component = AttachedBuildingObject.gameObject.GetComponent<AutoMiner>();
		if (component != null)
		{
			component.ResourceDefinition = AutoMinerResourceDefinition;
			component.ConfigureFromDefinition();
		}
	}

	public void ShowGhost(bool show = true, PlacementNodeRequirement placementNodeRequirement = PlacementNodeRequirement.None)
	{
		if (AttachedBuildingObject != null)
		{
			show = false;
		}
		if (show)
		{
			Material material = ((placementNodeRequirement == PlacementNodeRequirement.None || placementNodeRequirement == RequirementType) ? Singleton<BuildingManager>.Instance.BuildingNodeGhost : Singleton<BuildingManager>.Instance.BuildingNodeGhost_WrongType);
			if (_lastUsedGhostMaterial != material)
			{
				_lastUsedGhostMaterial = material;
				Renderer[] componentsInChildren = GhostPrefab.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					for (int j = 0; j < sharedMaterials.Length; j++)
					{
						sharedMaterials[j] = _lastUsedGhostMaterial;
					}
					renderer.sharedMaterials = sharedMaterials;
				}
			}
		}
		GhostPrefab.SetActive(show);
	}

	public string GetAttachmentText(BuildingObject buildingObject)
	{
		string result = "error!";
		if (AutoMinerResourceDefinition != null)
		{
			bool canProduceGems = true;
			if (buildingObject.TryGetComponent<AutoMiner>(out var component))
			{
				canProduceGems = component.CanProduceGems;
			}
			result = AutoMinerResourceDefinition.GetFormattedAvailableResourcesText(canProduceGems);
		}
		return result;
	}

	private static bool IsOnGrid(Vector3 pos, Vector3 eulerDeg)
	{
		float num = 0.001f;
		bool num2 = IsInteger(pos.x - 0.5f, num);
		bool flag = IsInteger(pos.z - 0.5f, num);
		bool flag2 = IsInteger(pos.y, num);
		float f = NormalizeAngle(eulerDeg.x);
		float value = NormalizeAngle(eulerDeg.y);
		float f2 = NormalizeAngle(eulerDeg.z);
		bool flag3 = Mathf.Abs(f) < num;
		bool flag4 = Mathf.Abs(f2) < num;
		bool flag5 = IsMultipleOf(value, 90f, num);
		return num2 && flag2 && flag && flag3 && flag5 && flag4;
	}

	private static bool IsInteger(float value, float eps)
	{
		return Mathf.Abs(value - Mathf.Round(value)) < eps;
	}

	private static bool IsMultipleOf(float value, float step, float eps)
	{
		return Mathf.Abs(value - Mathf.Round(value / step) * step) < eps;
	}

	private static float NormalizeAngle(float deg)
	{
		deg %= 360f;
		if (deg < 0f)
		{
			deg += 360f;
		}
		if (!(Mathf.Abs(deg - 360f) < 1E-05f))
		{
			return deg;
		}
		return 0f;
	}
}
