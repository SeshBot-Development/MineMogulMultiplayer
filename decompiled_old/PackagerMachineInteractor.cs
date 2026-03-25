using System.Collections.Generic;
using UnityEngine;

public class PackagerMachineInteractor : MonoBehaviour, IInteractable
{
	public PackagerMachine PackagerMachine;

	[SerializeField]
	private List<Interaction> _interactions;

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public string GetObjectName()
	{
		return "Packager";
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Eject Box")
		{
			PackagerMachine.SpawnNewBox();
		}
	}
}
