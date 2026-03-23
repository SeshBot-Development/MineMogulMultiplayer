using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007C RID: 124
public class OreSled : BaseSellableItem, IInteractable, ISaveLoadableObject, IIconItem
{
	// Token: 0x06000343 RID: 835 RVA: 0x00010CD2 File Offset: 0x0000EED2
	public virtual Sprite GetIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(this.ProgrammerInventoryIcon != null))
			{
				return this.InventoryIcon;
			}
			return this.ProgrammerInventoryIcon;
		}
		else
		{
			if (!(this.InventoryIcon != null))
			{
				return this.ProgrammerInventoryIcon;
			}
			return this.InventoryIcon;
		}
	}

	// Token: 0x06000344 RID: 836 RVA: 0x00010D12 File Offset: 0x0000EF12
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x06000345 RID: 837 RVA: 0x00010D15 File Offset: 0x0000EF15
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000346 RID: 838 RVA: 0x00010D1D File Offset: 0x0000EF1D
	public string GetObjectName()
	{
		return this.Name;
	}

	// Token: 0x06000347 RID: 839 RVA: 0x00010D25 File Offset: 0x0000EF25
	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Destroy")
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000348 RID: 840 RVA: 0x00010D44 File Offset: 0x0000EF44
	public bool ShouldBeSaved()
	{
		return true;
	}

	// Token: 0x06000349 RID: 841 RVA: 0x00010D47 File Offset: 0x0000EF47
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x0600034A RID: 842 RVA: 0x00010D54 File Offset: 0x0000EF54
	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	// Token: 0x0600034B RID: 843 RVA: 0x00010D74 File Offset: 0x0000EF74
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x0600034C RID: 844 RVA: 0x00010D7C File Offset: 0x0000EF7C
	// (set) Token: 0x0600034D RID: 845 RVA: 0x00010D84 File Offset: 0x0000EF84
	public bool HasBeenSaved { get; set; }

	// Token: 0x0600034E RID: 846 RVA: 0x00010D8D File Offset: 0x0000EF8D
	public virtual void LoadFromSave(string json)
	{
	}

	// Token: 0x0600034F RID: 847 RVA: 0x00010D8F File Offset: 0x0000EF8F
	public virtual string GetCustomSaveData()
	{
		return null;
	}

	// Token: 0x0400034C RID: 844
	public string Name;

	// Token: 0x0400034D RID: 845
	public SavableObjectID SavableObjectID;

	// Token: 0x0400034E RID: 846
	public Sprite ProgrammerInventoryIcon;

	// Token: 0x0400034F RID: 847
	public Sprite InventoryIcon;

	// Token: 0x04000350 RID: 848
	[SerializeField]
	private List<Interaction> _interactions;
}
