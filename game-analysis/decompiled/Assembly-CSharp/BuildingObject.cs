using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000019 RID: 25
public class BuildingObject : MonoBehaviour, IInteractable, ISaveLoadableBuildingObject, ISaveLoadableObject
{
	// Token: 0x14000001 RID: 1
	// (add) Token: 0x060000BF RID: 191 RVA: 0x00005390 File Offset: 0x00003590
	// (remove) Token: 0x060000C0 RID: 192 RVA: 0x000053C8 File Offset: 0x000035C8
	public event Action OnBuildingRemoved;

	// Token: 0x060000C1 RID: 193 RVA: 0x000053FD File Offset: 0x000035FD
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x060000C2 RID: 194 RVA: 0x00005400 File Offset: 0x00003600
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x060000C3 RID: 195 RVA: 0x00005408 File Offset: 0x00003608
	public string GetObjectName()
	{
		return this.Definition.Name;
	}

	// Token: 0x060000C4 RID: 196 RVA: 0x00005415 File Offset: 0x00003615
	private void Awake()
	{
		this._saveDataProvider = base.GetComponentInChildren<ICustomSaveDataProvider>(true);
		this._modularBuildingSupports = base.GetComponentsInChildren<BaseModularSupports>().ToList<BaseModularSupports>();
	}

	// Token: 0x060000C5 RID: 197 RVA: 0x00005438 File Offset: 0x00003638
	public void Start()
	{
		if (this.IsGhost)
		{
			if (this.ExtraGhostRenderers != null)
			{
				this.ExtraGhostRenderers.SetActive(true);
			}
			return;
		}
		if (this.ExtraGhostRenderers != null)
		{
			this.ExtraGhostRenderers.SetActive(false);
		}
		this._interactions.Add(Singleton<BuildingManager>.Instance.InteractionPack);
		this._interactions.Add(Singleton<BuildingManager>.Instance.InteractionTake);
		this.UpdateSupportsAbove(false);
		if (this.Definition == null)
		{
			Debug.LogError("BuildingObject " + base.name + " is missing Building Definition!");
		}
		if (this.SavableObjectID == SavableObjectID.INVALID)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"BuildingObject ",
				base.name,
				" (",
				this.Definition.Name,
				") has invalid SavableObjectID!"
			}));
		}
		PhysicsUtils.SetLayerRecursively(this.PhysicalColliderObject, LayerMask.NameToLayer("BuildingObject"));
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x00005538 File Offset: 0x00003738
	public void Interact(Interaction selectedInteraction)
	{
		if (this.IsGhost)
		{
			return;
		}
		string name = selectedInteraction.Name;
		if (name == "Take")
		{
			this.TryTakeOrPack();
			return;
		}
		if (!(name == "Pack"))
		{
			return;
		}
		this.Pack();
	}

	// Token: 0x060000C7 RID: 199 RVA: 0x00005580 File Offset: 0x00003780
	public void EnableBuildingSupports(bool enabled)
	{
		this.BuildingSupportsEnabled = enabled;
		foreach (BaseModularSupports baseModularSupports in this._modularBuildingSupports)
		{
			baseModularSupports.RespawnSupports(false);
		}
	}

	// Token: 0x060000C8 RID: 200 RVA: 0x000055D8 File Offset: 0x000037D8
	public void TryTakeOrPack()
	{
		if (this.IsGhost)
		{
			return;
		}
		if (!this.TryAddToInventory())
		{
			this.Pack();
		}
	}

	// Token: 0x060000C9 RID: 201 RVA: 0x000055F4 File Offset: 0x000037F4
	public bool TryAddToInventory()
	{
		if (base.gameObject == null)
		{
			return false;
		}
		if (this.Definition == null)
		{
			Debug.LogWarning("Tried to pickup Crate with missing Building Definition!");
			return false;
		}
		ToolBuilder toolBuilder = Object.Instantiate<ToolBuilder>(Singleton<BuildingManager>.Instance.BuildingToolPrefab);
		toolBuilder.Definition = this.Definition;
		toolBuilder.Setup();
		if (Object.FindObjectOfType<PlayerInventory>().TryAddToInventory(toolBuilder, -1))
		{
			Action onBuildingRemoved = this.OnBuildingRemoved;
			if (onBuildingRemoved != null)
			{
				onBuildingRemoved();
			}
			Object.Destroy(base.gameObject, 0f);
			return true;
		}
		return false;
	}

	// Token: 0x060000CA RID: 202 RVA: 0x00005680 File Offset: 0x00003880
	public void Pack()
	{
		if (this.IsGhost)
		{
			return;
		}
		Vector3 vector = (this.BuildingCrateSpawnPoint ? this.BuildingCrateSpawnPoint.position : (base.transform.position + new Vector3(0f, 0.25f, 0f)));
		Quaternion quaternion = (this.BuildingCrateSpawnPoint ? this.BuildingCrateSpawnPoint.rotation : Quaternion.identity);
		BuildingCrate buildingCrate = Object.Instantiate<BuildingCrate>(this.Definition.PackedPrefab ? this.Definition.PackedPrefab : Singleton<BuildingManager>.Instance.BuildingCratePrefab, vector, quaternion);
		buildingCrate.Definition = this.Definition;
		Rigidbody component = buildingCrate.GetComponent<Rigidbody>();
		if (component != null)
		{
			float num = 0.5f;
			Vector3 vector2 = new Vector3(Random.Range(-num, num), Random.Range(0f, num) * 2f, Random.Range(-num, num));
			component.linearVelocity = vector2;
			float num2 = 1f;
			Vector3 vector3 = new Vector3(Random.Range(-num2, num2), Random.Range(-num2, num2), Random.Range(-num2, num2));
			component.angularVelocity = vector3;
		}
		Action onBuildingRemoved = this.OnBuildingRemoved;
		if (onBuildingRemoved != null)
		{
			onBuildingRemoved();
		}
		Object.Destroy(base.gameObject, 0f);
	}

	// Token: 0x060000CB RID: 203 RVA: 0x000057CB File Offset: 0x000039CB
	private void OnDestroy()
	{
		if (this.IsGhost)
		{
			return;
		}
		this.UpdateSupportsAbove(true);
	}

	// Token: 0x060000CC RID: 204 RVA: 0x000057DD File Offset: 0x000039DD
	public virtual bool CanHaveBuildingSupports()
	{
		return this._modularBuildingSupports.Count > 0;
	}

	// Token: 0x060000CD RID: 205 RVA: 0x000057F0 File Offset: 0x000039F0
	public void UpdateSupportsAbove(bool isDestroyingThis)
	{
		GameObject physicalColliderObject = this.PhysicalColliderObject;
		if (physicalColliderObject != null)
		{
			physicalColliderObject.SetActive(false);
		}
		if (this.BuildingPlacementColliderObject != null)
		{
			this.BuildingPlacementColliderObject.SetActive(false);
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.up, out raycastHit, 20f, Singleton<BuildingManager>.Instance.BuildingSupportsCollisionLayers))
		{
			ModularBuildingSupports componentInParent = raycastHit.collider.GetComponentInParent<ModularBuildingSupports>();
			if (componentInParent != null)
			{
				componentInParent.RespawnSupports(true);
			}
		}
		if (!isDestroyingThis)
		{
			GameObject physicalColliderObject2 = this.PhysicalColliderObject;
			if (physicalColliderObject2 != null)
			{
				physicalColliderObject2.SetActive(true);
			}
			if (this.BuildingPlacementColliderObject != null)
			{
				this.BuildingPlacementColliderObject.SetActive(true);
			}
		}
	}

	// Token: 0x060000CE RID: 206 RVA: 0x000058A2 File Offset: 0x00003AA2
	public bool ShouldBeSaved()
	{
		return !this.IsGhost;
	}

	// Token: 0x060000CF RID: 207 RVA: 0x000058AD File Offset: 0x00003AAD
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x060000D0 RID: 208 RVA: 0x000058BC File Offset: 0x00003ABC
	public Vector3 GetRotation()
	{
		return MathExtensions.RoundVector3(base.transform.rotation.eulerAngles, 2);
	}

	// Token: 0x060000D1 RID: 209 RVA: 0x000058E2 File Offset: 0x00003AE2
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x1700000E RID: 14
	// (get) Token: 0x060000D2 RID: 210 RVA: 0x000058EA File Offset: 0x00003AEA
	// (set) Token: 0x060000D3 RID: 211 RVA: 0x000058F2 File Offset: 0x00003AF2
	public bool HasBeenSaved { get; set; }

	// Token: 0x060000D4 RID: 212 RVA: 0x000058FB File Offset: 0x00003AFB
	public virtual void LoadFromSave(string json)
	{
		if (this._saveDataProvider != null)
		{
			this._saveDataProvider.LoadFromSave(json);
		}
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x00005911 File Offset: 0x00003B11
	public virtual string GetCustomSaveData()
	{
		if (this._saveDataProvider != null)
		{
			return this._saveDataProvider.GetCustomSaveData();
		}
		return null;
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x00005928 File Offset: 0x00003B28
	public virtual bool GetBuildingSupportsEnabled()
	{
		return this.BuildingSupportsEnabled;
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x00005930 File Offset: 0x00003B30
	public virtual void LoadBuildingSaveData(BuildingObjectEntry buildingObjectEntry)
	{
		this.BuildingSupportsEnabled = buildingObjectEntry.BuildingSupportsEnable;
	}

	// Token: 0x040000B7 RID: 183
	public SavableObjectID SavableObjectID;

	// Token: 0x040000B8 RID: 184
	public BuildingInventoryDefinition Definition;

	// Token: 0x040000B9 RID: 185
	public Vector3 BuildModePlacementOffset;

	// Token: 0x040000BA RID: 186
	[SerializeField]
	private List<Interaction> _interactions;

	// Token: 0x040000BB RID: 187
	public Transform BuildingCrateSpawnPoint;

	// Token: 0x040000BC RID: 188
	public bool RequiresFlatGround;

	// Token: 0x040000BD RID: 189
	public PlacementNodeRequirement PlacementNodeRequirement;

	// Token: 0x040000BE RID: 190
	public SupportType SupportType;

	// Token: 0x040000BF RID: 191
	[FormerlySerializedAs("ColliderObject")]
	public GameObject PhysicalColliderObject;

	// Token: 0x040000C0 RID: 192
	public GameObject BuildingPlacementColliderObject;

	// Token: 0x040000C1 RID: 193
	public GameObject ExtraGhostRenderers;

	// Token: 0x040000C2 RID: 194
	public List<Transform> ConveyorInputSnapPositions;

	// Token: 0x040000C3 RID: 195
	public List<Transform> ConveyorOutputSnapPositions;

	// Token: 0x040000C4 RID: 196
	public bool RotatingShouldMirrorWhenSnapped;

	// Token: 0x040000C5 RID: 197
	public List<GameObject> DontDestroyWhenPreviewingModel;

	// Token: 0x040000C6 RID: 198
	public bool BuildingSupportsEnabled = true;

	// Token: 0x040000C8 RID: 200
	[HideInInspector]
	public bool IsGhost;

	// Token: 0x040000C9 RID: 201
	private ICustomSaveDataProvider _saveDataProvider;

	// Token: 0x040000CA RID: 202
	private List<BaseModularSupports> _modularBuildingSupports = new List<BaseModularSupports>();
}
