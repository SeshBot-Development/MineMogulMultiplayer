using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContractsTerminal : MonoBehaviour, IInteractable
{
	[SerializeField]
	private TMP_Text _screenText;

	[SerializeField]
	private List<Interaction> _interactions;

	[SerializeField]
	private SoundDefinition _interactSound;

	private void OnEnable()
	{
		RefreshScreenText();
		ContractsManager instance = Singleton<ContractsManager>.Instance;
		instance.OnActiveContractUpdated = (Action)Delegate.Combine(instance.OnActiveContractUpdated, new Action(RefreshScreenText));
	}

	private void OnDisable()
	{
		ContractsManager instance = Singleton<ContractsManager>.Instance;
		instance.OnActiveContractUpdated = (Action)Delegate.Remove(instance.OnActiveContractUpdated, new Action(RefreshScreenText));
	}

	public void RefreshScreenText()
	{
		ContractInstance activeContract = Singleton<ContractsManager>.Instance.ActiveContract;
		if (activeContract != null)
		{
			_screenText.text = "Active Contract:\n" + activeContract.Name + "\nProgress: " + activeContract.GetPercentCompleteText();
		}
		else
		{
			_screenText.text = "No Active Contract";
		}
	}

	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	public string GetObjectName()
	{
		return "Contracts Terminal";
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
		Singleton<UIManager>.Instance.ShowContractsTerminal();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_interactSound, base.transform.position);
	}
}
