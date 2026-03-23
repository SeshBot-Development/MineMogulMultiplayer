using System.Collections.Generic;
using UnityEngine;

public class OreSled : BaseSellableItem, IInteractable, ISaveLoadableObject, IIconItem
{
	public string Name;

	public SavableObjectID SavableObjectID;

	public Sprite ProgrammerInventoryIcon;

	public Sprite InventoryIcon;

	[SerializeField]
	private List<Interaction> _interactions;

	public bool HasBeenSaved { get; set; }

	public virtual Sprite GetIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
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
		return Name;
	}

	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Destroy")
		{
			Object.Destroy(base.gameObject);
		}
	}

	public bool ShouldBeSaved()
	{
		return true;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	public SavableObjectID GetSavableObjectID()
	{
		return SavableObjectID;
	}

	public virtual void LoadFromSave(string json)
	{
	}

	public virtual string GetCustomSaveData()
	{
		return null;
	}
}
