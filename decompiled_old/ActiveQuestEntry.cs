using System;
using System.Collections.Generic;

[Serializable]
public class ActiveQuestEntry
{
	public QuestID QuestID;

	public List<ResourceQuestRequirementEntry> ResourceRequirements = new List<ResourceQuestRequirementEntry>();
}
