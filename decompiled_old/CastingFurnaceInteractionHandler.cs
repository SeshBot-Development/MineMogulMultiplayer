using System.Collections.Generic;
using UnityEngine;

public class CastingFurnaceInteractionHandler : MonoBehaviour, IInteractable
{
	public CastingFurnace CastingFurnace;

	[SerializeField]
	private List<Interaction> _interactions;

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
		return "Casting Furnace";
	}

	public virtual void Interact(Interaction selectedInteraction)
	{
		switch (selectedInteraction.Name)
		{
		case "Eject Mold 1":
			CastingFurnace.MoldAreas[0].EjectMold();
			break;
		case "Eject Mold 2":
			CastingFurnace.MoldAreas[1].EjectMold();
			break;
		case "Eject Mold 3":
			CastingFurnace.MoldAreas[2].EjectMold();
			break;
		}
	}
}
