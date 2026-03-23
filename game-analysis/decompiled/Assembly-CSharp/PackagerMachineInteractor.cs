using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007E RID: 126
public class PackagerMachineInteractor : MonoBehaviour, IInteractable
{
	// Token: 0x06000359 RID: 857 RVA: 0x00010F2B File Offset: 0x0000F12B
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x0600035A RID: 858 RVA: 0x00010F2E File Offset: 0x0000F12E
	public string GetObjectName()
	{
		return "Packager";
	}

	// Token: 0x0600035B RID: 859 RVA: 0x00010F35 File Offset: 0x0000F135
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x0600035C RID: 860 RVA: 0x00010F3D File Offset: 0x0000F13D
	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Eject Box")
		{
			this.PackagerMachine.SpawnNewBox();
		}
	}

	// Token: 0x0400035A RID: 858
	public PackagerMachine PackagerMachine;

	// Token: 0x0400035B RID: 859
	[SerializeField]
	private List<Interaction> _interactions;
}
