using System;

[Serializable]
public class UnlockResearchQuestRequirement : QuestRequirement
{
	public ResearchItemDefinition ResearchItemDefinition;

	public override string GetRequirementText()
	{
		return "Unlock " + ResearchItemDefinition.GetName() + " in the Research Tree";
	}

	public override bool IsCompleted()
	{
		return ResearchItemDefinition.IsResearched();
	}

	public override QuestRequirement Clone()
	{
		return new UnlockResearchQuestRequirement
		{
			ResearchItemDefinition = ResearchItemDefinition,
			IsHidden = IsHidden,
			UnlocksHiddenQuest = UnlocksHiddenQuest
		};
	}
}
