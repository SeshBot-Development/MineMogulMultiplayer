using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ContractsManager : Singleton<ContractsManager>
{
	public ContractInstance ActiveContract;

	public List<ContractInstance> InactiveContracts;

	public int MaxActiveContracts = 3;

	public Action OnActiveContractUpdated;

	[SerializeField]
	private List<ContractDefinition> _allContractDefinitions = new List<ContractDefinition>();

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<ContractsManager>.Instance != this))
		{
			ActiveContract = null;
			InactiveContracts = new List<ContractInstance>();
		}
	}

	private void Start()
	{
		foreach (ContractDefinition allContractDefinition in _allContractDefinitions)
		{
			ContractInstance item = allContractDefinition.GenerateContract();
			InactiveContracts.Add(item);
		}
	}

	public void DepositBox(BoxObject box)
	{
		bool flag = false;
		foreach (BoxContentEntry content in box.BoxContents.Contents)
		{
			if (DepositContentInSelectedContract(content))
			{
				flag = true;
			}
		}
		if (flag)
		{
			box.Delete();
			OnActiveContractUpdated?.Invoke();
		}
	}

	private bool DepositContentInSelectedContract(BoxContentEntry entry)
	{
		if (ActiveContract == null)
		{
			return false;
		}
		bool result = false;
		foreach (QuestRequirement questRequirement in ActiveContract.QuestRequirements)
		{
			if (questRequirement is ResourceQuestRequirement resourceQuestRequirement && resourceQuestRequirement.ResourceType == entry.ResourceType && resourceQuestRequirement.PieceType == entry.PieceType && (!resourceQuestRequirement.RequirePolishedResource || entry.IsPolished))
			{
				resourceQuestRequirement.CurrentAmount += entry.Count;
				result = true;
				Debug.Log($"Deposited {entry.Count} polished {entry.ResourceType} {entry.PieceType} into contract {ActiveContract.Name}");
			}
		}
		return result;
	}

	public void SetContractActive(ContractInstance contract)
	{
		if (ActiveContract != null)
		{
			InactiveContracts.Add(ActiveContract);
		}
		ActiveContract = contract;
		InactiveContracts.Remove(contract);
		OnActiveContractUpdated?.Invoke();
	}

	public void SetContractInactive(ContractInstance contract)
	{
		if (ActiveContract == contract)
		{
			ActiveContract = null;
			InactiveContracts.Add(contract);
			OnActiveContractUpdated?.Invoke();
		}
	}

	public void ClaimReward(ContractInstance contract)
	{
		if (!contract.IsCompleted())
		{
			Debug.Log("Trying to claim reward for a contract that is not completed: " + contract.Name);
			return;
		}
		Debug.Log("Completed contract and claimed reward for: " + contract.Name);
		Singleton<EconomyManager>.Instance.AddMoney(contract.RewardMoney);
		if (ActiveContract == contract)
		{
			ActiveContract = null;
			OnActiveContractUpdated?.Invoke();
		}
		InactiveContracts.Remove(contract);
	}
}
