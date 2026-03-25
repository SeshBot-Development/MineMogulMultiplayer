using System;

[Serializable]
public abstract class QuestRequirement
{
	public bool IsHidden;

	public bool UnlocksHiddenQuest;

	public virtual string GetRequirementText()
	{
		return "invalid requirement";
	}

	public abstract bool IsCompleted();

	public abstract QuestRequirement Clone();
}
