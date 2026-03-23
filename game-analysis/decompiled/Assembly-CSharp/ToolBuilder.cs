using System;
using UnityEngine;

// Token: 0x020000EA RID: 234
public class ToolBuilder : BaseHeldTool
{
	// Token: 0x0600063C RID: 1596 RVA: 0x000208D8 File Offset: 0x0001EAD8
	public override string GetControlsText()
	{
		string text = string.Concat(new string[]
		{
			"Drop - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool),
			"\nPlace Building - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack),
			"\nRotate - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.RotateObject)
		});
		if (this.Definition.BuildingPrefabs.Count > 1)
		{
			text = string.Concat(new string[]
			{
				text,
				"\n",
				this.Definition.QButtonFunction,
				" - ",
				Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.MirrorObject)
			});
		}
		return text;
	}

	// Token: 0x0600063D RID: 1597 RVA: 0x00020988 File Offset: 0x0001EB88
	public void Setup()
	{
		if (this.Definition != null)
		{
			this.Name = this.Definition.Name;
			this.Description = this.Definition.Description;
			this.CurrentPrefabIndex = 0;
			this.MaxAmount = this.Definition.MaxInventoryStackSize;
			return;
		}
		Debug.LogError("ToolBuilder doesn't have a Definition, destroying! " + base.name + "\n(This save file is likely from a newer version of the game, or requires mods which are not installed)");
		Object.Destroy(this);
	}

	// Token: 0x0600063E RID: 1598 RVA: 0x000209FE File Offset: 0x0001EBFE
	private BuildingObject GetSelectedPrefab()
	{
		return this.Definition.BuildingPrefabs[this.CurrentPrefabIndex];
	}

	// Token: 0x0600063F RID: 1599 RVA: 0x00020A16 File Offset: 0x0001EC16
	public override Sprite GetIcon()
	{
		return this.Definition.GetIcon();
	}

	// Token: 0x06000640 RID: 1600 RVA: 0x00020A23 File Offset: 0x0001EC23
	private void Start()
	{
		this.Setup();
	}

	// Token: 0x06000641 RID: 1601 RVA: 0x00020A2C File Offset: 0x0001EC2C
	private void Update()
	{
		if (this.Owner == null)
		{
			return;
		}
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 buildPosition = this.GetBuildPosition(componentInChildren);
		Vector3Int closestGridPosition = Singleton<BuildingManager>.Instance.GetClosestGridPosition(buildPosition);
		Singleton<BuildingManager>.Instance.UpdateGhostObject(closestGridPosition, this.GetSelectedPrefab(), this.CurrentRotation, this);
		foreach (BuildingPlacementNode buildingPlacementNode in BuildingPlacementNode.All)
		{
			buildingPlacementNode.ShowGhost(true, this.GetSelectedPrefab().PlacementNodeRequirement);
		}
	}

	// Token: 0x06000642 RID: 1602 RVA: 0x00020AD8 File Offset: 0x0001ECD8
	public override void QButtonPressed()
	{
		this.CycleAlternateModels();
	}

	// Token: 0x06000643 RID: 1603 RVA: 0x00020AE0 File Offset: 0x0001ECE0
	public bool IsUsingMirroredVersion()
	{
		return this.CurrentPrefabIndex == 1;
	}

	// Token: 0x06000644 RID: 1604 RVA: 0x00020AEB File Offset: 0x0001ECEB
	public void CycleAlternateModels()
	{
		this.CurrentPrefabIndex++;
		if (this.CurrentPrefabIndex >= this.Definition.BuildingPrefabs.Count)
		{
			this.CurrentPrefabIndex = 0;
		}
		Singleton<BuildingManager>.Instance.CleanUpGhostObject();
	}

	// Token: 0x06000645 RID: 1605 RVA: 0x00020B24 File Offset: 0x0001ED24
	public void MirrorObject()
	{
		if (this.Definition.BuildingPrefabs.Count <= 1)
		{
			return;
		}
		if (this.CurrentPrefabIndex == 1)
		{
			this.CurrentPrefabIndex = 0;
		}
		else
		{
			this.CurrentPrefabIndex = 1;
		}
		Singleton<BuildingManager>.Instance.CleanUpGhostObject();
	}

	// Token: 0x06000646 RID: 1606 RVA: 0x00020B60 File Offset: 0x0001ED60
	public override void Reload()
	{
		if (this.Definition.GetMainPrefab().RotatingShouldMirrorWhenSnapped && Singleton<BuildingManager>.Instance.CurrentObjectIsSnapped)
		{
			this.MirrorObject();
			return;
		}
		int num = 90;
		bool useReverseRotationDirection = this.Definition.UseReverseRotationDirection;
		this.CurrentRotation *= Quaternion.Euler(0f, (float)num, 0f);
		Singleton<BuildingManager>.Instance.CurrentObjectIsSnapped = false;
	}

	// Token: 0x06000647 RID: 1607 RVA: 0x00020BD0 File Offset: 0x0001EDD0
	public override void PrimaryFire()
	{
		if (this.Owner == null)
		{
			return;
		}
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 buildPosition = this.GetBuildPosition(componentInChildren);
		Vector3Int closestGridPosition = Singleton<BuildingManager>.Instance.GetClosestGridPosition(buildPosition);
		BuildingPlacementNode buildingPlacementNode;
		if (Singleton<BuildingManager>.Instance.CanPlaceObject(closestGridPosition, this.GetSelectedPrefab(), this.CurrentRotation, this.GetSelectedPrefab().RequiresFlatGround, this.Definition.CanBePlacedInTerrain, this.GetSelectedPrefab().PlacementNodeRequirement, out buildingPlacementNode, this) == CanPlaceBuilding.Valid)
		{
			BuildingObject buildingObject = Object.Instantiate<BuildingObject>(this.GetSelectedPrefab(), Singleton<BuildingManager>.Instance.GhostObjectTransform.position, Singleton<BuildingManager>.Instance.GhostObjectTransform.rotation);
			if (buildingPlacementNode != null)
			{
				buildingPlacementNode.AttachBuilding(buildingObject);
			}
			if (!Singleton<DebugManager>.Instance.UnlimitedBuilding)
			{
				this.Quantity--;
				if (this.Quantity <= 0)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
	}

	// Token: 0x06000648 RID: 1608 RVA: 0x00020CC0 File Offset: 0x0001EEC0
	public override void DropItem()
	{
		if (this.Definition != null)
		{
			Transform transform = Object.FindObjectOfType<PlayerController>().PlayerCamera.transform;
			BuildingCrate buildingCrate = Object.Instantiate<BuildingCrate>(this.Definition.PackedPrefab ? this.Definition.PackedPrefab : Singleton<BuildingManager>.Instance.BuildingCratePrefab, transform.position + transform.forward * 0.5f, transform.rotation);
			if (buildingCrate != null)
			{
				buildingCrate.Definition = this.Definition;
				buildingCrate.Quantity = this.Quantity;
				Rigidbody component = buildingCrate.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.linearVelocity = transform.forward * 5f;
				}
			}
		}
		this.Quantity = 0;
		if (this.Quantity < 1)
		{
			Object.FindObjectOfType<PlayerInventory>().RemoveFromInventory(this, 1);
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000649 RID: 1609 RVA: 0x00020DAC File Offset: 0x0001EFAC
	private Vector3 GetBuildPosition(Camera playerCamera)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, this.UseRange, Singleton<BuildingManager>.Instance.BuildingPlacementRaycastLayer))
		{
			return raycastHit.point;
		}
		return playerCamera.transform.position + playerCamera.transform.forward * this.UseRange;
	}

	// Token: 0x0600064A RID: 1610 RVA: 0x00020E1C File Offset: 0x0001F01C
	protected override void OnDisable()
	{
		base.OnDisable();
		foreach (BuildingPlacementNode buildingPlacementNode in BuildingPlacementNode.All)
		{
			buildingPlacementNode.ShowGhost(false, PlacementNodeRequirement.None);
		}
		Singleton<UIManager>.Instance.HideBuildingInfo();
		Singleton<BuildingManager>.Instance.CleanUpGhostObject();
	}

	// Token: 0x0600064B RID: 1611 RVA: 0x00020E88 File Offset: 0x0001F088
	public override void LoadFromSave(string json)
	{
		ToolBuilderSaveData toolBuilderSaveData = JsonUtility.FromJson<ToolBuilderSaveData>(json);
		if (toolBuilderSaveData == null)
		{
			toolBuilderSaveData = new ToolBuilderSaveData();
		}
		this.Definition = Singleton<SavingLoadingManager>.Instance.GetBuildingInventoryDefinition(toolBuilderSaveData.BuildObjectID);
		this.Quantity = toolBuilderSaveData.Quantity;
		if (toolBuilderSaveData.IsInPlayerInventory)
		{
			base.StartCoroutine(base.WaitThenAddToInventory(toolBuilderSaveData.InventorySlotIndex));
		}
	}

	// Token: 0x0600064C RID: 1612 RVA: 0x00020EE4 File Offset: 0x0001F0E4
	public override string GetCustomSaveData()
	{
		ToolBuilderSaveData toolBuilderSaveData = new ToolBuilderSaveData
		{
			IsInPlayerInventory = (this.Owner != null)
		};
		toolBuilderSaveData.Quantity = this.Quantity;
		toolBuilderSaveData.BuildObjectID = this.Definition.GetMainPrefab().SavableObjectID;
		if (toolBuilderSaveData.IsInPlayerInventory)
		{
			toolBuilderSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		return JsonUtility.ToJson(toolBuilderSaveData);
	}

	// Token: 0x04000784 RID: 1924
	public float UseRange = 3f;

	// Token: 0x04000785 RID: 1925
	public BuildingInventoryDefinition Definition;

	// Token: 0x04000786 RID: 1926
	public int CurrentPrefabIndex;

	// Token: 0x04000787 RID: 1927
	public Quaternion CurrentRotation = Quaternion.identity;
}
