using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Token: 0x02000006 RID: 6
[CreateAssetMenu(fileName = "New AutoMinerResourceDefinition", menuName = "Building/AutoMiner Resource Definition")]
[ExecuteAlways]
public class AutoMinerResourceDefinition : ScriptableObject
{
	// Token: 0x0600002A RID: 42 RVA: 0x00002893 File Offset: 0x00000A93
	private void OnValidate()
	{
		this._possibleOrePrefabs.Sort((WeightedOreChance a, WeightedOreChance b) => b.Weight.CompareTo(a.Weight));
	}

	// Token: 0x0600002B RID: 43 RVA: 0x000028BF File Offset: 0x00000ABF
	public ResourceType GetPrimaryResourceType()
	{
		if (this._possibleOrePrefabs == null || this._possibleOrePrefabs.Count == 0)
		{
			return ResourceType.INVALID;
		}
		return this._possibleOrePrefabs[0].OrePrefab.ResourceType;
	}

	// Token: 0x0600002C RID: 44 RVA: 0x000028F0 File Offset: 0x00000AF0
	public OrePiece GetOrePrefab(bool canProduceGems)
	{
		if (this._possibleOrePrefabs == null || this._possibleOrePrefabs.Count == 0)
		{
			return null;
		}
		List<WeightedOreChance> list = this._possibleOrePrefabs;
		if (!canProduceGems)
		{
			list = this._possibleOrePrefabs.Where((WeightedOreChance o) => o.OrePrefab.PieceType != PieceType.Gem).ToList<WeightedOreChance>();
		}
		float num = 0f;
		foreach (WeightedOreChance weightedOreChance in list)
		{
			num += weightedOreChance.Weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		foreach (WeightedOreChance weightedOreChance2 in list)
		{
			num3 += weightedOreChance2.Weight;
			if (num2 <= num3)
			{
				return weightedOreChance2.OrePrefab;
			}
		}
		return this._possibleOrePrefabs[this._possibleOrePrefabs.Count - 1].OrePrefab;
	}

	// Token: 0x0600002D RID: 45 RVA: 0x00002A1C File Offset: 0x00000C1C
	public string GetFormattedAvailableResourcesText(bool canProduceGems)
	{
		if (this._possibleOrePrefabs == null || this._possibleOrePrefabs.Count == 0)
		{
			return string.Concat(new string[]
			{
				"<color=#",
				global::Singleton<OreManager>.Instance.GetResourceColor(ResourceType.Copper).ToHexString(),
				">Copper</color> is not available in the demo. Will produce <color=#",
				global::Singleton<OreManager>.Instance.GetResourceColor(ResourceType.Iron).ToHexString(),
				">Iron</color> instead."
			});
		}
		float num = 0f;
		foreach (WeightedOreChance weightedOreChance in this._possibleOrePrefabs)
		{
			num += weightedOreChance.Weight;
		}
		List<string> list = new List<string>();
		foreach (WeightedOreChance weightedOreChance2 in this._possibleOrePrefabs)
		{
			float num2 = weightedOreChance2.Weight / num * 100f;
			string coloredFormattedResourcePieceString = global::Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(weightedOreChance2.OrePrefab.ResourceType, weightedOreChance2.OrePrefab.PieceType, weightedOreChance2.OrePrefab.PolishedPercent >= 1f);
			string text = ((num2 % 1f == 0f) ? string.Format("{0}%", (int)num2) : string.Format("{0:F1}%", num2));
			string text2 = coloredFormattedResourcePieceString + " - " + text;
			if (num2 < 10f)
			{
				text2 = "<size=80%>" + text2 + "</size>";
			}
			if (!canProduceGems && weightedOreChance2.OrePrefab.PieceType == PieceType.Gem)
			{
				text2 = "<s>" + text2 + "</s>";
			}
			list.Add(text2);
		}
		return string.Join("\n", list);
	}

	// Token: 0x04000035 RID: 53
	[Range(0f, 100f)]
	public float SpawnProbability = 80f;

	// Token: 0x04000036 RID: 54
	[Range(0f, 20f)]
	public float SpawnRate = 2f;

	// Token: 0x04000037 RID: 55
	[SerializeField]
	private List<WeightedOreChance> _possibleOrePrefabs = new List<WeightedOreChance>();
}
