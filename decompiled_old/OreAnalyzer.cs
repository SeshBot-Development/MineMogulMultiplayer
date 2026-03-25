using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OreAnalyzer : MonoBehaviour
{
	public SoundDefinition AnalyzeSoundEffect;

	public Transform AudioPosition;

	public TMP_Text OreTypeText;

	public TMP_Text OreWeightText;

	public TMP_Text SellValueText;

	public TMP_Text PolishedText;

	public TMP_Text ItemsPerMinuteText;

	private readonly List<float> entryTimes = new List<float>();

	private TimeUntil _timeUntilNextItemPerMinUpdate;

	private TimeUntil _timeUntilClearRecentlyCounted;

	private readonly HashSet<BasePhysicsObject> _recentlyCountedObjects = new HashSet<BasePhysicsObject>();

	private void OnEnable()
	{
		SetDefaultText();
		_timeUntilNextItemPerMinUpdate = 1f;
		_timeUntilClearRecentlyCounted = 1f;
	}

	private void OnTriggerEnter(Collider other)
	{
		AddToItemCounter(other);
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			AnalyzeOre(component);
			return;
		}
		BuildingCrate component2 = other.GetComponent<BuildingCrate>();
		if (component2 != null)
		{
			AnalyzeBuildingCrate(component2);
			return;
		}
		BaseHeldTool componentInParent = other.GetComponentInParent<BaseHeldTool>();
		if (componentInParent != null)
		{
			AnalyzeTool(componentInParent);
			return;
		}
		BaseSellableItem component3 = other.GetComponent<BaseSellableItem>();
		if (component3 != null)
		{
			AnalyzeGenericSellable(component3);
			return;
		}
		PlayerController component4 = other.GetComponent<PlayerController>();
		if (component4 != null)
		{
			AnalyzePlayer(component4);
		}
	}

	private void AddToItemCounter(Collider other)
	{
		BasePhysicsObject component = other.GetComponent<BasePhysicsObject>();
		if (component != null)
		{
			if (_recentlyCountedObjects.Contains(component))
			{
				return;
			}
			_recentlyCountedObjects.Add(component);
		}
		entryTimes.Add(Time.time);
		UpdateItemsPerMin();
	}

	private void UpdateItemsPerMin()
	{
		_timeUntilNextItemPerMinUpdate = 1f;
		float cutoff = Time.time - 60f;
		entryTimes.RemoveAll((float t) => t < cutoff);
		ItemsPerMinuteText.text = $"{entryTimes.Count} items / min";
	}

	private void Update()
	{
		if (_timeUntilNextItemPerMinUpdate < 0)
		{
			UpdateItemsPerMin();
		}
		if (_timeUntilClearRecentlyCounted < 0)
		{
			_timeUntilClearRecentlyCounted = 2f;
			_recentlyCountedObjects.Clear();
		}
	}

	private void AnalyzeOre(OrePiece ore)
	{
		PlayAnalyzeEffect();
		OreTypeText.text = Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(ore.ResourceType, ore.PieceType);
		if (ore.IsPolished)
		{
			PolishedText.text = "Polished";
		}
		else if (ore.MakesPolishingMachineDirty)
		{
			PolishedText.text = "Dirty";
		}
		else if (ore.PolishedPrefab != null)
		{
			if (ore.PolishedPrefab.GetComponent<OrePiece>().MakesPolishingMachineDirty)
			{
				PolishedText.text = "Dirty";
			}
			else
			{
				PolishedText.text = "Unpolished";
			}
		}
		else
		{
			PolishedText.text = "";
		}
		OreWeightText.text = $"{ore.RandomPriceMultiplier * 100f:0}%";
		SellValueText.text = "$ " + ore.GetSellValue();
	}

	private void AnalyzeTool(BaseHeldTool tool)
	{
		PlayAnalyzeEffect();
		OreTypeText.text = tool.GetObjectName();
		PolishedText.text = "";
		OreWeightText.text = "";
		SellValueText.text = "$ " + tool.GetSellValue();
	}

	private void AnalyzeBuildingCrate(BuildingCrate crate)
	{
		PlayAnalyzeEffect();
		OreTypeText.text = crate.GetObjectName();
		PolishedText.text = "";
		OreWeightText.text = "";
		SellValueText.text = "$ " + crate.GetSellValue();
	}

	private void AnalyzeGenericSellable(BaseSellableItem sellableItem)
	{
		PlayAnalyzeEffect();
		OreTypeText.text = "Unknown";
		PolishedText.text = "";
		OreWeightText.text = "";
		SellValueText.text = "$ " + sellableItem.GetSellValue();
	}

	private void AnalyzePlayer(PlayerController player)
	{
		PlayAnalyzeEffect();
		OreTypeText.text = "Organic Material";
		PolishedText.text = "";
		OreWeightText.text = "";
		SellValueText.text = "$ 0.00";
	}

	private void SetDefaultText()
	{
		OreTypeText.text = "";
		PolishedText.text = "";
		OreWeightText.text = "";
		SellValueText.text = "$ ";
	}

	private void PlayAnalyzeEffect()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(AnalyzeSoundEffect, AudioPosition.position);
	}
}
