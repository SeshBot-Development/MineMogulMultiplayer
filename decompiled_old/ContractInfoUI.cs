using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContractInfoUI : MonoBehaviour
{
	public ContractInstance Contract;

	private List<QuestRequirementUI> _requirementUIs = new List<QuestRequirementUI>();

	[SerializeField]
	private GameObject _setActiveButton;

	[SerializeField]
	private GameObject _setInactiveButton;

	[SerializeField]
	private GameObject _claimRewardButton;

	[SerializeField]
	private TMP_Text _contractNameText;

	[SerializeField]
	private TMP_Text _contractDescriptionText;

	[SerializeField]
	private TMP_Text _milestoneText;

	[SerializeField]
	private TMP_Text _rewardText;

	public RectTransform RequirementsContainer;

	public QuestRequirementUI RequirementUIPrefab;

	private ContractsTerminalUI _owner;

	private bool _isActiveContract;

	public void Initialize(ContractInstance contract, ContractsTerminalUI owner, bool isActiveContract)
	{
		Contract = contract;
		_owner = owner;
		_isActiveContract = isActiveContract;
		if (Contract.IsCompleted())
		{
			_setActiveButton.SetActive(value: false);
			_setInactiveButton.SetActive(value: false);
			_claimRewardButton.SetActive(value: true);
		}
		else
		{
			_claimRewardButton.SetActive(value: false);
			if (_isActiveContract)
			{
				_setActiveButton.SetActive(value: false);
				_setInactiveButton.SetActive(value: true);
			}
			else
			{
				_setActiveButton.SetActive(value: true);
				_setInactiveButton.SetActive(value: false);
			}
		}
		_contractNameText.text = Contract.Name;
		_contractDescriptionText.text = Contract.Description;
		_milestoneText.text = "Milestone 1 (" + Contract.GetPercentCompleteText() + "):";
		foreach (Transform item in RequirementsContainer)
		{
			Object.Destroy(item.gameObject);
		}
		_requirementUIs.Clear();
		foreach (QuestRequirement questRequirement in Contract.QuestRequirements)
		{
			if (!questRequirement.IsHidden)
			{
				QuestRequirementUI questRequirementUI = Object.Instantiate(RequirementUIPrefab, RequirementsContainer);
				questRequirementUI.Initialize(questRequirement);
				_requirementUIs.Add(questRequirementUI);
			}
		}
		string rewardText = Contract.GetRewardText();
		if (string.IsNullOrEmpty(rewardText))
		{
			Object.Destroy(_rewardText.gameObject);
		}
		else
		{
			_rewardText.text = rewardText;
		}
		StartCoroutine(WaitThenRebuildLayout());
	}

	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
	}

	private void Update()
	{
		if (Contract == null)
		{
			Object.Destroy(base.gameObject);
		}
		else if (_isActiveContract)
		{
			RefreshDisplay();
		}
	}

	public void RefreshDisplay()
	{
		foreach (QuestRequirementUI requirementUI in _requirementUIs)
		{
			requirementUI.RefreshDisplay();
		}
	}

	public void SetContractActive()
	{
		_owner.SetContractActive(this);
	}

	public void SetContractInactive()
	{
		_owner.SetContractInactive(this);
	}

	public void ClaimReward()
	{
		_owner.ClaimReward(this);
	}
}
