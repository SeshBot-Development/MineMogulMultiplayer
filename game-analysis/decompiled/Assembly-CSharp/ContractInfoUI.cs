using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200002C RID: 44
public class ContractInfoUI : MonoBehaviour
{
	// Token: 0x0600013F RID: 319 RVA: 0x000076C8 File Offset: 0x000058C8
	public void Initialize(ContractInstance contract, ContractsTerminalUI owner, bool isActiveContract)
	{
		this.Contract = contract;
		this._owner = owner;
		this._isActiveContract = isActiveContract;
		if (this.Contract.IsCompleted())
		{
			this._setActiveButton.SetActive(false);
			this._setInactiveButton.SetActive(false);
			this._claimRewardButton.SetActive(true);
		}
		else
		{
			this._claimRewardButton.SetActive(false);
			if (this._isActiveContract)
			{
				this._setActiveButton.SetActive(false);
				this._setInactiveButton.SetActive(true);
			}
			else
			{
				this._setActiveButton.SetActive(true);
				this._setInactiveButton.SetActive(false);
			}
		}
		this._contractNameText.text = this.Contract.Name;
		this._contractDescriptionText.text = this.Contract.Description;
		this._milestoneText.text = "Milestone 1 (" + this.Contract.GetPercentCompleteText() + "):";
		foreach (object obj in this.RequirementsContainer)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this._requirementUIs.Clear();
		foreach (QuestRequirement questRequirement in this.Contract.QuestRequirements)
		{
			if (!questRequirement.IsHidden)
			{
				QuestRequirementUI questRequirementUI = Object.Instantiate<QuestRequirementUI>(this.RequirementUIPrefab, this.RequirementsContainer);
				questRequirementUI.Initialize(questRequirement);
				this._requirementUIs.Add(questRequirementUI);
			}
		}
		string rewardText = this.Contract.GetRewardText();
		if (string.IsNullOrEmpty(rewardText))
		{
			Object.Destroy(this._rewardText.gameObject);
		}
		else
		{
			this._rewardText.text = rewardText;
		}
		base.StartCoroutine(this.WaitThenRebuildLayout());
	}

	// Token: 0x06000140 RID: 320 RVA: 0x000078C0 File Offset: 0x00005AC0
	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield break;
	}

	// Token: 0x06000141 RID: 321 RVA: 0x000078CF File Offset: 0x00005ACF
	private void Update()
	{
		if (this.Contract == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (this._isActiveContract)
		{
			this.RefreshDisplay();
		}
	}

	// Token: 0x06000142 RID: 322 RVA: 0x000078F4 File Offset: 0x00005AF4
	public void RefreshDisplay()
	{
		foreach (QuestRequirementUI questRequirementUI in this._requirementUIs)
		{
			questRequirementUI.RefreshDisplay();
		}
	}

	// Token: 0x06000143 RID: 323 RVA: 0x00007944 File Offset: 0x00005B44
	public void SetContractActive()
	{
		this._owner.SetContractActive(this);
	}

	// Token: 0x06000144 RID: 324 RVA: 0x00007952 File Offset: 0x00005B52
	public void SetContractInactive()
	{
		this._owner.SetContractInactive(this);
	}

	// Token: 0x06000145 RID: 325 RVA: 0x00007960 File Offset: 0x00005B60
	public void ClaimReward()
	{
		this._owner.ClaimReward(this);
	}

	// Token: 0x04000138 RID: 312
	public ContractInstance Contract;

	// Token: 0x04000139 RID: 313
	private List<QuestRequirementUI> _requirementUIs = new List<QuestRequirementUI>();

	// Token: 0x0400013A RID: 314
	[SerializeField]
	private GameObject _setActiveButton;

	// Token: 0x0400013B RID: 315
	[SerializeField]
	private GameObject _setInactiveButton;

	// Token: 0x0400013C RID: 316
	[SerializeField]
	private GameObject _claimRewardButton;

	// Token: 0x0400013D RID: 317
	[SerializeField]
	private TMP_Text _contractNameText;

	// Token: 0x0400013E RID: 318
	[SerializeField]
	private TMP_Text _contractDescriptionText;

	// Token: 0x0400013F RID: 319
	[SerializeField]
	private TMP_Text _milestoneText;

	// Token: 0x04000140 RID: 320
	[SerializeField]
	private TMP_Text _rewardText;

	// Token: 0x04000141 RID: 321
	public RectTransform RequirementsContainer;

	// Token: 0x04000142 RID: 322
	public QuestRequirementUI RequirementUIPrefab;

	// Token: 0x04000143 RID: 323
	private ContractsTerminalUI _owner;

	// Token: 0x04000144 RID: 324
	private bool _isActiveContract;
}
