using UnityEngine;

public class ToolBuilder : BaseHeldTool
{
	public float UseRange = 3f;

	public BuildingInventoryDefinition Definition;

	public int CurrentPrefabIndex;

	public Quaternion CurrentRotation = Quaternion.identity;

	public override string GetControlsText()
	{
		string text = "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nPlace Building - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack) + "\nRotate - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.RotateObject);
		if (Definition.BuildingPrefabs.Count > 1)
		{
			text = text + "\n" + Definition.QButtonFunction + " - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.MirrorObject);
		}
		return text;
	}

	public void Setup()
	{
		if (Definition != null)
		{
			Name = Definition.Name;
			Description = Definition.Description;
			CurrentPrefabIndex = 0;
			MaxAmount = Definition.MaxInventoryStackSize;
		}
		else
		{
			Debug.LogError("ToolBuilder doesn't have a Definition, destroying! " + base.name + "\n(This save file is likely from a newer version of the game, or requires mods which are not installed)");
			Object.Destroy(this);
		}
	}

	private BuildingObject GetSelectedPrefab()
	{
		return Definition.BuildingPrefabs[CurrentPrefabIndex];
	}

	public override Sprite GetIcon()
	{
		return Definition.GetIcon();
	}

	private void Start()
	{
		Setup();
	}

	private void Update()
	{
		if (Owner == null)
		{
			return;
		}
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 buildPosition = GetBuildPosition(componentInChildren);
		Vector3Int closestGridPosition = Singleton<BuildingManager>.Instance.GetClosestGridPosition(buildPosition);
		Singleton<BuildingManager>.Instance.UpdateGhostObject(closestGridPosition, GetSelectedPrefab(), CurrentRotation, this);
		foreach (BuildingPlacementNode item in BuildingPlacementNode.All)
		{
			item.ShowGhost(show: true, GetSelectedPrefab().PlacementNodeRequirement);
		}
	}

	public override void QButtonPressed()
	{
		CycleAlternateModels();
	}

	public bool IsUsingMirroredVersion()
	{
		return CurrentPrefabIndex == 1;
	}

	public void CycleAlternateModels()
	{
		CurrentPrefabIndex++;
		if (CurrentPrefabIndex >= Definition.BuildingPrefabs.Count)
		{
			CurrentPrefabIndex = 0;
		}
		Singleton<BuildingManager>.Instance.CleanUpGhostObject();
	}

	public void MirrorObject()
	{
		if (Definition.BuildingPrefabs.Count > 1)
		{
			if (CurrentPrefabIndex == 1)
			{
				CurrentPrefabIndex = 0;
			}
			else
			{
				CurrentPrefabIndex = 1;
			}
			Singleton<BuildingManager>.Instance.CleanUpGhostObject();
		}
	}

	public override void Reload()
	{
		if (Definition.GetMainPrefab().RotatingShouldMirrorWhenSnapped && Singleton<BuildingManager>.Instance.CurrentObjectIsSnapped)
		{
			MirrorObject();
			return;
		}
		int num = 90;
		_ = Definition.UseReverseRotationDirection;
		CurrentRotation *= Quaternion.Euler(0f, num, 0f);
		Singleton<BuildingManager>.Instance.CurrentObjectIsSnapped = false;
	}

	public override void PrimaryFire()
	{
		if (Owner == null)
		{
			return;
		}
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 buildPosition = GetBuildPosition(componentInChildren);
		Vector3Int closestGridPosition = Singleton<BuildingManager>.Instance.GetClosestGridPosition(buildPosition);
		if (Singleton<BuildingManager>.Instance.CanPlaceObject(closestGridPosition, GetSelectedPrefab(), CurrentRotation, GetSelectedPrefab().RequiresFlatGround, Definition.CanBePlacedInTerrain, GetSelectedPrefab().PlacementNodeRequirement, out var AttachedNode, this) != CanPlaceBuilding.Valid)
		{
			return;
		}
		BuildingObject attachedBuildingObject = Object.Instantiate(GetSelectedPrefab(), Singleton<BuildingManager>.Instance.GhostObjectTransform.position, Singleton<BuildingManager>.Instance.GhostObjectTransform.rotation);
		if (AttachedNode != null)
		{
			AttachedNode.AttachBuilding(attachedBuildingObject);
		}
		if (!Singleton<DebugManager>.Instance.UnlimitedBuilding)
		{
			Quantity--;
			if (Quantity <= 0)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public override void DropItem()
	{
		if (Definition != null)
		{
			Transform transform = Object.FindObjectOfType<PlayerController>().PlayerCamera.transform;
			BuildingCrate buildingCrate = Object.Instantiate(Definition.PackedPrefab ? Definition.PackedPrefab : Singleton<BuildingManager>.Instance.BuildingCratePrefab, transform.position + transform.forward * 0.5f, transform.rotation);
			if (buildingCrate != null)
			{
				buildingCrate.Definition = Definition;
				buildingCrate.Quantity = Quantity;
				Rigidbody component = buildingCrate.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.linearVelocity = transform.forward * 5f;
				}
			}
		}
		Quantity = 0;
		if (Quantity < 1)
		{
			Object.FindObjectOfType<PlayerInventory>().RemoveFromInventory(this);
			Object.Destroy(base.gameObject);
		}
	}

	private Vector3 GetBuildPosition(Camera playerCamera)
	{
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hitInfo, UseRange, Singleton<BuildingManager>.Instance.BuildingPlacementRaycastLayer))
		{
			return hitInfo.point;
		}
		return playerCamera.transform.position + playerCamera.transform.forward * UseRange;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		foreach (BuildingPlacementNode item in BuildingPlacementNode.All)
		{
			item.ShowGhost(show: false);
		}
		Singleton<UIManager>.Instance.HideBuildingInfo();
		Singleton<BuildingManager>.Instance.CleanUpGhostObject();
	}

	public override void LoadFromSave(string json)
	{
		ToolBuilderSaveData toolBuilderSaveData = JsonUtility.FromJson<ToolBuilderSaveData>(json);
		if (toolBuilderSaveData == null)
		{
			toolBuilderSaveData = new ToolBuilderSaveData();
		}
		Definition = Singleton<SavingLoadingManager>.Instance.GetBuildingInventoryDefinition(toolBuilderSaveData.BuildObjectID);
		Quantity = toolBuilderSaveData.Quantity;
		if (toolBuilderSaveData.IsInPlayerInventory)
		{
			StartCoroutine(WaitThenAddToInventory(toolBuilderSaveData.InventorySlotIndex));
		}
	}

	public override string GetCustomSaveData()
	{
		ToolBuilderSaveData toolBuilderSaveData = new ToolBuilderSaveData
		{
			IsInPlayerInventory = (Owner != null)
		};
		toolBuilderSaveData.Quantity = Quantity;
		toolBuilderSaveData.BuildObjectID = Definition.GetMainPrefab().SavableObjectID;
		if (toolBuilderSaveData.IsInPlayerInventory)
		{
			toolBuilderSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		return JsonUtility.ToJson(toolBuilderSaveData);
	}
}
