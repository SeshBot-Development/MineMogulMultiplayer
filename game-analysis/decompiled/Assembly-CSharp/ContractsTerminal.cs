using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x02000030 RID: 48
public class ContractsTerminal : MonoBehaviour, IInteractable
{
	// Token: 0x06000158 RID: 344 RVA: 0x00007EE4 File Offset: 0x000060E4
	private void OnEnable()
	{
		this.RefreshScreenText();
		ContractsManager instance = Singleton<ContractsManager>.Instance;
		instance.OnActiveContractUpdated = (Action)Delegate.Combine(instance.OnActiveContractUpdated, new Action(this.RefreshScreenText));
	}

	// Token: 0x06000159 RID: 345 RVA: 0x00007F12 File Offset: 0x00006112
	private void OnDisable()
	{
		ContractsManager instance = Singleton<ContractsManager>.Instance;
		instance.OnActiveContractUpdated = (Action)Delegate.Remove(instance.OnActiveContractUpdated, new Action(this.RefreshScreenText));
	}

	// Token: 0x0600015A RID: 346 RVA: 0x00007F3C File Offset: 0x0000613C
	public void RefreshScreenText()
	{
		ContractInstance activeContract = Singleton<ContractsManager>.Instance.ActiveContract;
		if (activeContract != null)
		{
			this._screenText.text = "Active Contract:\n" + activeContract.Name + "\nProgress: " + activeContract.GetPercentCompleteText();
			return;
		}
		this._screenText.text = "No Active Contract";
	}

	// Token: 0x0600015B RID: 347 RVA: 0x00007F8E File Offset: 0x0000618E
	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	// Token: 0x0600015C RID: 348 RVA: 0x00007F91 File Offset: 0x00006191
	public string GetObjectName()
	{
		return "Contracts Terminal";
	}

	// Token: 0x0600015D RID: 349 RVA: 0x00007F98 File Offset: 0x00006198
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x0600015E RID: 350 RVA: 0x00007FA0 File Offset: 0x000061A0
	public void Interact(Interaction selectedInteraction)
	{
		this.ToggleComputerUI();
	}

	// Token: 0x0600015F RID: 351 RVA: 0x00007FA8 File Offset: 0x000061A8
	private void ToggleComputerUI()
	{
		Singleton<UIManager>.Instance.ShowContractsTerminal();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._interactSound, base.transform.position, 1f, 1f, true, false);
	}

	// Token: 0x0400014F RID: 335
	[SerializeField]
	private TMP_Text _screenText;

	// Token: 0x04000150 RID: 336
	[SerializeField]
	private List<Interaction> _interactions;

	// Token: 0x04000151 RID: 337
	[SerializeField]
	private SoundDefinition _interactSound;
}
