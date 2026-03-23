using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ResearchItemDefinition : ScriptableObject
{
	[FormerlySerializedAs("_researchCost")]
	[SerializeField]
	protected int _researchTicketsCost = 1;

	[SerializeField]
	protected float _moneyCost;

	public List<ResearchItemDefinition> PrerequisiteResearch = new List<ResearchItemDefinition>();

	public bool IsLockedInDemo = true;

	public virtual void OnResearched()
	{
		Debug.Log("ResearchItemDefinition: OnResearched not implemented for " + base.name);
	}

	public bool IsLocked()
	{
		if (PrerequisiteResearch.Count == 0)
		{
			return false;
		}
		foreach (ResearchItemDefinition item in PrerequisiteResearch)
		{
			if (Singleton<ResearchManager>.Instance.IsResearchItemCompleted(item))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsResearched()
	{
		return Singleton<ResearchManager>.Instance.IsResearchItemCompleted(this);
	}

	public virtual bool CanAfford()
	{
		if (Singleton<ResearchManager>.Instance.CanAffordResearch(GetResearchTicketCost()))
		{
			return Singleton<EconomyManager>.Instance.CanAfford(GetMoneyCost());
		}
		return false;
	}

	public virtual int GetResearchTicketCost()
	{
		return _researchTicketsCost;
	}

	public virtual float GetMoneyCost()
	{
		return _moneyCost;
	}

	public virtual Sprite GetIcon()
	{
		return null;
	}

	public virtual string GetName()
	{
		return "MISSING RESEARCH ITEM NAME";
	}

	public virtual string GetDescription()
	{
		return "MISSING RESEARCH ITEM DESCRIPTION";
	}

	public virtual SavableObjectID GetSavableObjectID()
	{
		Debug.LogError("ResearchItemDefinition: GetSavableObjectID not implemented for " + base.name);
		return SavableObjectID.INVALID;
	}
}
