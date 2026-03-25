using System.Collections.Generic;
using UnityEngine;

public class ContractsTerminalUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform _activeContractInfoUIContainer;

	[SerializeField]
	private RectTransform _inactiveContractInfoUIsContainer;

	[SerializeField]
	private GameObject _activeContractSection;

	[SerializeField]
	private GameObject _inactiveContractsSection;

	[SerializeField]
	private ContractInfoUI _contractInfoUIPrefab;

	private List<ContractInfoUI> _contractInfoUIs = new List<ContractInfoUI>();

	private void OnEnable()
	{
		RegenerateContractsList();
	}

	public void RegenerateContractsList()
	{
		foreach (Transform item in _activeContractInfoUIContainer)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in _inactiveContractInfoUIsContainer)
		{
			Object.Destroy(item2.gameObject);
		}
		_contractInfoUIs.Clear();
		_activeContractSection.SetActive(Singleton<ContractsManager>.Instance.ActiveContract != null);
		if (Singleton<ContractsManager>.Instance.ActiveContract != null)
		{
			AddContract(Singleton<ContractsManager>.Instance.ActiveContract, isActive: true);
		}
		_inactiveContractsSection.SetActive(Singleton<ContractsManager>.Instance.InactiveContracts.Count > 0);
		foreach (ContractInstance inactiveContract in Singleton<ContractsManager>.Instance.InactiveContracts)
		{
			AddContract(inactiveContract, isActive: false);
		}
	}

	private void AddContract(ContractInstance contract, bool isActive)
	{
		RectTransform parent = (isActive ? _activeContractInfoUIContainer : _inactiveContractInfoUIsContainer);
		ContractInfoUI contractInfoUI = Object.Instantiate(_contractInfoUIPrefab, parent);
		contractInfoUI.Initialize(contract, this, isActive);
		_contractInfoUIs.Add(contractInfoUI);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetContractActive(ContractInfoUI contractInfoUI)
	{
		Singleton<ContractsManager>.Instance.SetContractActive(contractInfoUI.Contract);
		RegenerateContractsList();
	}

	public void SetContractInactive(ContractInfoUI contractInfoUI)
	{
		Singleton<ContractsManager>.Instance.SetContractInactive(contractInfoUI.Contract);
		RegenerateContractsList();
	}

	public void ClaimReward(ContractInfoUI contractInfoUI)
	{
		Singleton<ContractsManager>.Instance.ClaimReward(contractInfoUI.Contract);
		RegenerateContractsList();
	}
}
