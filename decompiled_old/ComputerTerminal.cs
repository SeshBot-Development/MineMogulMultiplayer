using System.Collections.Generic;
using UnityEngine;

public class ComputerTerminal : MonoBehaviour, IInteractable
{
	[SerializeField]
	private List<Interaction> _interactions;

	[SerializeField]
	private SoundDefinition _interactSound;

	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	public string GetObjectName()
	{
		return "Computer Store";
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		ToggleComputerUI();
	}

	private void ToggleComputerUI()
	{
		GameObject obj = Singleton<UIManager>.Instance.ComputerShopUI.gameObject;
		obj.SetActive(!obj.activeSelf);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_interactSound, base.transform.position);
	}
}
