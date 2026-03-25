using System;

[Serializable]
public class ResourceQuestRequirementEntry
{
	public ResourceType ResourceType;

	public PieceType PieceType;

	public bool RequirePolishedResource;

	public int CurrentAmount;
}
