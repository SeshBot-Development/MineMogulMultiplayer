using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x02000042 RID: 66
public class DetonatorBuySign : MonoBehaviour, IInteractable
{
	// Token: 0x060001BE RID: 446 RVA: 0x00009A09 File Offset: 0x00007C09
	public void Initialize(DetonatorExplosion owner)
	{
		this._owner = owner;
		this._priceText.text = EconomyManager.GetFormattedMoneyString(this._owner.CostToBuy, false);
		if (this._owner.HasPurchased())
		{
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x060001BF RID: 447 RVA: 0x00009A48 File Offset: 0x00007C48
	public void TryBuySign(bool isFromLoading = false)
	{
		if (this._owner.HasPurchased())
		{
			return;
		}
		if (!isFromLoading)
		{
			if (!Singleton<EconomyManager>.Instance.CanAfford(this._owner.CostToBuy))
			{
				return;
			}
			Singleton<EconomyManager>.Instance.AddMoney(-this._owner.CostToBuy);
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._purchaseSoundDefinition, base.transform.position, 1f, 1f, true, false);
		}
		this._owner.PurchaseTNT();
	}

	// Token: 0x060001C0 RID: 448 RVA: 0x00009AC6 File Offset: 0x00007CC6
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x060001C1 RID: 449 RVA: 0x00009AC9 File Offset: 0x00007CC9
	public string GetObjectName()
	{
		return "Cost: " + EconomyManager.GetFormattedMoneyString(this._owner.CostToBuy, false);
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x00009AE6 File Offset: 0x00007CE6
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x060001C3 RID: 451 RVA: 0x00009AEE File Offset: 0x00007CEE
	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Purchase")
		{
			this.TryBuySign(false);
		}
	}

	// Token: 0x040001B8 RID: 440
	[SerializeField]
	private SoundDefinition _purchaseSoundDefinition;

	// Token: 0x040001B9 RID: 441
	[SerializeField]
	private TMP_Text _priceText;

	// Token: 0x040001BA RID: 442
	private DetonatorExplosion _owner;

	// Token: 0x040001BB RID: 443
	[SerializeField]
	private List<Interaction> _interactions;
}
