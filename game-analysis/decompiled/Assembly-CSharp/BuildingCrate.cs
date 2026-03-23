using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000013 RID: 19
public class BuildingCrate : BaseSellableItem, IInteractable, ISaveLoadableObject
{
	// Token: 0x06000097 RID: 151 RVA: 0x000042B0 File Offset: 0x000024B0
	private void Start()
	{
		if (!(this.Definition != null) || !(this.Definition.GetIcon() != null))
		{
			Canvas[] componentsInChildren = base.GetComponentsInChildren<Canvas>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
			return;
		}
		Image[] componentsInChildren2 = base.GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].sprite = this.Definition.GetIcon();
		}
		TMP_Text[] array;
		if (this.Quantity > 1)
		{
			array = base.GetComponentsInChildren<TMP_Text>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].text = "x" + this.Quantity.ToString();
			}
			return;
		}
		array = base.GetComponentsInChildren<TMP_Text>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000438C File Offset: 0x0000258C
	public void TryAddToInventory()
	{
		if (this.Definition == null)
		{
			Debug.LogWarning("Tried to pickup Crate with missing Building Definition!");
			return;
		}
		ToolBuilder toolBuilder = Object.Instantiate<ToolBuilder>(Singleton<BuildingManager>.Instance.BuildingToolPrefab);
		toolBuilder.Definition = this.Definition;
		toolBuilder.Quantity = this.Quantity;
		toolBuilder.Setup();
		if (Object.FindObjectOfType<PlayerInventory>().TryAddToInventory(toolBuilder, -1))
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000099 RID: 153 RVA: 0x000043F9 File Offset: 0x000025F9
	public override float GetSellValue()
	{
		return Singleton<EconomyManager>.Instance.GetPriceOfBuildingDefinition(this.Definition) * 0.9f * (float)this.Quantity;
	}

	// Token: 0x0600009A RID: 154 RVA: 0x00004419 File Offset: 0x00002619
	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	// Token: 0x0600009B RID: 155 RVA: 0x0000441C File Offset: 0x0000261C
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x0600009C RID: 156 RVA: 0x00004424 File Offset: 0x00002624
	public string GetObjectName()
	{
		return this.Definition.Name;
	}

	// Token: 0x0600009D RID: 157 RVA: 0x00004434 File Offset: 0x00002634
	public void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Take")
		{
			this.TryAddToInventory();
			return;
		}
		if (!(name == "Destroy"))
		{
			return;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00004475 File Offset: 0x00002675
	public bool ShouldBeSaved()
	{
		return true;
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00004478 File Offset: 0x00002678
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x00004488 File Offset: 0x00002688
	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x000044A8 File Offset: 0x000026A8
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x060000A2 RID: 162 RVA: 0x000044B0 File Offset: 0x000026B0
	// (set) Token: 0x060000A3 RID: 163 RVA: 0x000044B8 File Offset: 0x000026B8
	public bool HasBeenSaved { get; set; }

	// Token: 0x060000A4 RID: 164 RVA: 0x000044C4 File Offset: 0x000026C4
	public virtual void LoadFromSave(string json)
	{
		BuildingCrateSaveData buildingCrateSaveData = JsonUtility.FromJson<BuildingCrateSaveData>(json);
		if (buildingCrateSaveData == null)
		{
			buildingCrateSaveData = new BuildingCrateSaveData();
		}
		this.Definition = Singleton<SavingLoadingManager>.Instance.GetBuildingInventoryDefinition(buildingCrateSaveData.BuildObjectID);
		this.Quantity = buildingCrateSaveData.Quantity;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00004503 File Offset: 0x00002703
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new BuildingCrateSaveData
		{
			Quantity = this.Quantity,
			BuildObjectID = this.Definition.GetMainPrefab().SavableObjectID
		});
	}

	// Token: 0x04000088 RID: 136
	public SavableObjectID SavableObjectID;

	// Token: 0x04000089 RID: 137
	public int Quantity = 1;

	// Token: 0x0400008A RID: 138
	public BuildingInventoryDefinition Definition;

	// Token: 0x0400008B RID: 139
	[SerializeField]
	private List<Interaction> _interactions;
}
