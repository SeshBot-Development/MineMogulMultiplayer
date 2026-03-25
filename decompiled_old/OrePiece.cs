using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrePiece : BaseSellableItem
{
	public ResourceType ResourceType;

	public PieceType PieceType;

	public bool IsPolished;

	public Sprite InventoryIcon;

	public float VolumeInsideBox = 0.1f;

	[Obsolete]
	public float PolishedPercent;

	public float SievePercent;

	public float RandomPriceMultiplier = 1f;

	public SoundDefinition OverrideCrushedSound;

	public GameObject CrushedPrefab;

	public GameObject IngotPrefab;

	public GameObject PlatePrefab;

	public GameObject PipePrefab;

	public GameObject RodPrefab;

	public GameObject ThreadedPrefab;

	public bool MakesPolishingMachineDirty;

	public GameObject PolishedPrefab;

	public Material PolishedMaterial;

	public List<WeightedOreChance> PossibleSievedPrefabs;

	public bool UseSameClusterBreakerPrefabForAllDrops;

	public int MinClusterBreakerSpawns;

	public int MaxClusterBreakerSpawns;

	public List<WeightedOreChance> PossibleClusterBreakerPrefabs;

	[SerializeField]
	private Mesh[] _possibleMeshes;

	public MeshFilter MeshFilter;

	public MeshCollider MeshCollider;

	public bool UseRandomMesh = true;

	public int MeshID;

	public bool UseRandomScale = true;

	[SerializeField]
	private Vector3 scaleVariance = new Vector3(0.25f, 0.25f, 0.25f);

	public ToolMagnet CurrentMagnetTool;

	public HashSet<BaseBasket> BasketsThisIsInside = new HashSet<BaseBasket>();

	public static List<OrePiece> AllOrePieces = new List<OrePiece>();

	private void Start()
	{
		if (_possibleMeshes.Length != 0)
		{
			if (!UseRandomMesh && MeshID >= _possibleMeshes.Length)
			{
				Debug.Log($"Trying to use invalid MeshID {MeshID} on {Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(ResourceType, PieceType, PolishedPercent == 1f)}, using a random MeshID instead.");
				UseRandomMesh = true;
			}
			if (UseRandomMesh)
			{
				MeshID = UnityEngine.Random.Range(0, _possibleMeshes.Length);
			}
			if (MeshCollider != null)
			{
				MeshCollider.sharedMesh = _possibleMeshes[MeshID];
			}
			MeshFilter.sharedMesh = _possibleMeshes[MeshID];
		}
		if (UseRandomScale)
		{
			Vector3 localScale = new Vector3(base.transform.localScale.x + UnityEngine.Random.Range(0f - scaleVariance.x, scaleVariance.x), base.transform.localScale.y + UnityEngine.Random.Range(0f - scaleVariance.y, scaleVariance.y), base.transform.localScale.z + UnityEngine.Random.Range(0f - scaleVariance.z, scaleVariance.z));
			base.transform.localScale = localScale;
		}
		RandomPriceMultiplier = UnityEngine.Random.Range(0.9f, 1.1f);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		AllOrePieces.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		AllOrePieces.Remove(this);
	}

	public virtual Sprite GetIcon()
	{
		return InventoryIcon;
	}

	public void AddPolish(float value)
	{
		if (!(PolishedPercent >= 1f))
		{
			PolishedPercent = Mathf.Min(1f, PolishedPercent + value);
			if (PolishedPercent >= 1f)
			{
				CompletePolishing();
			}
		}
	}

	private void CompletePolishing()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (PolishedPrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(PolishedPrefab, base.transform.position, base.transform.rotation);
			if (orePiece != null)
			{
				AddPolish(100f);
				orePiece.RandomPriceMultiplier = RandomPriceMultiplier;
			}
			Delete();
		}
		else if (PolishedMaterial != null)
		{
			GetComponent<Renderer>().sharedMaterial = PolishedMaterial;
		}
	}

	public void AddSieveValue(float value)
	{
		if (!(SievePercent >= 1f))
		{
			SievePercent = Mathf.Min(1f, SievePercent + value);
			if (SievePercent >= 1f)
			{
				CompleteSieving();
			}
		}
	}

	private void CompleteSieving()
	{
		if (!base.gameObject.activeSelf || PossibleSievedPrefabs.Count == 0)
		{
			return;
		}
		OrePiece orePiece = null;
		if (PossibleSievedPrefabs.Count == 1)
		{
			orePiece = PossibleSievedPrefabs.First().OrePrefab;
		}
		else
		{
			float num = 0f;
			foreach (WeightedOreChance possibleSievedPrefab in PossibleSievedPrefabs)
			{
				num += possibleSievedPrefab.Weight;
			}
			float num2 = UnityEngine.Random.value * num;
			float num3 = 0f;
			foreach (WeightedOreChance possibleSievedPrefab2 in PossibleSievedPrefabs)
			{
				num3 += possibleSievedPrefab2.Weight;
				if (num2 <= num3)
				{
					orePiece = possibleSievedPrefab2.OrePrefab;
					break;
				}
			}
		}
		if (orePiece != null)
		{
			Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece, base.transform.position, base.transform.rotation);
		}
		Delete();
	}

	public void CompleteClusterBreaking()
	{
		if (!base.gameObject.activeSelf || MaxClusterBreakerSpawns < 1 || PossibleClusterBreakerPrefabs.Count == 0)
		{
			return;
		}
		int num = UnityEngine.Random.Range(MinClusterBreakerSpawns, MaxClusterBreakerSpawns + 1);
		if (UseSameClusterBreakerPrefabForAllDrops)
		{
			OrePiece orePiece = SelectClusterBreakerPrefab();
			if (orePiece != null)
			{
				for (int i = 0; i < num; i++)
				{
					Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece, base.transform.position, base.transform.rotation);
				}
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				OrePiece orePiece2 = SelectClusterBreakerPrefab();
				if (orePiece2 != null)
				{
					Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece2, base.transform.position, base.transform.rotation);
				}
			}
		}
		Delete();
	}

	private OrePiece SelectClusterBreakerPrefab()
	{
		OrePiece result = null;
		if (PossibleClusterBreakerPrefabs.Count == 1)
		{
			result = PossibleClusterBreakerPrefabs.First().OrePrefab;
		}
		else
		{
			float num = 0f;
			foreach (WeightedOreChance possibleClusterBreakerPrefab in PossibleClusterBreakerPrefabs)
			{
				num += possibleClusterBreakerPrefab.Weight;
			}
			float num2 = UnityEngine.Random.value * num;
			float num3 = 0f;
			foreach (WeightedOreChance possibleClusterBreakerPrefab2 in PossibleClusterBreakerPrefabs)
			{
				num3 += possibleClusterBreakerPrefab2.Weight;
				if (num2 <= num3)
				{
					result = possibleClusterBreakerPrefab2.OrePrefab;
					break;
				}
			}
		}
		return result;
	}

	public OrePiece ConvertToPlate()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (PlatePrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(PlatePrefab, base.transform.position, base.transform.rotation);
			if (orePiece != null)
			{
				orePiece.AddPolish(PolishedPercent);
				Delete();
				return orePiece;
			}
			if (OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(OverrideCrushedSound, base.transform.position);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position);
			}
		}
		Delete();
		return null;
	}

	public OrePiece ConvertToRod()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (RodPrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(RodPrefab, base.transform.position, base.transform.rotation);
			if (orePiece != null)
			{
				orePiece.AddPolish(PolishedPercent);
				Delete();
				return orePiece;
			}
			if (OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(OverrideCrushedSound, base.transform.position);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position);
			}
		}
		Delete();
		return null;
	}

	public OrePiece ConvertToThreaded()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (ThreadedPrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(ThreadedPrefab, base.transform.position, base.transform.rotation);
			if (orePiece != null)
			{
				orePiece.AddPolish(PolishedPercent);
				Delete();
				return orePiece;
			}
			if (OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(OverrideCrushedSound, base.transform.position);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position);
			}
			Delete();
		}
		return null;
	}

	public OrePiece ConvertToPipe()
	{
		if (!base.gameObject.activeSelf)
		{
			return null;
		}
		if (PipePrefab != null)
		{
			OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(PipePrefab, base.transform.position, base.transform.rotation);
			if (orePiece != null)
			{
				orePiece.AddPolish(PolishedPercent);
				Delete();
				return orePiece;
			}
			if (OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(OverrideCrushedSound, base.transform.position);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position);
			}
		}
		Delete();
		return null;
	}

	public bool TryConvertToCrushed()
	{
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		if (CrushedPrefab != null)
		{
			int num = 2;
			for (int i = 0; i < num; i++)
			{
				Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(CrushedPrefab, base.transform.position, base.transform.rotation);
			}
			if (OverrideCrushedSound != null)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(OverrideCrushedSound, base.transform.position);
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Ore_Crush, base.transform.position);
			}
			Delete();
			return true;
		}
		return false;
	}

	public override float GetSellValue()
	{
		return Mathf.Round(BaseSellValue * RandomPriceMultiplier * 100f) / 100f;
	}

	public void Delete()
	{
		Singleton<OrePiecePoolManager>.Instance.ReturnToPool(this);
	}

	public void SellAfterDelay(float delay = 2f)
	{
		if (CurrentMagnetTool != null)
		{
			CurrentMagnetTool.DetachBody(base.Rb);
		}
		base.gameObject.tag = "MarkedForDestruction";
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.tag = "MarkedForDestruction";
		}
		StartCoroutine(DelayThenSell(delay));
	}

	private IEnumerator DelayThenSell(float delayBeforeSelling)
	{
		yield return new WaitForSeconds(delayBeforeSelling);
		if (!(this == null) && base.isActiveAndEnabled)
		{
			float sellValue = GetSellValue();
			Singleton<EconomyManager>.Instance.AddMoney(sellValue);
			Singleton<EconomyManager>.Instance.DispatchOnItemSoldEvent();
			Singleton<QuestManager>.Instance?.OnResourceDeposited(ResourceType, PieceType, PolishedPercent, 1);
			Delete();
		}
	}
}
