using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002F RID: 47
[DefaultExecutionOrder(-100)]
public class ContractsManager : Singleton<ContractsManager>
{
	// Token: 0x06000150 RID: 336 RVA: 0x00007BD9 File Offset: 0x00005DD9
	protected override void Awake()
	{
		base.Awake();
		if (Singleton<ContractsManager>.Instance != this)
		{
			return;
		}
		this.ActiveContract = null;
		this.InactiveContracts = new List<ContractInstance>();
	}

	// Token: 0x06000151 RID: 337 RVA: 0x00007C04 File Offset: 0x00005E04
	private void Start()
	{
		foreach (ContractDefinition contractDefinition in this._allContractDefinitions)
		{
			ContractInstance contractInstance = contractDefinition.GenerateContract();
			this.InactiveContracts.Add(contractInstance);
		}
	}

	// Token: 0x06000152 RID: 338 RVA: 0x00007C64 File Offset: 0x00005E64
	public void DepositBox(BoxObject box)
	{
		bool flag = false;
		foreach (BoxContentEntry boxContentEntry in box.BoxContents.Contents)
		{
			if (this.DepositContentInSelectedContract(boxContentEntry))
			{
				flag = true;
			}
		}
		if (flag)
		{
			box.Delete();
			Action onActiveContractUpdated = this.OnActiveContractUpdated;
			if (onActiveContractUpdated == null)
			{
				return;
			}
			onActiveContractUpdated();
		}
	}

	// Token: 0x06000153 RID: 339 RVA: 0x00007CDC File Offset: 0x00005EDC
	private bool DepositContentInSelectedContract(BoxContentEntry entry)
	{
		if (this.ActiveContract == null)
		{
			return false;
		}
		bool flag = false;
		foreach (QuestRequirement questRequirement in this.ActiveContract.QuestRequirements)
		{
			ResourceQuestRequirement resourceQuestRequirement = questRequirement as ResourceQuestRequirement;
			if (resourceQuestRequirement != null && resourceQuestRequirement.ResourceType == entry.ResourceType && resourceQuestRequirement.PieceType == entry.PieceType && (!resourceQuestRequirement.RequirePolishedResource || entry.IsPolished))
			{
				resourceQuestRequirement.CurrentAmount += entry.Count;
				flag = true;
				Debug.Log(string.Format("Deposited {0} polished {1} {2} into contract {3}", new object[]
				{
					entry.Count,
					entry.ResourceType,
					entry.PieceType,
					this.ActiveContract.Name
				}));
			}
		}
		return flag;
	}

	// Token: 0x06000154 RID: 340 RVA: 0x00007DDC File Offset: 0x00005FDC
	public void SetContractActive(ContractInstance contract)
	{
		if (this.ActiveContract != null)
		{
			this.InactiveContracts.Add(this.ActiveContract);
		}
		this.ActiveContract = contract;
		this.InactiveContracts.Remove(contract);
		Action onActiveContractUpdated = this.OnActiveContractUpdated;
		if (onActiveContractUpdated == null)
		{
			return;
		}
		onActiveContractUpdated();
	}

	// Token: 0x06000155 RID: 341 RVA: 0x00007E1B File Offset: 0x0000601B
	public void SetContractInactive(ContractInstance contract)
	{
		if (this.ActiveContract == contract)
		{
			this.ActiveContract = null;
			this.InactiveContracts.Add(contract);
			Action onActiveContractUpdated = this.OnActiveContractUpdated;
			if (onActiveContractUpdated == null)
			{
				return;
			}
			onActiveContractUpdated();
		}
	}

	// Token: 0x06000156 RID: 342 RVA: 0x00007E4C File Offset: 0x0000604C
	public void ClaimReward(ContractInstance contract)
	{
		if (!contract.IsCompleted())
		{
			Debug.Log("Trying to claim reward for a contract that is not completed: " + contract.Name);
			return;
		}
		Debug.Log("Completed contract and claimed reward for: " + contract.Name);
		Singleton<EconomyManager>.Instance.AddMoney(contract.RewardMoney);
		if (this.ActiveContract == contract)
		{
			this.ActiveContract = null;
			Action onActiveContractUpdated = this.OnActiveContractUpdated;
			if (onActiveContractUpdated != null)
			{
				onActiveContractUpdated();
			}
		}
		this.InactiveContracts.Remove(contract);
	}

	// Token: 0x0400014A RID: 330
	public ContractInstance ActiveContract;

	// Token: 0x0400014B RID: 331
	public List<ContractInstance> InactiveContracts;

	// Token: 0x0400014C RID: 332
	public int MaxActiveContracts = 3;

	// Token: 0x0400014D RID: 333
	public Action OnActiveContractUpdated;

	// Token: 0x0400014E RID: 334
	[SerializeField]
	private List<ContractDefinition> _allContractDefinitions = new List<ContractDefinition>();
}
