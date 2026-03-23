using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Contract", menuName = "Contracts/New Contract")]
public class ContractDefinition : ScriptableObject
{
	public string Name;

	[TextArea]
	public string Description;

	public List<ResourceQuestRequirement> ResourceRequirements = new List<ResourceQuestRequirement>();

	public float RewardMoney;

	public ContractInstance GenerateContract()
	{
		ContractInstance contractInstance = new ContractInstance();
		contractInstance.Name = Name;
		contractInstance.Description = Description;
		contractInstance.RewardMoney = RewardMoney;
		foreach (ResourceQuestRequirement resourceRequirement in ResourceRequirements)
		{
			contractInstance.QuestRequirements.Add(resourceRequirement.Clone());
		}
		return contractInstance;
	}
}
