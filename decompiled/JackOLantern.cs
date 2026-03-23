using System.Collections.Generic;
using UnityEngine;

public class JackOLantern : BaseSellableItem, IInteractable, ISaveLoadableObject, IIconItem
{
	public string Name;

	public SavableObjectID SavableObjectID;

	public SoundDefinition InteractSound;

	public float ExplosionMultiplier = 1f;

	public Sprite ProgrammerInventoryIcon;

	public Sprite InventoryIcon;

	[SerializeField]
	private List<Interaction> _interactions;

	public bool HasBeenSaved { get; set; }

	public void Scare()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(InteractSound, base.transform.position);
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			Vector3 vector = Vector3.up + -base.transform.forward + Random.insideUnitSphere * 0.2f;
			float num = 2f * ExplosionMultiplier;
			float num2 = 6f * ExplosionMultiplier;
			component.AddForce(vector * num, ForceMode.Impulse);
			component.AddTorque(vector * num2, ForceMode.Impulse);
		}
	}

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
		_ = selectedInteraction.Name;
		Scare();
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
