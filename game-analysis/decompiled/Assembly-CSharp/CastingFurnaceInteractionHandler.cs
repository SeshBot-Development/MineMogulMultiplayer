using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000023 RID: 35
public class CastingFurnaceInteractionHandler : MonoBehaviour, IInteractable
{
	// Token: 0x06000111 RID: 273 RVA: 0x00006DE6 File Offset: 0x00004FE6
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x06000112 RID: 274 RVA: 0x00006DE9 File Offset: 0x00004FE9
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000113 RID: 275 RVA: 0x00006DF1 File Offset: 0x00004FF1
	public string GetObjectName()
	{
		return "Casting Furnace";
	}

	// Token: 0x06000114 RID: 276 RVA: 0x00006DF8 File Offset: 0x00004FF8
	public virtual void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Eject Mold 1")
		{
			this.CastingFurnace.MoldAreas[0].EjectMold();
			return;
		}
		if (name == "Eject Mold 2")
		{
			this.CastingFurnace.MoldAreas[1].EjectMold();
			return;
		}
		if (!(name == "Eject Mold 3"))
		{
			return;
		}
		this.CastingFurnace.MoldAreas[2].EjectMold();
	}

	// Token: 0x0400010B RID: 267
	public CastingFurnace CastingFurnace;

	// Token: 0x0400010C RID: 268
	[SerializeField]
	private List<Interaction> _interactions;
}
