using System;
using UnityEngine;

// Token: 0x020000A8 RID: 168
[CreateAssetMenu(fileName = "New UpgradeDepositBoxResearchItem", menuName = "Research/UpgradeDepositBoxResearchItem")]
public class UpgradeDepositBoxResearchItemDefinition : ResearchItemDefinition
{
	// Token: 0x060004AA RID: 1194 RVA: 0x00019340 File Offset: 0x00017540
	public override void OnResearched()
	{
		Object.FindObjectOfType<DepositBox>().UpgradeToTier2();
	}

	// Token: 0x060004AB RID: 1195 RVA: 0x0001934C File Offset: 0x0001754C
	public override Sprite GetIcon()
	{
		if (SettingsManager.ShouldUseProgrammerIcons())
		{
			if (!(this._programmerIcon != null))
			{
				return this._icon;
			}
			return this._programmerIcon;
		}
		else
		{
			if (!(this._icon != null))
			{
				return this._programmerIcon;
			}
			return this._icon;
		}
	}

	// Token: 0x060004AC RID: 1196 RVA: 0x0001938C File Offset: 0x0001758C
	public override string GetName()
	{
		return this._displayName;
	}

	// Token: 0x060004AD RID: 1197 RVA: 0x00019394 File Offset: 0x00017594
	public override string GetDescription()
	{
		return this._description;
	}

	// Token: 0x060004AE RID: 1198 RVA: 0x0001939C File Offset: 0x0001759C
	public override SavableObjectID GetSavableObjectID()
	{
		return this._savableObjectID;
	}

	// Token: 0x04000537 RID: 1335
	[SerializeField]
	private string _displayName;

	// Token: 0x04000538 RID: 1336
	[TextArea]
	[SerializeField]
	private string _description;

	// Token: 0x04000539 RID: 1337
	[SerializeField]
	private Sprite _icon;

	// Token: 0x0400053A RID: 1338
	[SerializeField]
	private Sprite _programmerIcon;

	// Token: 0x0400053B RID: 1339
	[SerializeField]
	private SavableObjectID _savableObjectID;
}
