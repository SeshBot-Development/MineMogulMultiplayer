using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetonatorBuySign : MonoBehaviour, IInteractable
{
	[SerializeField]
	private SoundDefinition _purchaseSoundDefinition;

	[SerializeField]
	private TMP_Text _priceText;

	private DetonatorExplosion _owner;

	[SerializeField]
	private List<Interaction> _interactions;

	public void Initialize(DetonatorExplosion owner)
	{
		_owner = owner;
		_priceText.text = EconomyManager.GetFormattedMoneyString(_owner.CostToBuy, includeDecimal: false);
		if (_owner.HasPurchased())
		{
			base.gameObject.SetActive(value: false);
		}
		if (Singleton<DemoManager>.Instance.IsDemoVersion)
		{
			_priceText.text = "Locked in demo";
			_interactions.RemoveAll((Interaction interaction) => interaction.Name == "Purchase");
		}
	}

	public void TryBuySign(bool isFromLoading = false)
	{
		if (Singleton<DemoManager>.Instance.IsDemoVersion || _owner.HasPurchased())
		{
			return;
		}
		if (!isFromLoading)
		{
			if (!Singleton<EconomyManager>.Instance.CanAfford(_owner.CostToBuy))
			{
				return;
			}
			Singleton<EconomyManager>.Instance.AddMoney(0f - _owner.CostToBuy);
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(_purchaseSoundDefinition, base.transform.position);
		}
		_owner.PurchaseTNT();
	}

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public string GetObjectName()
	{
		return "Cost: " + EconomyManager.GetFormattedMoneyString(_owner.CostToBuy, includeDecimal: false);
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Purchase")
		{
			TryBuySign();
		}
	}
}
