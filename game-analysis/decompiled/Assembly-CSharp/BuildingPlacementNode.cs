using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200001A RID: 26
public class BuildingPlacementNode : MonoBehaviour
{
	// Token: 0x1700000F RID: 15
	// (get) Token: 0x060000D9 RID: 217 RVA: 0x00005958 File Offset: 0x00003B58
	// (set) Token: 0x060000DA RID: 218 RVA: 0x00005960 File Offset: 0x00003B60
	public BuildingObject AttachedBuildingObject { get; private set; }

	// Token: 0x060000DB RID: 219 RVA: 0x00005969 File Offset: 0x00003B69
	private void OnEnable()
	{
		BuildingPlacementNode.All.Add(this);
	}

	// Token: 0x060000DC RID: 220 RVA: 0x00005976 File Offset: 0x00003B76
	private void OnDisable()
	{
		BuildingPlacementNode.All.Remove(this);
	}

	// Token: 0x060000DD RID: 221 RVA: 0x00005984 File Offset: 0x00003B84
	private void Start()
	{
		this.ShowGhost(false, PlacementNodeRequirement.None);
	}

	// Token: 0x060000DE RID: 222 RVA: 0x0000598E File Offset: 0x00003B8E
	public ResourceType GetPrimaryResourceType()
	{
		if (this.AutoMinerResourceDefinition != null)
		{
			return this.AutoMinerResourceDefinition.GetPrimaryResourceType();
		}
		return ResourceType.INVALID;
	}

	// Token: 0x060000DF RID: 223 RVA: 0x000059AC File Offset: 0x00003BAC
	public void AttachBuilding(BuildingObject attachedBuildingObject)
	{
		this.AttachedBuildingObject = attachedBuildingObject;
		AutoMiner component = this.AttachedBuildingObject.gameObject.GetComponent<AutoMiner>();
		if (component != null)
		{
			component.ResourceDefinition = this.AutoMinerResourceDefinition;
			component.ConfigureFromDefinition();
		}
	}

	// Token: 0x060000E0 RID: 224 RVA: 0x000059EC File Offset: 0x00003BEC
	public void ShowGhost(bool show = true, PlacementNodeRequirement placementNodeRequirement = PlacementNodeRequirement.None)
	{
		if (this.AttachedBuildingObject != null)
		{
			show = false;
		}
		if (show)
		{
			Material material = ((placementNodeRequirement == PlacementNodeRequirement.None || placementNodeRequirement == this.RequirementType) ? Singleton<BuildingManager>.Instance.BuildingNodeGhost : Singleton<BuildingManager>.Instance.BuildingNodeGhost_WrongType);
			if (this._lastUsedGhostMaterial != material)
			{
				this._lastUsedGhostMaterial = material;
				foreach (Renderer renderer in this.GhostPrefab.GetComponentsInChildren<Renderer>())
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					for (int j = 0; j < sharedMaterials.Length; j++)
					{
						sharedMaterials[j] = this._lastUsedGhostMaterial;
					}
					renderer.sharedMaterials = sharedMaterials;
				}
			}
		}
		this.GhostPrefab.SetActive(show);
	}

	// Token: 0x060000E1 RID: 225 RVA: 0x00005AA8 File Offset: 0x00003CA8
	public string GetAttachmentText(BuildingObject buildingObject)
	{
		string text = "error!";
		if (this.AutoMinerResourceDefinition != null)
		{
			bool flag = true;
			AutoMiner autoMiner;
			if (buildingObject.TryGetComponent<AutoMiner>(out autoMiner))
			{
				flag = autoMiner.CanProduceGems;
			}
			text = this.AutoMinerResourceDefinition.GetFormattedAvailableResourcesText(flag);
		}
		return text;
	}

	// Token: 0x060000E2 RID: 226 RVA: 0x00005AEC File Offset: 0x00003CEC
	private static bool IsOnGrid(Vector3 pos, Vector3 eulerDeg)
	{
		float num = 0.001f;
		bool flag = BuildingPlacementNode.IsInteger(pos.x - 0.5f, num);
		bool flag2 = BuildingPlacementNode.IsInteger(pos.z - 0.5f, num);
		bool flag3 = BuildingPlacementNode.IsInteger(pos.y, num);
		float num2 = BuildingPlacementNode.NormalizeAngle(eulerDeg.x);
		float num3 = BuildingPlacementNode.NormalizeAngle(eulerDeg.y);
		float num4 = BuildingPlacementNode.NormalizeAngle(eulerDeg.z);
		bool flag4 = Mathf.Abs(num2) < num;
		bool flag5 = Mathf.Abs(num4) < num;
		bool flag6 = BuildingPlacementNode.IsMultipleOf(num3, 90f, num);
		return flag && flag3 && flag2 && flag4 && flag6 && flag5;
	}

	// Token: 0x060000E3 RID: 227 RVA: 0x00005B82 File Offset: 0x00003D82
	private static bool IsInteger(float value, float eps)
	{
		return Mathf.Abs(value - Mathf.Round(value)) < eps;
	}

	// Token: 0x060000E4 RID: 228 RVA: 0x00005B94 File Offset: 0x00003D94
	private static bool IsMultipleOf(float value, float step, float eps)
	{
		return Mathf.Abs(value - Mathf.Round(value / step) * step) < eps;
	}

	// Token: 0x060000E5 RID: 229 RVA: 0x00005BAA File Offset: 0x00003DAA
	private static float NormalizeAngle(float deg)
	{
		deg %= 360f;
		if (deg < 0f)
		{
			deg += 360f;
		}
		if (Mathf.Abs(deg - 360f) >= 1E-05f)
		{
			return deg;
		}
		return 0f;
	}

	// Token: 0x040000CC RID: 204
	public PlacementNodeRequirement RequirementType;

	// Token: 0x040000CD RID: 205
	public GameObject GhostPrefab;

	// Token: 0x040000CF RID: 207
	public AutoMinerResourceDefinition AutoMinerResourceDefinition;

	// Token: 0x040000D0 RID: 208
	public static List<BuildingPlacementNode> All = new List<BuildingPlacementNode>();

	// Token: 0x040000D1 RID: 209
	private Material _lastUsedGhostMaterial;
}
