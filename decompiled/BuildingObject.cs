using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingObject : MonoBehaviour, IInteractable, ISaveLoadableBuildingObject, ISaveLoadableObject
{
	public SavableObjectID SavableObjectID;

	public BuildingInventoryDefinition Definition;

	public Vector3 BuildModePlacementOffset;

	[SerializeField]
	private List<Interaction> _interactions;

	public Transform BuildingCrateSpawnPoint;

	public bool RequiresFlatGround;

	public PlacementNodeRequirement PlacementNodeRequirement;

	public SupportType SupportType;

	[FormerlySerializedAs("ColliderObject")]
	public GameObject PhysicalColliderObject;

	public GameObject BuildingPlacementColliderObject;

	public GameObject ExtraGhostRenderers;

	public List<Transform> ConveyorInputSnapPositions;

	public List<Transform> ConveyorOutputSnapPositions;

	public bool RotatingShouldMirrorWhenSnapped;

	public List<GameObject> DontDestroyWhenPreviewingModel;

	public bool BuildingSupportsEnabled = true;

	[HideInInspector]
	public bool IsGhost;

	private ICustomSaveDataProvider _saveDataProvider;

	private List<BaseModularSupports> _modularBuildingSupports = new List<BaseModularSupports>();

	public bool HasBeenSaved { get; set; }

	public event Action OnBuildingRemoved;

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public string GetObjectName()
	{
		return Definition.Name;
	}

	private void Awake()
	{
		_saveDataProvider = GetComponentInChildren<ICustomSaveDataProvider>(includeInactive: true);
		_modularBuildingSupports = GetComponentsInChildren<BaseModularSupports>().ToList();
	}

	public void Start()
	{
		if (IsGhost)
		{
			if (ExtraGhostRenderers != null)
			{
				ExtraGhostRenderers.SetActive(value: true);
			}
			return;
		}
		if (ExtraGhostRenderers != null)
		{
			ExtraGhostRenderers.SetActive(value: false);
		}
		_interactions.Add(Singleton<BuildingManager>.Instance.InteractionPack);
		_interactions.Add(Singleton<BuildingManager>.Instance.InteractionTake);
		UpdateSupportsAbove(isDestroyingThis: false);
		if (Definition == null)
		{
			Debug.LogError("BuildingObject " + base.name + " is missing Building Definition!");
		}
		if (SavableObjectID == SavableObjectID.INVALID)
		{
			Debug.LogError("BuildingObject " + base.name + " (" + Definition.Name + ") has invalid SavableObjectID!");
		}
		PhysicsUtils.SetLayerRecursively(PhysicalColliderObject, LayerMask.NameToLayer("BuildingObject"));
	}

	public void Interact(Interaction selectedInteraction)
	{
		if (IsGhost)
		{
			return;
		}
		string text = selectedInteraction.Name;
		if (!(text == "Take"))
		{
			if (text == "Pack")
			{
				Pack();
			}
		}
		else
		{
			TryTakeOrPack();
		}
	}

	public void EnableBuildingSupports(bool enabled)
	{
		BuildingSupportsEnabled = enabled;
		foreach (BaseModularSupports modularBuildingSupport in _modularBuildingSupports)
		{
			modularBuildingSupport.RespawnSupports();
		}
	}

	public void TryTakeOrPack()
	{
		if (!IsGhost && !TryAddToInventory())
		{
			Pack();
		}
	}

	public bool TryAddToInventory()
	{
		if (base.gameObject == null)
		{
			return false;
		}
		if (Definition == null)
		{
			Debug.LogWarning("Tried to pickup Crate with missing Building Definition!");
			return false;
		}
		ToolBuilder toolBuilder = UnityEngine.Object.Instantiate(Singleton<BuildingManager>.Instance.BuildingToolPrefab);
		toolBuilder.Definition = Definition;
		toolBuilder.Setup();
		if (UnityEngine.Object.FindObjectOfType<PlayerInventory>().TryAddToInventory(toolBuilder))
		{
			this.OnBuildingRemoved?.Invoke();
			UnityEngine.Object.Destroy(base.gameObject, 0f);
			return true;
		}
		return false;
	}

	public void Pack()
	{
		if (!IsGhost)
		{
			Vector3 position = (BuildingCrateSpawnPoint ? BuildingCrateSpawnPoint.position : (base.transform.position + new Vector3(0f, 0.25f, 0f)));
			Quaternion rotation = (BuildingCrateSpawnPoint ? BuildingCrateSpawnPoint.rotation : Quaternion.identity);
			BuildingCrate buildingCrate = UnityEngine.Object.Instantiate(Definition.PackedPrefab ? Definition.PackedPrefab : Singleton<BuildingManager>.Instance.BuildingCratePrefab, position, rotation);
			buildingCrate.Definition = Definition;
			Rigidbody component = buildingCrate.GetComponent<Rigidbody>();
			if (component != null)
			{
				float num = 0.5f;
				Vector3 linearVelocity = new Vector3(UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f, num) * 2f, UnityEngine.Random.Range(0f - num, num));
				component.linearVelocity = linearVelocity;
				float num2 = 1f;
				Vector3 angularVelocity = new Vector3(UnityEngine.Random.Range(0f - num2, num2), UnityEngine.Random.Range(0f - num2, num2), UnityEngine.Random.Range(0f - num2, num2));
				component.angularVelocity = angularVelocity;
			}
			this.OnBuildingRemoved?.Invoke();
			UnityEngine.Object.Destroy(base.gameObject, 0f);
		}
	}

	private void OnDestroy()
	{
		if (!IsGhost)
		{
			UpdateSupportsAbove(isDestroyingThis: true);
		}
	}

	public virtual bool CanHaveBuildingSupports()
	{
		return _modularBuildingSupports.Count > 0;
	}

	public void UpdateSupportsAbove(bool isDestroyingThis)
	{
		PhysicalColliderObject?.SetActive(value: false);
		if (BuildingPlacementColliderObject != null)
		{
			BuildingPlacementColliderObject.SetActive(value: false);
		}
		if (Physics.Raycast(base.transform.position, Vector3.up, out var hitInfo, 20f, Singleton<BuildingManager>.Instance.BuildingSupportsCollisionLayers))
		{
			ModularBuildingSupports componentInParent = hitInfo.collider.GetComponentInParent<ModularBuildingSupports>();
			if (componentInParent != null)
			{
				componentInParent.RespawnSupports(RespawnNextFrame: true);
			}
		}
		if (!isDestroyingThis)
		{
			PhysicalColliderObject?.SetActive(value: true);
			if (BuildingPlacementColliderObject != null)
			{
				BuildingPlacementColliderObject.SetActive(value: true);
			}
		}
	}

	public bool ShouldBeSaved()
	{
		return !IsGhost;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public Vector3 GetRotation()
	{
		return MathExtensions.RoundVector3(base.transform.rotation.eulerAngles);
	}

	public SavableObjectID GetSavableObjectID()
	{
		return SavableObjectID;
	}

	public virtual void LoadFromSave(string json)
	{
		if (_saveDataProvider != null)
		{
			_saveDataProvider.LoadFromSave(json);
		}
	}

	public virtual string GetCustomSaveData()
	{
		if (_saveDataProvider != null)
		{
			return _saveDataProvider.GetCustomSaveData();
		}
		return null;
	}

	public virtual bool GetBuildingSupportsEnabled()
	{
		return BuildingSupportsEnabled;
	}

	public virtual void LoadBuildingSaveData(BuildingObjectEntry buildingObjectEntry)
	{
		BuildingSupportsEnabled = buildingObjectEntry.BuildingSupportsEnable;
	}
}
