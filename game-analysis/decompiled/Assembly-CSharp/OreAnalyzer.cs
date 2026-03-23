using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x02000071 RID: 113
public class OreAnalyzer : MonoBehaviour
{
	// Token: 0x060002FF RID: 767 RVA: 0x0000F07B File Offset: 0x0000D27B
	private void OnEnable()
	{
		this.SetDefaultText();
		this._timeUntilNextItemPerMinUpdate = 1f;
		this._timeUntilClearRecentlyCounted = 1f;
	}

	// Token: 0x06000300 RID: 768 RVA: 0x0000F0A4 File Offset: 0x0000D2A4
	private void OnTriggerEnter(Collider other)
	{
		this.AddToItemCounter(other);
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this.AnalyzeOre(component);
			return;
		}
		BuildingCrate component2 = other.GetComponent<BuildingCrate>();
		if (component2 != null)
		{
			this.AnalyzeBuildingCrate(component2);
			return;
		}
		BaseHeldTool componentInParent = other.GetComponentInParent<BaseHeldTool>();
		if (componentInParent != null)
		{
			this.AnalyzeTool(componentInParent);
			return;
		}
		BaseSellableItem component3 = other.GetComponent<BaseSellableItem>();
		if (component3 != null)
		{
			this.AnalyzeGenericSellable(component3);
			return;
		}
		PlayerController component4 = other.GetComponent<PlayerController>();
		if (component4 != null)
		{
			this.AnalyzePlayer(component4);
			return;
		}
	}

	// Token: 0x06000301 RID: 769 RVA: 0x0000F134 File Offset: 0x0000D334
	private void AddToItemCounter(Collider other)
	{
		BasePhysicsObject component = other.GetComponent<BasePhysicsObject>();
		if (component != null)
		{
			if (this._recentlyCountedObjects.Contains(component))
			{
				return;
			}
			this._recentlyCountedObjects.Add(component);
		}
		this.entryTimes.Add(Time.time);
		this.UpdateItemsPerMin();
	}

	// Token: 0x06000302 RID: 770 RVA: 0x0000F184 File Offset: 0x0000D384
	private void UpdateItemsPerMin()
	{
		this._timeUntilNextItemPerMinUpdate = 1f;
		float cutoff = Time.time - 60f;
		this.entryTimes.RemoveAll((float t) => t < cutoff);
		this.ItemsPerMinuteText.text = string.Format("{0} items / min", this.entryTimes.Count);
	}

	// Token: 0x06000303 RID: 771 RVA: 0x0000F1F5 File Offset: 0x0000D3F5
	private void Update()
	{
		if ((in this._timeUntilNextItemPerMinUpdate) < 0)
		{
			this.UpdateItemsPerMin();
		}
		if ((in this._timeUntilClearRecentlyCounted) < 0)
		{
			this._timeUntilClearRecentlyCounted = 2f;
			this._recentlyCountedObjects.Clear();
		}
	}

	// Token: 0x06000304 RID: 772 RVA: 0x0000F234 File Offset: 0x0000D434
	private void AnalyzeOre(OrePiece ore)
	{
		this.PlayAnalyzeEffect();
		this.OreTypeText.text = Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(ore.ResourceType, ore.PieceType, false);
		if (ore.IsPolished)
		{
			this.PolishedText.text = "Polished";
		}
		else if (ore.MakesPolishingMachineDirty)
		{
			this.PolishedText.text = "Dirty";
		}
		else if (ore.PolishedPrefab != null)
		{
			if (ore.PolishedPrefab.GetComponent<OrePiece>().MakesPolishingMachineDirty)
			{
				this.PolishedText.text = "Dirty";
			}
			else
			{
				this.PolishedText.text = "Unpolished";
			}
		}
		else
		{
			this.PolishedText.text = "";
		}
		this.OreWeightText.text = string.Format("{0:0}%", ore.RandomPriceMultiplier * 100f);
		this.SellValueText.text = "$ " + ore.GetSellValue().ToString();
	}

	// Token: 0x06000305 RID: 773 RVA: 0x0000F33C File Offset: 0x0000D53C
	private void AnalyzeTool(BaseHeldTool tool)
	{
		this.PlayAnalyzeEffect();
		this.OreTypeText.text = tool.GetObjectName();
		this.PolishedText.text = "";
		this.OreWeightText.text = "";
		this.SellValueText.text = "$ " + tool.GetSellValue().ToString();
	}

	// Token: 0x06000306 RID: 774 RVA: 0x0000F3A4 File Offset: 0x0000D5A4
	private void AnalyzeBuildingCrate(BuildingCrate crate)
	{
		this.PlayAnalyzeEffect();
		this.OreTypeText.text = crate.GetObjectName();
		this.PolishedText.text = "";
		this.OreWeightText.text = "";
		this.SellValueText.text = "$ " + crate.GetSellValue().ToString();
	}

	// Token: 0x06000307 RID: 775 RVA: 0x0000F40C File Offset: 0x0000D60C
	private void AnalyzeGenericSellable(BaseSellableItem sellableItem)
	{
		this.PlayAnalyzeEffect();
		this.OreTypeText.text = "Unknown";
		this.PolishedText.text = "";
		this.OreWeightText.text = "";
		this.SellValueText.text = "$ " + sellableItem.GetSellValue().ToString();
	}

	// Token: 0x06000308 RID: 776 RVA: 0x0000F474 File Offset: 0x0000D674
	private void AnalyzePlayer(PlayerController player)
	{
		this.PlayAnalyzeEffect();
		this.OreTypeText.text = "Organic Material";
		this.PolishedText.text = "";
		this.OreWeightText.text = "";
		this.SellValueText.text = "$ 0.00";
	}

	// Token: 0x06000309 RID: 777 RVA: 0x0000F4C8 File Offset: 0x0000D6C8
	private void SetDefaultText()
	{
		this.OreTypeText.text = "";
		this.PolishedText.text = "";
		this.OreWeightText.text = "";
		this.SellValueText.text = "$ ";
	}

	// Token: 0x0600030A RID: 778 RVA: 0x0000F515 File Offset: 0x0000D715
	private void PlayAnalyzeEffect()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.AnalyzeSoundEffect, this.AudioPosition.position, 1f, 1f, true, false);
	}

	// Token: 0x040002E9 RID: 745
	public SoundDefinition AnalyzeSoundEffect;

	// Token: 0x040002EA RID: 746
	public Transform AudioPosition;

	// Token: 0x040002EB RID: 747
	public TMP_Text OreTypeText;

	// Token: 0x040002EC RID: 748
	public TMP_Text OreWeightText;

	// Token: 0x040002ED RID: 749
	public TMP_Text SellValueText;

	// Token: 0x040002EE RID: 750
	public TMP_Text PolishedText;

	// Token: 0x040002EF RID: 751
	public TMP_Text ItemsPerMinuteText;

	// Token: 0x040002F0 RID: 752
	private readonly List<float> entryTimes = new List<float>();

	// Token: 0x040002F1 RID: 753
	private TimeUntil _timeUntilNextItemPerMinUpdate;

	// Token: 0x040002F2 RID: 754
	private TimeUntil _timeUntilClearRecentlyCounted;

	// Token: 0x040002F3 RID: 755
	private readonly HashSet<BasePhysicsObject> _recentlyCountedObjects = new HashSet<BasePhysicsObject>();
}
