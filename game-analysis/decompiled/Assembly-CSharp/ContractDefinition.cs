using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002B RID: 43
[CreateAssetMenu(fileName = "New Contract", menuName = "Contracts/New Contract")]
public class ContractDefinition : ScriptableObject
{
	// Token: 0x0600013D RID: 317 RVA: 0x0000762C File Offset: 0x0000582C
	public ContractInstance GenerateContract()
	{
		ContractInstance contractInstance = new ContractInstance();
		contractInstance.Name = this.Name;
		contractInstance.Description = this.Description;
		contractInstance.RewardMoney = this.RewardMoney;
		foreach (ResourceQuestRequirement resourceQuestRequirement in this.ResourceRequirements)
		{
			contractInstance.QuestRequirements.Add(resourceQuestRequirement.Clone());
		}
		return contractInstance;
	}

	// Token: 0x04000134 RID: 308
	public string Name;

	// Token: 0x04000135 RID: 309
	[TextArea]
	public string Description;

	// Token: 0x04000136 RID: 310
	public List<ResourceQuestRequirement> ResourceRequirements = new List<ResourceQuestRequirement>();

	// Token: 0x04000137 RID: 311
	public float RewardMoney;
}
