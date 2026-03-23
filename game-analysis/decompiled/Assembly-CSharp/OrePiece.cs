using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000078 RID: 120
public class OrePiece : BaseSellableItem
{
	// Token: 0x06000327 RID: 807 RVA: 0x0000FE28 File Offset: 0x0000E028
	private void Start()
	{
		if (this._possibleMeshes.Length != 0)
		{
			if (!this.UseRandomMesh && this.MeshID >= this._possibleMeshes.Length)
			{
				Debug.Log(string.Format("Trying to use invalid MeshID {0} on {1}, using a random MeshID instead.", this.MeshID, Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(this.ResourceType, this.PieceType, this.PolishedPercent == 1f)));
				this.UseRandomMesh = true;
			}
			if (this.UseRandomMesh)
			{
				this.MeshID = Random.Range(0, this._possibleMeshes.Length);
			}
			if (this.MeshCollider != null)
			{
				this.MeshCollider.sharedMesh = this._possibleMeshes[this.MeshID];
			}
			this.MeshFilter.sharedMesh = this._possibleMeshes[this.MeshID];
		}
		if (this.UseRandomScale)
		{
			Vector3 vector = new Vector3(base.transform.localScale.x + Random.Range(-this.scaleVariance.x, this.scaleVariance.x), base.transform.localScale.y + Random.Range(-this.scaleVariance.y, this.scaleVariance.y), base.transform.localScale.z + Random.Range(-this.scaleVariance.z, this.scaleVariance.z));
			base.transform.localScale = vector;
		}
		this.RandomPriceMultiplier = Random.Range(0.9f, 1.1f);
	}

	// Token: 0x06000328 RID: 808 RVA: 0x0000FFB1 File Offset: 0x0000E1B1
	protected override void OnEnable()
	{
		base.OnEnable();
		OrePiece.AllOrePieces.Add(this);
	}

	// Token: 0x06000329 RID: 809 RVA: 0x0000FFC4 File Offset: 0x0000E1C4
	protected override void OnDisable()
	{
		base.OnDisable();
		OrePiece.AllOrePieces.Remove(this);
	}

	// Token: 0x0600032A RID: 810 RVA: 0x0000FFD8 File Offset: 0x0000E1D8
	public virtual Sprite GetIcon()
	{
		return this.InventoryIcon;
	}

	// Token: 0x0600032B RID: 811 RVA: 0x0000FFE0 File Offset: 0x0000E1E0
	public void AddPolish(float value)
	{
		if (this.PolishedPercent >= 1f)
		{
			return;
		}
		this.PolishedPercent = Mathf.Min(1f, this.PolishedPercent + value);
		if (this.PolishedPercent >= 1f)
		{
			this.CompletePolishing();
		}
	}

	// Token: 0x0600032C RID: 812 RVA: 0x0001001C File Offset: 0x0000E21C
	private void CompletePolishing()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (this.PolishedPrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(this.PolishedPrefab, base.transform.position, base.transform.rotation, null);
			if (orePiece != null)
			{
				this.AddPolish(100f);
				orePiece.RandomPriceMultiplier = this.RandomPriceMultiplier;
			}
			this.Delete();
			return;
		}
		if (this.PolishedMaterial != null)
		{
			base.GetComponent<Renderer>().sharedMaterial = this.PolishedMaterial;
		}
	}

	// Token: 0x0600032D RID: 813 RVA: 0x000100B3 File Offset: 0x0000E2B3
	public void AddSieveValue(float value)
	{
		if (this.SievePercent >= 1f)
		{
			return;
		}
		this.SievePercent = Mathf.Min(1f, this.SievePercent + value);
		if (this.SievePercent >= 1f)
		{
			this.CompleteSieving();
		}
	}

	// Token: 0x0600032E RID: 814 RVA: 0x000100F0 File Offset: 0x0000E2F0
	private void CompleteSieving()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (this.PossibleSievedPrefabs.Count == 0)
		{
			return;
		}
		OrePiece orePiece = null;
		if (this.PossibleSievedPrefabs.Count == 1)
		{
			orePiece = this.PossibleSievedPrefabs.First<WeightedOreChance>().OrePrefab;
		}
		else
		{
			float num = 0f;
			foreach (WeightedOreChance weightedOreChance in this.PossibleSievedPrefabs)
			{
				num += weightedOreChance.Weight;
			}
			float num2 = Random.value * num;
			float num3 = 0f;
			foreach (WeightedOreChance weightedOreChance2 in this.PossibleSievedPrefabs)
			{
				num3 += weightedOreChance2.Weight;
				if (num2 <= num3)
				{
					orePiece = weightedOreChance2.OrePrefab;
					break;
				}
			}
		}
		if (orePiece != null)
		{
			Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece, base.transform.position, base.transform.rotation, null);
		}
		this.Delete();
	}

	// Token: 0x0600032F RID: 815 RVA: 0x00010228 File Offset: 0x0000E428
	public void CompleteClusterBreaking()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (this.MaxClusterBreakerSpawns < 1)
		{
			return;
		}
		if (this.PossibleClusterBreakerPrefabs.Count == 0)
		{
			return;
		}
		int num = Random.Range(this.MinClusterBreakerSpawns, this.MaxClusterBreakerSpawns + 1);
		if (this.UseSameClusterBreakerPrefabForAllDrops)
		{
			OrePiece orePiece = this.SelectClusterBreakerPrefab();
			if (orePiece != null)
			{
				for (int i = 0; i < num; i++)
				{
					Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece, base.transform.position, base.transform.rotation, null);
				}
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				OrePiece orePiece2 = this.SelectClusterBreakerPrefab();
				if (orePiece2 != null)
				{
					Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece2, base.transform.position, base.transform.rotation, null);
				}
			}
		}
		this.Delete();
	}

	// Token: 0x06000330 RID: 816 RVA: 0x00010300 File Offset: 0x0000E500
	private OrePiece SelectClusterBreakerPrefab()
	{
		OrePiece orePiece = null;
		if (this.PossibleClusterBreakerPrefabs.Count == 1)
		{
			orePiece = this.PossibleClusterBreakerPrefabs.First<WeightedOreChance>().OrePrefab;
		}
		else
		{
			float num = 0f;
			foreach (WeightedOreChance weightedOreChance in this.PossibleClusterBreakerPrefabs)
			{
				num += weightedOreChance.Weight;
			}
			float num2 = Random.value * num;
			float num3 = 0f;
			foreach (WeightedOreChance weightedOreChance2 in this.PossibleClusterBreakerPrefabs)
			{
				num3 += weightedOreChance2.Weight;
				if (num2 <= num3)
				{
					orePiece = weightedOreChance2.OrePrefab;
					break;
				}
			}
		}
		return orePiece;
	}

	// Token: 0x06000331 RID: 817 RVA: 0x000103E8 File Offset: 0x0000E5E8
	public OrePiece ConvertToPlate()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (this.PlatePrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(this.PlatePrefab, base.transform.position, base.transform.rotation, null);
			if (orePiece != null)
			{
				orePiece.AddPolish(this.PolishedPercent);
				this.Delete();
				return orePiece;
			}
			if (this.OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.OverrideCrushedSound, base.transform.position, 1f, 1f, true, false);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position, 1f, 1f, true, false);
			}
		}
		this.Delete();
		return null;
	}

	// Token: 0x06000332 RID: 818 RVA: 0x000104C4 File Offset: 0x0000E6C4
	public OrePiece ConvertToRod()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (this.RodPrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(this.RodPrefab, base.transform.position, base.transform.rotation, null);
			if (orePiece != null)
			{
				orePiece.AddPolish(this.PolishedPercent);
				this.Delete();
				return orePiece;
			}
			if (this.OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.OverrideCrushedSound, base.transform.position, 1f, 1f, true, false);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position, 1f, 1f, true, false);
			}
		}
		this.Delete();
		return null;
	}

	// Token: 0x06000333 RID: 819 RVA: 0x000105A0 File Offset: 0x0000E7A0
	public OrePiece ConvertToThreaded()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (this.ThreadedPrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(this.ThreadedPrefab, base.transform.position, base.transform.rotation, null);
			if (orePiece != null)
			{
				orePiece.AddPolish(this.PolishedPercent);
				this.Delete();
				return orePiece;
			}
			if (this.OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.OverrideCrushedSound, base.transform.position, 1f, 1f, true, false);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position, 1f, 1f, true, false);
			}
			this.Delete();
		}
		return null;
	}

	// Token: 0x06000334 RID: 820 RVA: 0x0001067C File Offset: 0x0000E87C
	public OrePiece ConvertToPipe()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (this.PipePrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(this.PipePrefab, base.transform.position, base.transform.rotation, null);
			if (orePiece != null)
			{
				orePiece.AddPolish(this.PolishedPercent);
				this.Delete();
				return orePiece;
			}
			if (this.OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.OverrideCrushedSound, base.transform.position, 1f, 1f, true, false);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position, 1f, 1f, true, false);
			}
		}
		this.Delete();
		return null;
	}

	// Token: 0x06000335 RID: 821 RVA: 0x00010758 File Offset: 0x0000E958
	public bool TryConvertToCrushed()
	{
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		if (this.CrushedPrefab != null)
		{
			int num = 2;
			for (int i = 0; i < num; i++)
			{
				Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(this.CrushedPrefab, base.transform.position, base.transform.rotation, null);
			}
			if (this.OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.OverrideCrushedSound, base.transform.position, 1f, 1f, true, false);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position, 1f, 1f, true, false);
			}
			this.Delete();
			return true;
		}
		return false;
	}

	// Token: 0x06000336 RID: 822 RVA: 0x00010826 File Offset: 0x0000EA26
	public override float GetSellValue()
	{
		return Mathf.Round(this.BaseSellValue * this.RandomPriceMultiplier * 100f) / 100f;
	}

	// Token: 0x06000337 RID: 823 RVA: 0x00010846 File Offset: 0x0000EA46
	public void Delete()
	{
		Singleton<OrePiecePoolManager>.Instance.ReturnToPool(this);
	}

	// Token: 0x06000338 RID: 824 RVA: 0x00010854 File Offset: 0x0000EA54
	public void SellAfterDelay(float delay = 2f)
	{
		if (this.CurrentMagnetTool != null)
		{
			this.CurrentMagnetTool.DetachBody(base.Rb);
		}
		base.gameObject.tag = "MarkedForDestruction";
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.tag = "MarkedForDestruction";
		}
		base.StartCoroutine(this.DelayThenSell(delay));
	}

	// Token: 0x06000339 RID: 825 RVA: 0x000108CA File Offset: 0x0000EACA
	private IEnumerator DelayThenSell(float delayBeforeSelling)
	{
		yield return new WaitForSeconds(delayBeforeSelling);
		if (this == null || !base.isActiveAndEnabled)
		{
			yield break;
		}
		float sellValue = this.GetSellValue();
		Singleton<EconomyManager>.Instance.AddMoney(sellValue);
		Singleton<EconomyManager>.Instance.DispatchOnItemSoldEvent();
		QuestManager instance = Singleton<QuestManager>.Instance;
		if (instance != null)
		{
			instance.OnResourceDeposited(this.ResourceType, this.PieceType, this.PolishedPercent, 1);
		}
		this.Delete();
		yield break;
	}

	// Token: 0x04000309 RID: 777
	public ResourceType ResourceType;

	// Token: 0x0400030A RID: 778
	public PieceType PieceType;

	// Token: 0x0400030B RID: 779
	public bool IsPolished;

	// Token: 0x0400030C RID: 780
	public Sprite InventoryIcon;

	// Token: 0x0400030D RID: 781
	public float VolumeInsideBox = 0.1f;

	// Token: 0x0400030E RID: 782
	[Obsolete]
	public float PolishedPercent;

	// Token: 0x0400030F RID: 783
	public float SievePercent;

	// Token: 0x04000310 RID: 784
	public float RandomPriceMultiplier = 1f;

	// Token: 0x04000311 RID: 785
	public SoundDefinition OverrideCrushedSound;

	// Token: 0x04000312 RID: 786
	public GameObject CrushedPrefab;

	// Token: 0x04000313 RID: 787
	public GameObject IngotPrefab;

	// Token: 0x04000314 RID: 788
	public GameObject PlatePrefab;

	// Token: 0x04000315 RID: 789
	public GameObject PipePrefab;

	// Token: 0x04000316 RID: 790
	public GameObject RodPrefab;

	// Token: 0x04000317 RID: 791
	public GameObject ThreadedPrefab;

	// Token: 0x04000318 RID: 792
	public bool MakesPolishingMachineDirty;

	// Token: 0x04000319 RID: 793
	public GameObject PolishedPrefab;

	// Token: 0x0400031A RID: 794
	public Material PolishedMaterial;

	// Token: 0x0400031B RID: 795
	public List<WeightedOreChance> PossibleSievedPrefabs;

	// Token: 0x0400031C RID: 796
	public bool UseSameClusterBreakerPrefabForAllDrops;

	// Token: 0x0400031D RID: 797
	public int MinClusterBreakerSpawns;

	// Token: 0x0400031E RID: 798
	public int MaxClusterBreakerSpawns;

	// Token: 0x0400031F RID: 799
	public List<WeightedOreChance> PossibleClusterBreakerPrefabs;

	// Token: 0x04000320 RID: 800
	[SerializeField]
	private Mesh[] _possibleMeshes;

	// Token: 0x04000321 RID: 801
	public MeshFilter MeshFilter;

	// Token: 0x04000322 RID: 802
	public MeshCollider MeshCollider;

	// Token: 0x04000323 RID: 803
	public bool UseRandomMesh = true;

	// Token: 0x04000324 RID: 804
	public int MeshID;

	// Token: 0x04000325 RID: 805
	public bool UseRandomScale = true;

	// Token: 0x04000326 RID: 806
	[SerializeField]
	private Vector3 scaleVariance = new Vector3(0.25f, 0.25f, 0.25f);

	// Token: 0x04000327 RID: 807
	public ToolMagnet CurrentMagnetTool;

	// Token: 0x04000328 RID: 808
	public HashSet<BaseBasket> BasketsThisIsInside = new HashSet<BaseBasket>();

	// Token: 0x04000329 RID: 809
	public static List<OrePiece> AllOrePieces = new List<OrePiece>();
}
