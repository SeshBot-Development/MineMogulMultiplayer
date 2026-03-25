using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BaseHeldTool : BaseSellableItem, IInteractable, ISaveLoadableObject, IIconItem
{
	public SavableObjectID SavableObjectID;

	public string Name = "test";

	[TextArea]
	public string Description = "description";

	[FormerlySerializedAs("Icon")]
	public Sprite ProgrammerInventoryIcon;

	public Sprite InventoryIcon;

	public int Quantity = 1;

	public int MaxAmount = 1;

	public bool EquipWhenPickedUp;

	[SerializeField]
	private bool _shouldUseInteractionWheel = true;

	public PlayerController Owner;

	public GameObject WorldModel;

	public GameObject ViewModel;

	public Animator ViewModelAnimator;

	[SerializeField]
	private List<Interaction> _interactions;

	public bool HasBeenSaved { get; set; }

	public virtual Sprite GetIcon()
	{
		if (Singleton<SettingsManager>.Instance != null && Singleton<SettingsManager>.Instance.UseProgrammerIcons)
		{
			if (!(ProgrammerInventoryIcon != null))
			{
				return InventoryIcon;
			}
			return ProgrammerInventoryIcon;
		}
		if (!(InventoryIcon != null))
		{
			return ProgrammerInventoryIcon;
		}
		return InventoryIcon;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.transform.parent != null)
		{
			PlayerController component = base.transform.parent.GetComponent<PlayerController>();
			if (component != null)
			{
				Owner = component;
			}
		}
		if (Owner == null)
		{
			HideViewModel();
			HideWorldModel(hide: false);
			return;
		}
		HideWorldModel();
		HideViewModel(hide: false);
		if (base.transform.parent == null || base.transform.parent != Owner.ViewModelContainer)
		{
			base.transform.position = Owner.ViewModelContainer.position;
			base.transform.rotation = Owner.ViewModelContainer.rotation;
			base.transform.parent = Owner.ViewModelContainer;
		}
	}

	public virtual string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool);
	}

	public virtual void PrimaryFire()
	{
		if (ViewModelAnimator != null)
		{
			ViewModelAnimator.Play("Attack1", -1, 0f);
		}
	}

	public virtual void PrimaryFireHeld()
	{
	}

	public virtual void SecondaryFire()
	{
	}

	public virtual void SecondaryFireHeld()
	{
	}

	public virtual void Reload()
	{
	}

	public virtual void QButtonPressed()
	{
	}

	public virtual void DropItem()
	{
		base.gameObject.SetActive(value: true);
		Object.FindObjectOfType<PlayerInventory>().RemoveFromInventory(this);
		HideWorldModel(hide: false);
		HideViewModel();
		Rigidbody componentInChildren = GetComponentInChildren<Rigidbody>();
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
		Owner = null;
	}

	public virtual void Equip()
	{
		HideWorldModel();
		HideViewModel();
	}

	public virtual void UnEquip()
	{
		HideViewModel(hide: false);
	}

	public virtual void HideViewModel(bool hide = true)
	{
		if (ViewModel != null)
		{
			ViewModel.SetActive(!hide);
		}
	}

	public virtual void HideWorldModel(bool hide = true)
	{
		if (WorldModel != null)
		{
			WorldModel.SetActive(!hide);
		}
	}

	public bool ShouldUseInteractionWheel()
	{
		return _shouldUseInteractionWheel;
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public string GetObjectName()
	{
		return Name;
	}

	public virtual void Interact(Interaction selectedInteraction)
	{
		string text = selectedInteraction.Name;
		if (!(text == "Take"))
		{
			if (text == "Destroy")
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			TryAddToInventory();
		}
	}

	public virtual bool TryAddToInventory(int slotIndex = -1)
	{
		return Object.FindObjectOfType<PlayerInventory>().TryAddToInventory(this, slotIndex);
	}

	public bool ShouldBeSaved()
	{
		return true;
	}

	public Vector3 GetPosition()
	{
		if (WorldModel != null)
		{
			return WorldModel.transform.position;
		}
		return base.transform.position;
	}

	public Vector3 GetRotation()
	{
		if (WorldModel != null)
		{
			return WorldModel.transform.rotation.eulerAngles;
		}
		return base.transform.rotation.eulerAngles;
	}

	public SavableObjectID GetSavableObjectID()
	{
		return SavableObjectID;
	}

	public virtual void LoadFromSave(string json)
	{
		BaseHeldToolSaveData baseHeldToolSaveData = JsonUtility.FromJson<BaseHeldToolSaveData>(json);
		if (baseHeldToolSaveData == null)
		{
			baseHeldToolSaveData = new BaseHeldToolSaveData();
		}
		if (baseHeldToolSaveData.IsInPlayerInventory)
		{
			StartCoroutine(WaitThenAddToInventory(baseHeldToolSaveData.InventorySlotIndex));
		}
	}

	public virtual string GetCustomSaveData()
	{
		BaseHeldToolSaveData baseHeldToolSaveData = new BaseHeldToolSaveData
		{
			IsInPlayerInventory = (Owner != null)
		};
		if (baseHeldToolSaveData.IsInPlayerInventory)
		{
			baseHeldToolSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		return JsonUtility.ToJson(baseHeldToolSaveData);
	}

	protected IEnumerator WaitThenAddToInventory(int index = -1)
	{
		yield return new WaitForFixedUpdate();
		if (base.gameObject != null)
		{
			TryAddToInventory(index);
		}
	}
}
