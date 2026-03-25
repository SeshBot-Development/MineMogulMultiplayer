using System;
using UnityEngine;

[Serializable]
public class ResourceQuestRequirement : QuestRequirement
{
	public ResourceType ResourceType;

	public PieceType PieceType;

	public bool RequirePolishedResource;

	public int AmountRequired;

	[HideInInspector]
	public int CurrentAmount;

	public string OverrideDisplayName;

	public override string GetRequirementText()
	{
		string text = "Deposit";
		if (!string.IsNullOrEmpty(OverrideDisplayName))
		{
			text = OverrideDisplayName;
		}
		return $"{text} {Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(ResourceType, PieceType, RequirePolishedResource)}: ({CurrentAmount}/{AmountRequired})";
	}

	public override bool IsCompleted()
	{
		return CurrentAmount >= AmountRequired;
	}

	public override QuestRequirement Clone()
	{
		return new ResourceQuestRequirement
		{
			ResourceType = ResourceType,
			OverrideDisplayName = OverrideDisplayName,
			PieceType = PieceType,
			RequirePolishedResource = RequirePolishedResource,
			AmountRequired = AmountRequired,
			IsHidden = IsHidden,
			UnlocksHiddenQuest = UnlocksHiddenQuest,
			CurrentAmount = 0
		};
	}
}
