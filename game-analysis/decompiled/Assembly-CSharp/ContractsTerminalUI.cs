using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000031 RID: 49
public class ContractsTerminalUI : MonoBehaviour
{
	// Token: 0x06000161 RID: 353 RVA: 0x00007FE3 File Offset: 0x000061E3
	private void OnEnable()
	{
		this.RegenerateContractsList();
	}

	// Token: 0x06000162 RID: 354 RVA: 0x00007FEC File Offset: 0x000061EC
	public void RegenerateContractsList()
	{
		foreach (object obj in this._activeContractInfoUIContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (object obj2 in this._inactiveContractInfoUIsContainer)
		{
			Object.Destroy(((Transform)obj2).gameObject);
		}
		this._contractInfoUIs.Clear();
		this._activeContractSection.SetActive(Singleton<ContractsManager>.Instance.ActiveContract != null);
		if (Singleton<ContractsManager>.Instance.ActiveContract != null)
		{
			this.AddContract(Singleton<ContractsManager>.Instance.ActiveContract, true);
		}
		this._inactiveContractsSection.SetActive(Singleton<ContractsManager>.Instance.InactiveContracts.Count > 0);
		foreach (ContractInstance contractInstance in Singleton<ContractsManager>.Instance.InactiveContracts)
		{
			this.AddContract(contractInstance, false);
		}
	}

	// Token: 0x06000163 RID: 355 RVA: 0x00008138 File Offset: 0x00006338
	private void AddContract(ContractInstance contract, bool isActive)
	{
		RectTransform rectTransform = (isActive ? this._activeContractInfoUIContainer : this._inactiveContractInfoUIsContainer);
		ContractInfoUI contractInfoUI = Object.Instantiate<ContractInfoUI>(this._contractInfoUIPrefab, rectTransform);
		contractInfoUI.Initialize(contract, this, isActive);
		this._contractInfoUIs.Add(contractInfoUI);
	}

	// Token: 0x06000164 RID: 356 RVA: 0x00008179 File Offset: 0x00006379
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000165 RID: 357 RVA: 0x00008190 File Offset: 0x00006390
	public void SetContractActive(ContractInfoUI contractInfoUI)
	{
		Singleton<ContractsManager>.Instance.SetContractActive(contractInfoUI.Contract);
		this.RegenerateContractsList();
	}

	// Token: 0x06000166 RID: 358 RVA: 0x000081A8 File Offset: 0x000063A8
	public void SetContractInactive(ContractInfoUI contractInfoUI)
	{
		Singleton<ContractsManager>.Instance.SetContractInactive(contractInfoUI.Contract);
		this.RegenerateContractsList();
	}

	// Token: 0x06000167 RID: 359 RVA: 0x000081C0 File Offset: 0x000063C0
	public void ClaimReward(ContractInfoUI contractInfoUI)
	{
		Singleton<ContractsManager>.Instance.ClaimReward(contractInfoUI.Contract);
		this.RegenerateContractsList();
	}

	// Token: 0x04000152 RID: 338
	[SerializeField]
	private RectTransform _activeContractInfoUIContainer;

	// Token: 0x04000153 RID: 339
	[SerializeField]
	private RectTransform _inactiveContractInfoUIsContainer;

	// Token: 0x04000154 RID: 340
	[SerializeField]
	private GameObject _activeContractSection;

	// Token: 0x04000155 RID: 341
	[SerializeField]
	private GameObject _inactiveContractsSection;

	// Token: 0x04000156 RID: 342
	[SerializeField]
	private ContractInfoUI _contractInfoUIPrefab;

	// Token: 0x04000157 RID: 343
	private List<ContractInfoUI> _contractInfoUIs = new List<ContractInfoUI>();
}
