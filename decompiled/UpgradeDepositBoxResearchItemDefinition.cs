using UnityEngine;

[CreateAssetMenu(fileName = "New UpgradeDepositBoxResearchItem", menuName = "Research/UpgradeDepositBoxResearchItem")]
public class UpgradeDepositBoxResearchItemDefinition : ResearchItemDefinition
{
	[SerializeField]
	private string _displayName;

	[TextArea]
	[SerializeField]
	private string _description;

	[SerializeField]
	private Sprite _icon;

	[SerializeField]
	private Sprite _programmerIcon;

	[SerializeField]
	private SavableObjectID _savableObjectID;

	public override bool IsLocked()
	{
		if (Singleton<GamemodeManager>.Instance.GameModeType == GameModeType.Sandbox)
		{
			return false;
		}
		return base.IsLocked();
	}

	public override bool CanAfford()
	{
		if (Singleton<GamemodeManager>.Instance.GameModeType == GameModeType.Sandbox)
		{
			return true;
		}
		return base.CanAfford();
	}

	public override void OnResearched()
	{
		DepositBox depositBox = Object.FindObjectOfType<DepositBox>();
		if (depositBox != null)
		{
			depositBox.UpgradeToTier2();
		}
		if (Singleton<GamemodeManager>.Instance.GameModeType == GameModeType.Sandbox)
		{
			Singleton<ResearchManager>.Instance.AddResearchTickets(_researchTicketsCost);
			Singleton<EconomyManager>.Instance.AddMoney(_moneyCost);
		}
	}

	public override Sprite GetIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(_programmerIcon != null))
			{
				return _icon;
			}
			return _programmerIcon;
		}
		if (!(_icon != null))
		{
			return _programmerIcon;
		}
		return _icon;
	}

	public override string GetName()
	{
		return _displayName;
	}

	public override string GetDescription()
	{
		return _description;
	}

	public override SavableObjectID GetSavableObjectID()
	{
		return _savableObjectID;
	}
}
