using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002A RID: 42
public class ComputerTerminal : MonoBehaviour, IInteractable
{
	// Token: 0x06000137 RID: 311 RVA: 0x000075B3 File Offset: 0x000057B3
	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	// Token: 0x06000138 RID: 312 RVA: 0x000075B6 File Offset: 0x000057B6
	public string GetObjectName()
	{
		return "Computer Store";
	}

	// Token: 0x06000139 RID: 313 RVA: 0x000075BD File Offset: 0x000057BD
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x0600013A RID: 314 RVA: 0x000075C5 File Offset: 0x000057C5
	public void Interact(Interaction selectedInteraction)
	{
		this.ToggleComputerUI();
	}

	// Token: 0x0600013B RID: 315 RVA: 0x000075D0 File Offset: 0x000057D0
	private void ToggleComputerUI()
	{
		GameObject gameObject = Singleton<UIManager>.Instance.ComputerShopUI.gameObject;
		gameObject.SetActive(!gameObject.activeSelf);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._interactSound, base.transform.position, 1f, 1f, true, false);
	}

	// Token: 0x04000132 RID: 306
	[SerializeField]
	private List<Interaction> _interactions;

	// Token: 0x04000133 RID: 307
	[SerializeField]
	private SoundDefinition _interactSound;
}
