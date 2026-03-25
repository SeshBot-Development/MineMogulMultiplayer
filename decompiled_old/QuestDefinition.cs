using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/New Quest")]
public class QuestDefinition : ScriptableObject
{
	public QuestID QuestID;

	public string Name;

	[TextArea]
	public string Description;

	public float UIPriority = 100f;

	public string OverrideRewardText;

	public Sprite OverrideQuestIcon;

	public Sprite OverrideQuestProgrammerIcon;

	public bool HideInQuestTree;

	[Tooltip("OFF = requires all prerequisites")]
	public bool UnlockWhenAnyPrerequisitesAreComplete;

	public List<QuestDefinition> PrerequisiteQuests = new List<QuestDefinition>();

	public List<UnlockResearchQuestRequirement> UnlockResearchQuestRequirements = new List<UnlockResearchQuestRequirement>();

	public List<ShopItemQuestRequirement> ShopItemQuestRequirements = new List<ShopItemQuestRequirement>();

	public List<TimedQuestRequirement> TimedRequirements = new List<TimedQuestRequirement>();

	public List<TriggeredQuestRequirement> TriggeredRequirements = new List<TriggeredQuestRequirement>();

	public List<ResourceQuestRequirement> ResourceRequirements = new List<ResourceQuestRequirement>();

	[FormerlySerializedAs("QuestsToUnlock")]
	public List<QuestDefinition> QuestsToAutoStart = new List<QuestDefinition>();

	public List<ShopItemDefinition> ShopItemsToUnlock = new List<ShopItemDefinition>();

	public List<ShopItemDefinition> HiddenShopItemsToUnlock = new List<ShopItemDefinition>();

	public float RewardMoney;

	public int RewardResearchTickets;

	public Quest GenerateQuest()
	{
		Quest quest = new Quest();
		quest.QuestID = QuestID;
		quest.Name = Name;
		quest.Description = Description;
		quest.RewardMoney = RewardMoney;
		quest.RewardResearchTickets = RewardResearchTickets;
		quest.UIPriority = UIPriority;
		quest.OverrideRewardText = OverrideRewardText;
		quest.HideInQuestTree = HideInQuestTree;
		foreach (UnlockResearchQuestRequirement unlockResearchQuestRequirement in UnlockResearchQuestRequirements)
		{
			quest.QuestRequirements.Add(unlockResearchQuestRequirement.Clone());
		}
		foreach (ShopItemQuestRequirement shopItemQuestRequirement in ShopItemQuestRequirements)
		{
			quest.QuestRequirements.Add(shopItemQuestRequirement.Clone());
		}
		foreach (TimedQuestRequirement timedRequirement in TimedRequirements)
		{
			quest.QuestRequirements.Add(timedRequirement.Clone());
		}
		foreach (TriggeredQuestRequirement triggeredRequirement in TriggeredRequirements)
		{
			quest.QuestRequirements.Add(triggeredRequirement.Clone());
		}
		foreach (ResourceQuestRequirement resourceRequirement in ResourceRequirements)
		{
			quest.QuestRequirements.Add(resourceRequirement.Clone());
		}
		quest.UnlockWhenAnyPrerequisitesAreComplete = UnlockWhenAnyPrerequisitesAreComplete;
		quest.PrerequisiteQuests = PrerequisiteQuests;
		quest.QuestsToAutoStart = QuestsToAutoStart;
		quest.ShopItemsToUnlock = ShopItemsToUnlock;
		quest.HiddenShopItemsToUnlock = HiddenShopItemsToUnlock;
		return quest;
	}

	public Sprite GetOverrideIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(OverrideQuestProgrammerIcon != null))
			{
				return OverrideQuestIcon;
			}
			return OverrideQuestProgrammerIcon;
		}
		if (!(OverrideQuestIcon != null))
		{
			return OverrideQuestProgrammerIcon;
		}
		return OverrideQuestIcon;
	}
}
