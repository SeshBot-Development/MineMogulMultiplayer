using System;
using UnityEngine;

[Serializable]
public class TriggeredQuestRequirement : QuestRequirement
{
	public TriggeredQuestRequirementType TriggeredQuestRequirementType;

	public string RequirementName;

	public int AmountRequired;

	[HideInInspector]
	public int CurrentAmount;

	public override string GetRequirementText()
	{
		string text = "";
		text = ((AmountRequired != 1) ? $"{RequirementName}: ({CurrentAmount}/{AmountRequired})" : RequirementName);
		return Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(text);
	}

	public override bool IsCompleted()
	{
		return CurrentAmount >= AmountRequired;
	}

	public override QuestRequirement Clone()
	{
		return new TriggeredQuestRequirement
		{
			TriggeredQuestRequirementType = TriggeredQuestRequirementType,
			RequirementName = RequirementName,
			AmountRequired = AmountRequired,
			IsHidden = IsHidden,
			UnlocksHiddenQuest = UnlocksHiddenQuest,
			CurrentAmount = 0
		};
	}
}
