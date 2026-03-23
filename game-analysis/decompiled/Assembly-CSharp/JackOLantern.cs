using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000060 RID: 96
public class JackOLantern : BaseSellableItem, IInteractable, ISaveLoadableObject, IIconItem
{
	// Token: 0x0600026F RID: 623 RVA: 0x0000C4C8 File Offset: 0x0000A6C8
	public void Scare()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.InteractSound, base.transform.position, 1f, 1f, true, false);
		Rigidbody component = base.GetComponent<Rigidbody>();
		if (component != null)
		{
			Vector3 vector = Vector3.up + -base.transform.forward + Random.insideUnitSphere * 0.2f;
			float num = 2f * this.ExplosionMultiplier;
			float num2 = 6f * this.ExplosionMultiplier;
			component.AddForce(vector * num, ForceMode.Impulse);
			component.AddTorque(vector * num2, ForceMode.Impulse);
		}
	}

	// Token: 0x06000270 RID: 624 RVA: 0x0000C571 File Offset: 0x0000A771
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

	// Token: 0x06000271 RID: 625 RVA: 0x0000C5B1 File Offset: 0x0000A7B1
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x06000272 RID: 626 RVA: 0x0000C5B4 File Offset: 0x0000A7B4
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000273 RID: 627 RVA: 0x0000C5BC File Offset: 0x0000A7BC
	public string GetObjectName()
	{
		return this.Name;
	}

	// Token: 0x06000274 RID: 628 RVA: 0x0000C5C4 File Offset: 0x0000A7C4
	public void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		this.Scare();
	}

	// Token: 0x06000275 RID: 629 RVA: 0x0000C5D3 File Offset: 0x0000A7D3
	public bool ShouldBeSaved()
	{
		return true;
	}

	// Token: 0x06000276 RID: 630 RVA: 0x0000C5D6 File Offset: 0x0000A7D6
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x06000277 RID: 631 RVA: 0x0000C5E4 File Offset: 0x0000A7E4
	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	// Token: 0x06000278 RID: 632 RVA: 0x0000C604 File Offset: 0x0000A804
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x17000014 RID: 20
	// (get) Token: 0x06000279 RID: 633 RVA: 0x0000C60C File Offset: 0x0000A80C
	// (set) Token: 0x0600027A RID: 634 RVA: 0x0000C614 File Offset: 0x0000A814
	public bool HasBeenSaved { get; set; }

	// Token: 0x0600027B RID: 635 RVA: 0x0000C61D File Offset: 0x0000A81D
	public virtual void LoadFromSave(string json)
	{
	}

	// Token: 0x0600027C RID: 636 RVA: 0x0000C61F File Offset: 0x0000A81F
	public virtual string GetCustomSaveData()
	{
		return null;
	}

	// Token: 0x04000243 RID: 579
	public string Name;

	// Token: 0x04000244 RID: 580
	public SavableObjectID SavableObjectID;

	// Token: 0x04000245 RID: 581
	public SoundDefinition InteractSound;

	// Token: 0x04000246 RID: 582
	public float ExplosionMultiplier = 1f;

	// Token: 0x04000247 RID: 583
	public Sprite ProgrammerInventoryIcon;

	// Token: 0x04000248 RID: 584
	public Sprite InventoryIcon;

	// Token: 0x04000249 RID: 585
	[SerializeField]
	private List<Interaction> _interactions;
}
