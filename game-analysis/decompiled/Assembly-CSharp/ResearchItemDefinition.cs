using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x020000A3 RID: 163
public abstract class ResearchItemDefinition : ScriptableObject
{
	// Token: 0x06000476 RID: 1142 RVA: 0x000187A3 File Offset: 0x000169A3
	public virtual void OnResearched()
	{
		Debug.Log("ResearchItemDefinition: OnResearched not implemented for " + base.name);
	}

	// Token: 0x06000477 RID: 1143 RVA: 0x000187BC File Offset: 0x000169BC
	public bool IsLocked()
	{
		if (this.PrerequisiteResearch.Count == 0)
		{
			return false;
		}
		foreach (ResearchItemDefinition researchItemDefinition in this.PrerequisiteResearch)
		{
			if (Singleton<ResearchManager>.Instance.IsResearchItemCompleted(researchItemDefinition))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000478 RID: 1144 RVA: 0x0001882C File Offset: 0x00016A2C
	public virtual bool IsResearched()
	{
		return Singleton<ResearchManager>.Instance.IsResearchItemCompleted(this);
	}

	// Token: 0x06000479 RID: 1145 RVA: 0x00018839 File Offset: 0x00016A39
	public virtual bool CanAfford()
	{
		return Singleton<ResearchManager>.Instance.CanAffordResearch(this.GetResearchTicketCost()) && Singleton<EconomyManager>.Instance.CanAfford(this.GetMoneyCost());
	}

	// Token: 0x0600047A RID: 1146 RVA: 0x0001885F File Offset: 0x00016A5F
	public virtual int GetResearchTicketCost()
	{
		return this._researchTicketsCost;
	}

	// Token: 0x0600047B RID: 1147 RVA: 0x00018867 File Offset: 0x00016A67
	public virtual float GetMoneyCost()
	{
		return this._moneyCost;
	}

	// Token: 0x0600047C RID: 1148 RVA: 0x0001886F File Offset: 0x00016A6F
	public virtual Sprite GetIcon()
	{
		return null;
	}

	// Token: 0x0600047D RID: 1149 RVA: 0x00018872 File Offset: 0x00016A72
	public virtual string GetName()
	{
		return "MISSING RESEARCH ITEM NAME";
	}

	// Token: 0x0600047E RID: 1150 RVA: 0x00018879 File Offset: 0x00016A79
	public virtual string GetDescription()
	{
		return "MISSING RESEARCH ITEM DESCRIPTION";
	}

	// Token: 0x0600047F RID: 1151 RVA: 0x00018880 File Offset: 0x00016A80
	public virtual SavableObjectID GetSavableObjectID()
	{
		Debug.LogError("ResearchItemDefinition: GetSavableObjectID not implemented for " + base.name);
		return SavableObjectID.INVALID;
	}

	// Token: 0x04000517 RID: 1303
	[FormerlySerializedAs("_researchCost")]
	[SerializeField]
	protected int _researchTicketsCost = 1;

	// Token: 0x04000518 RID: 1304
	[SerializeField]
	protected float _moneyCost;

	// Token: 0x04000519 RID: 1305
	public List<ResearchItemDefinition> PrerequisiteResearch = new List<ResearchItemDefinition>();
}
