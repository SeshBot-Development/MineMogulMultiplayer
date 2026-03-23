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

	public override void OnResearched()
	{
		Object.FindObjectOfType<DepositBox>().UpgradeToTier2();
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
