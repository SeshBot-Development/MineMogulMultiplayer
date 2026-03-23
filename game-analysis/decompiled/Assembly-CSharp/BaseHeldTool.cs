using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000009 RID: 9
public class BaseHeldTool : BaseSellableItem, IInteractable, ISaveLoadableObject, IIconItem
{
	// Token: 0x06000035 RID: 53 RVA: 0x00002D38 File Offset: 0x00000F38
	public virtual Sprite GetIcon()
	{
		if (Singleton<SettingsManager>.Instance != null && Singleton<SettingsManager>.Instance.UseProgrammerIcons)
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

	// Token: 0x06000036 RID: 54 RVA: 0x00002D98 File Offset: 0x00000F98
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.transform.parent != null)
		{
			PlayerController component = base.transform.parent.GetComponent<PlayerController>();
			if (component != null)
			{
				this.Owner = component;
			}
		}
		if (this.Owner == null)
		{
			this.HideViewModel(true);
			this.HideWorldModel(false);
			return;
		}
		this.HideWorldModel(true);
		this.HideViewModel(false);
		if (base.transform.parent == null || base.transform.parent != this.Owner.ViewModelContainer)
		{
			base.transform.position = this.Owner.ViewModelContainer.position;
			base.transform.rotation = this.Owner.ViewModelContainer.rotation;
			base.transform.parent = this.Owner.ViewModelContainer;
		}
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00002E86 File Offset: 0x00001086
	public virtual string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool);
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00002E9E File Offset: 0x0000109E
	public virtual void PrimaryFire()
	{
		if (this.ViewModelAnimator != null)
		{
			this.ViewModelAnimator.Play("Attack1", -1, 0f);
		}
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00002EC4 File Offset: 0x000010C4
	public virtual void PrimaryFireHeld()
	{
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00002EC6 File Offset: 0x000010C6
	public virtual void SecondaryFire()
	{
	}

	// Token: 0x0600003B RID: 59 RVA: 0x00002EC8 File Offset: 0x000010C8
	public virtual void SecondaryFireHeld()
	{
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00002ECA File Offset: 0x000010CA
	public virtual void Reload()
	{
	}

	// Token: 0x0600003D RID: 61 RVA: 0x00002ECC File Offset: 0x000010CC
	public virtual void QButtonPressed()
	{
	}

	// Token: 0x0600003E RID: 62 RVA: 0x00002ED0 File Offset: 0x000010D0
	public virtual void DropItem()
	{
		base.gameObject.SetActive(true);
		Object.FindObjectOfType<PlayerInventory>().RemoveFromInventory(this, 1);
		this.HideWorldModel(false);
		this.HideViewModel(true);
		Rigidbody componentInChildren = base.GetComponentInChildren<Rigidbody>();
		if (componentInChildren != null)
		{
			base.transform.parent = null;
			Transform transform = Object.FindObjectOfType<PlayerController>().PlayerCamera.transform;
			componentInChildren.isKinematic = false;
			componentInChildren.transform.position = transform.position + transform.forward * 0.5f;
			componentInChildren.position = transform.position + transform.forward * 0.5f;
			componentInChildren.linearVelocity = transform.forward * 5f;
			componentInChildren.rotation = transform.rotation;
		}
		this.Owner = null;
	}

	// Token: 0x0600003F RID: 63 RVA: 0x00002FA9 File Offset: 0x000011A9
	public virtual void Equip()
	{
		this.HideWorldModel(true);
		this.HideViewModel(true);
	}

	// Token: 0x06000040 RID: 64 RVA: 0x00002FB9 File Offset: 0x000011B9
	public virtual void UnEquip()
	{
		this.HideViewModel(false);
	}

	// Token: 0x06000041 RID: 65 RVA: 0x00002FC2 File Offset: 0x000011C2
	public virtual void HideViewModel(bool hide = true)
	{
		if (this.ViewModel != null)
		{
			this.ViewModel.SetActive(!hide);
		}
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00002FE1 File Offset: 0x000011E1
	public virtual void HideWorldModel(bool hide = true)
	{
		if (this.WorldModel != null)
		{
			this.WorldModel.SetActive(!hide);
		}
	}

	// Token: 0x06000043 RID: 67 RVA: 0x00003000 File Offset: 0x00001200
	public bool ShouldUseInteractionWheel()
	{
		return this._shouldUseInteractionWheel;
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00003008 File Offset: 0x00001208
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x00003010 File Offset: 0x00001210
	public string GetObjectName()
	{
		return this.Name;
	}

	// Token: 0x06000046 RID: 70 RVA: 0x00003018 File Offset: 0x00001218
	public virtual void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Take")
		{
			this.TryAddToInventory(-1);
			return;
		}
		if (!(name == "Destroy"))
		{
			return;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000047 RID: 71 RVA: 0x0000305B File Offset: 0x0000125B
	public virtual bool TryAddToInventory(int slotIndex = -1)
	{
		return Object.FindObjectOfType<PlayerInventory>().TryAddToInventory(this, slotIndex);
	}

	// Token: 0x06000048 RID: 72 RVA: 0x00003069 File Offset: 0x00001269
	public bool ShouldBeSaved()
	{
		return true;
	}

	// Token: 0x06000049 RID: 73 RVA: 0x0000306C File Offset: 0x0000126C
	public Vector3 GetPosition()
	{
		if (this.WorldModel != null)
		{
			return this.WorldModel.transform.position;
		}
		return base.transform.position;
	}

	// Token: 0x0600004A RID: 74 RVA: 0x00003098 File Offset: 0x00001298
	public Vector3 GetRotation()
	{
		if (this.WorldModel != null)
		{
			return this.WorldModel.transform.rotation.eulerAngles;
		}
		return base.transform.rotation.eulerAngles;
	}

	// Token: 0x0600004B RID: 75 RVA: 0x000030DF File Offset: 0x000012DF
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x17000008 RID: 8
	// (get) Token: 0x0600004C RID: 76 RVA: 0x000030E7 File Offset: 0x000012E7
	// (set) Token: 0x0600004D RID: 77 RVA: 0x000030EF File Offset: 0x000012EF
	public bool HasBeenSaved { get; set; }

	// Token: 0x0600004E RID: 78 RVA: 0x000030F8 File Offset: 0x000012F8
	public virtual void LoadFromSave(string json)
	{
		BaseHeldToolSaveData baseHeldToolSaveData = JsonUtility.FromJson<BaseHeldToolSaveData>(json);
		if (baseHeldToolSaveData == null)
		{
			baseHeldToolSaveData = new BaseHeldToolSaveData();
		}
		if (baseHeldToolSaveData.IsInPlayerInventory)
		{
			base.StartCoroutine(this.WaitThenAddToInventory(baseHeldToolSaveData.InventorySlotIndex));
		}
	}

	// Token: 0x0600004F RID: 79 RVA: 0x00003130 File Offset: 0x00001330
	public virtual string GetCustomSaveData()
	{
		BaseHeldToolSaveData baseHeldToolSaveData = new BaseHeldToolSaveData
		{
			IsInPlayerInventory = (this.Owner != null)
		};
		if (baseHeldToolSaveData.IsInPlayerInventory)
		{
			baseHeldToolSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		return JsonUtility.ToJson(baseHeldToolSaveData);
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00003174 File Offset: 0x00001374
	protected IEnumerator WaitThenAddToInventory(int index = -1)
	{
		yield return new WaitForFixedUpdate();
		if (base.gameObject != null)
		{
			this.TryAddToInventory(index);
		}
		yield break;
	}

	// Token: 0x0400003B RID: 59
	public SavableObjectID SavableObjectID;

	// Token: 0x0400003C RID: 60
	public string Name = "test";

	// Token: 0x0400003D RID: 61
	[TextArea]
	public string Description = "description";

	// Token: 0x0400003E RID: 62
	[FormerlySerializedAs("Icon")]
	public Sprite ProgrammerInventoryIcon;

	// Token: 0x0400003F RID: 63
	public Sprite InventoryIcon;

	// Token: 0x04000040 RID: 64
	public int Quantity = 1;

	// Token: 0x04000041 RID: 65
	public int MaxAmount = 1;

	// Token: 0x04000042 RID: 66
	public bool EquipWhenPickedUp;

	// Token: 0x04000043 RID: 67
	[SerializeField]
	private bool _shouldUseInteractionWheel = true;

	// Token: 0x04000044 RID: 68
	public PlayerController Owner;

	// Token: 0x04000045 RID: 69
	public GameObject WorldModel;

	// Token: 0x04000046 RID: 70
	public GameObject ViewModel;

	// Token: 0x04000047 RID: 71
	public Animator ViewModelAnimator;

	// Token: 0x04000048 RID: 72
	[SerializeField]
	private List<Interaction> _interactions;
}
