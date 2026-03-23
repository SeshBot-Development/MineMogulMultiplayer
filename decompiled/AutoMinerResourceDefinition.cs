using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New AutoMinerResourceDefinition", menuName = "Building/AutoMiner Resource Definition")]
[ExecuteAlways]
public class AutoMinerResourceDefinition : ScriptableObject
{
	[Range(0f, 100f)]
	public float SpawnProbability = 80f;

	[Range(0f, 20f)]
	public float SpawnRate = 2f;

	[SerializeField]
	private List<WeightedOreChance> _possibleOrePrefabs = new List<WeightedOreChance>();

	private void OnValidate()
	{
		_possibleOrePrefabs.Sort((WeightedOreChance a, WeightedOreChance b) => b.Weight.CompareTo(a.Weight));
	}

	public ResourceType GetPrimaryResourceType()
	{
		if (_possibleOrePrefabs == null || _possibleOrePrefabs.Count == 0)
		{
			return ResourceType.INVALID;
		}
		return _possibleOrePrefabs[0].OrePrefab.ResourceType;
	}

	public OrePiece GetOrePrefab(bool canProduceGems)
	{
		if (_possibleOrePrefabs == null || _possibleOrePrefabs.Count == 0)
		{
			return null;
		}
		List<WeightedOreChance> list = _possibleOrePrefabs;
		if (!canProduceGems)
		{
			list = _possibleOrePrefabs.Where((WeightedOreChance o) => o.OrePrefab.PieceType != PieceType.Gem).ToList();
		}
		float num = 0f;
		foreach (WeightedOreChance item in list)
		{
			num += item.Weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		foreach (WeightedOreChance item2 in list)
		{
			num3 += item2.Weight;
			if (num2 <= num3)
			{
				return item2.OrePrefab;
			}
		}
		return _possibleOrePrefabs[_possibleOrePrefabs.Count - 1].OrePrefab;
	}

	public string GetFormattedAvailableResourcesText(bool canProduceGems)
	{
		if (_possibleOrePrefabs == null || _possibleOrePrefabs.Count == 0)
		{
			return "<color=#" + Singleton<OreManager>.Instance.GetResourceColor(ResourceType.Copper).ToHexString() + ">Copper</color> is not available in the demo. Will produce <color=#" + Singleton<OreManager>.Instance.GetResourceColor(ResourceType.Iron).ToHexString() + ">Iron</color> instead.";
		}
		float num = 0f;
		foreach (WeightedOreChance possibleOrePrefab in _possibleOrePrefabs)
		{
			num += possibleOrePrefab.Weight;
		}
		List<string> list = new List<string>();
		foreach (WeightedOreChance possibleOrePrefab2 in _possibleOrePrefabs)
		{
			float num2 = possibleOrePrefab2.Weight / num * 100f;
			string coloredFormattedResourcePieceString = Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(possibleOrePrefab2.OrePrefab.ResourceType, possibleOrePrefab2.OrePrefab.PieceType, possibleOrePrefab2.OrePrefab.PolishedPercent >= 1f);
			string text = ((num2 % 1f == 0f) ? $"{(int)num2}%" : $"{num2:F1}%");
			string text2 = coloredFormattedResourcePieceString + " - " + text;
			if (num2 < 10f)
			{
				text2 = "<size=80%>" + text2 + "</size>";
			}
			if (!canProduceGems && possibleOrePrefab2.OrePrefab.PieceType == PieceType.Gem)
			{
				text2 = "<s>" + text2 + "</s>";
			}
			list.Add(text2);
		}
		return string.Join("\n", list);
	}
}
