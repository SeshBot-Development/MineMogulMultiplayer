using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Token: 0x0200001D RID: 29
public class CastingFurnace : MonoBehaviour, ICustomSaveDataProvider
{
	// Token: 0x060000F5 RID: 245 RVA: 0x0000614C File Offset: 0x0000434C
	private void Start()
	{
		if (this.LiquidPlane != null)
		{
			this.initialLiquidPlaneY = this.LiquidPlane.localPosition.y;
		}
		if (this.ProcessingAnimator != null)
		{
			this.ProcessingAnimator.speed = 1f / this.ProcessingTime;
		}
	}

	// Token: 0x060000F6 RID: 246 RVA: 0x000061A4 File Offset: 0x000043A4
	private void OnEnable()
	{
		this.BuildingObject.OnBuildingRemoved += this.OnBuildingRemoved;
		this.RefreshContentsDisplay();
		this._outputProductText.text = "";
		for (int i = 0; i < this.MoldAreas.Count; i++)
		{
			this.MoldAreas[i].Initialize(this, i, CastingMoldType.None);
		}
		this.RecalculateMaterialAmountRequired();
	}

	// Token: 0x060000F7 RID: 247 RVA: 0x0000620E File Offset: 0x0000440E
	private void OnDisable()
	{
		this.BuildingObject.OnBuildingRemoved -= this.OnBuildingRemoved;
	}

	// Token: 0x060000F8 RID: 248 RVA: 0x00006227 File Offset: 0x00004427
	public float GetRequiredCoalForSteel()
	{
		return (float)this._materialRequiredToSmelt / this.MaterialPerCoalConsumed;
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x00006238 File Offset: 0x00004438
	private void OnBuildingRemoved()
	{
		foreach (CastingFurnaceMoldArea castingFurnaceMoldArea in this.MoldAreas)
		{
			castingFurnaceMoldArea.EjectMold();
		}
	}

	// Token: 0x060000FA RID: 250 RVA: 0x00006288 File Offset: 0x00004488
	public void RecalculateMaterialAmountRequired()
	{
		this._materialRequiredToSmelt = 0;
		using (List<CastingFurnaceMoldArea>.Enumerator enumerator = this.MoldAreas.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CastingFurnaceMoldArea area = enumerator.Current;
				CastingFurnaceMoldRecipieSet castingFurnaceMoldRecipieSet = this._moldRecipieSets.Find((CastingFurnaceMoldRecipieSet set) => set.CastingMoldType == area.CastingMoldType);
				if (castingFurnaceMoldRecipieSet != null)
				{
					this._materialRequiredToSmelt += castingFurnaceMoldRecipieSet.AmountOfMaterialRequired;
				}
				else
				{
					this._materialRequiredToSmelt += 6;
				}
			}
		}
		if (this._materialRequiredToSmelt < 29)
		{
			this._requiredAmountBarText.text = "";
			for (int i = 0; i < this._materialRequiredToSmelt - 1; i++)
			{
				TMP_Text requiredAmountBarText = this._requiredAmountBarText;
				requiredAmountBarText.text += " ";
			}
			TMP_Text requiredAmountBarText2 = this._requiredAmountBarText;
			requiredAmountBarText2.text += "|";
			return;
		}
		this._requiredAmountBarText.text = "";
	}

	// Token: 0x060000FB RID: 251 RVA: 0x00006398 File Offset: 0x00004598
	private void Update()
	{
		this.waitingList.RemoveAll((OrePiece o) => o == null || !o.gameObject.activeInHierarchy);
		if (this.resourceQueue.Count < 200 && this.waitingList.Count > 0)
		{
			this.ProcessWaitingOrePiece();
		}
		if (this.resourceQueue.Count >= this._materialRequiredToSmelt && !this.isProcessing)
		{
			base.StartCoroutine(this.ProcessOre());
		}
		this.UpdateLiquidPlane();
		if (this.CoalAmount > 0f)
		{
			float num = this.CoalBurnedPerSecond * Time.deltaTime;
			this.CoalAmount -= num;
			if (this.CoalAmount < 0f)
			{
				this.CoalAmount = 0f;
			}
		}
	}

	// Token: 0x060000FC RID: 252 RVA: 0x00006464 File Offset: 0x00004664
	private void UpdateLiquidPlane()
	{
		if (this.LiquidPlane == null)
		{
			return;
		}
		float num = Mathf.Clamp((float)this.resourceQueue.Count / 200f, 0f, 1f);
		Vector3 localPosition = this.LiquidPlane.localPosition;
		localPosition.y = this.initialLiquidPlaneY + 1f * num;
		this.LiquidPlane.localPosition = localPosition;
	}

	// Token: 0x060000FD RID: 253 RVA: 0x000064D0 File Offset: 0x000046D0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			if (this.resourceQueue.Count < 200)
			{
				this.EnqueueOrePiece(componentInParent);
				if (this._loopingSoundFader.GetCurrentTargetVolume() == 0f)
				{
					this._loopingSoundFader.FadeTo(1f, 1f);
					return;
				}
			}
			else
			{
				this.waitingList.Add(componentInParent);
			}
		}
	}

	// Token: 0x060000FE RID: 254 RVA: 0x0000653C File Offset: 0x0000473C
	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this.waitingList.Remove(componentInParent);
		}
	}

	// Token: 0x060000FF RID: 255 RVA: 0x00006568 File Offset: 0x00004768
	private void ProcessWaitingOrePiece()
	{
		if (this.waitingList.Count > 0)
		{
			OrePiece orePiece = this.waitingList[0];
			this.EnqueueOrePiece(orePiece);
			this.waitingList.RemoveAt(0);
		}
	}

	// Token: 0x06000100 RID: 256 RVA: 0x000065A4 File Offset: 0x000047A4
	private void EnqueueOrePiece(OrePiece ore)
	{
		if (ore == null)
		{
			return;
		}
		int num = 1;
		switch (ore.PieceType)
		{
		case PieceType.Ingot:
			num = 6;
			break;
		case PieceType.Plate:
			num = 5;
			break;
		case PieceType.Pipe:
			num = 5;
			break;
		case PieceType.Rod:
			num = 5;
			break;
		case PieceType.ThreadedRod:
			num = 5;
			break;
		case PieceType.Gear:
			num = 5;
			break;
		case PieceType.JunkCast:
			num = 3;
			break;
		}
		bool flag = this.visualResourceQueue.Count < 29;
		for (int i = 0; i < num; i++)
		{
			this.resourceQueue.Enqueue(ore.ResourceType);
			this.visualResourceQueue.Enqueue(ore.ResourceType);
		}
		if (flag)
		{
			this.RefreshContentsDisplay();
			if (this.visualResourceQueue.Count <= this._materialRequiredToSmelt)
			{
				this.UpdateProjectedOutputResource();
			}
		}
		ore.Delete();
		global::Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._addSound, this.LiquidPlane.position, 1f, 1f, true, false);
	}

	// Token: 0x06000101 RID: 257 RVA: 0x0000669D File Offset: 0x0000489D
	private IEnumerator ProcessOre()
	{
		this.isProcessing = true;
		global::Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._pourSound, this.PourSoundPosition.position, 1f, 1f, true, false);
		this.ProcessingAnimator.Play("BlastFurnace_Process");
		List<ResourceType> list = new List<ResourceType>();
		for (int i = 0; i < this._materialRequiredToSmelt; i++)
		{
			list.Add(this.resourceQueue.Dequeue());
		}
		ResourceType outputResourceType = this.DetermineOutputResourceType(list);
		this._outputProductText.text = global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(outputResourceType);
		this._moreCoalRequiredText.text = "Processing...";
		if (outputResourceType == ResourceType.Steel)
		{
			this.CoalAmount -= this.GetRequiredCoalForSteel();
			if (this.CoalAmount < 0f)
			{
				this.CoalAmount = 0f;
			}
		}
		yield return new WaitForSeconds(this.ProcessingTime);
		this.CreateOutputOrePiece(outputResourceType);
		int num = 0;
		while (num < this._materialRequiredToSmelt && this.visualResourceQueue.Count != 0)
		{
			this.visualResourceQueue.Dequeue();
			num++;
		}
		this.isProcessing = false;
		this.RefreshContentsDisplay();
		if (this.resourceQueue.Count == 0)
		{
			this._loopingSoundFader.FadeTo(0f, 5f);
		}
		yield break;
	}

	// Token: 0x06000102 RID: 258 RVA: 0x000066AC File Offset: 0x000048AC
	private void UpdateProjectedOutputResource()
	{
		if (this.resourceQueue.Count == 0)
		{
			this._outputProductText.text = "";
			return;
		}
		ResourceType resourceType = this.DetermineOutputResourceType(new List<ResourceType>(this.resourceQueue));
		ResourceType resourceType2;
		if (resourceType <= ResourceType.Gold)
		{
			if (resourceType == ResourceType.Iron)
			{
				resourceType2 = ResourceType.Iron;
				goto IL_005E;
			}
			if (resourceType == ResourceType.Gold)
			{
				resourceType2 = ResourceType.Gold;
				goto IL_005E;
			}
		}
		else
		{
			if (resourceType == ResourceType.Copper)
			{
				resourceType2 = ResourceType.Copper;
				goto IL_005E;
			}
			if (resourceType == ResourceType.Steel)
			{
				resourceType2 = ResourceType.Steel;
				goto IL_005E;
			}
		}
		resourceType2 = ResourceType.Slag;
		IL_005E:
		this._outputProductText.text = global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(resourceType2);
	}

	// Token: 0x06000103 RID: 259 RVA: 0x00006730 File Offset: 0x00004930
	private void RefreshContentsDisplay()
	{
		HashSet<ResourceType> hashSet = new HashSet<ResourceType>();
		foreach (ResourceType resourceType in this.visualResourceQueue)
		{
			hashSet.Add(resourceType);
		}
		this._contentsListText.text = "Contents:\n* ";
		TMP_Text contentsListText = this._contentsListText;
		contentsListText.text += string.Join("\n* ", hashSet.Select((ResourceType r) => global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(r)));
		Queue<ResourceType> queue = new Queue<ResourceType>(this.visualResourceQueue);
		this._contentsBarText.text = "";
		int num = 0;
		while (num < 29 && queue.Count != 0)
		{
			ResourceType resourceType2 = queue.Dequeue();
			TMP_Text contentsBarText = this._contentsBarText;
			contentsBarText.text = contentsBarText.text + "<color=#" + global::Singleton<OreManager>.Instance.GetResourceColor(resourceType2).ToHexString() + ">|</color>";
			if (num == this._materialRequiredToSmelt - 1)
			{
				TMP_Text contentsBarText2 = this._contentsBarText;
				contentsBarText2.text += " ";
			}
			num++;
		}
		if (this.isProcessing)
		{
			this._moreCoalRequiredText.text = "Processing...";
			return;
		}
		if (this.CoalAmount > this.GetRequiredCoalForSteel())
		{
			this._moreCoalRequiredText.text = "Sufficient " + global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Coal) + " for " + global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Steel);
			return;
		}
		this._moreCoalRequiredText.text = "More " + global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Coal) + " required for " + global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Steel);
	}

	// Token: 0x06000104 RID: 260 RVA: 0x00006900 File Offset: 0x00004B00
	private ResourceType DetermineOutputResourceType(List<ResourceType> processingResources)
	{
		ResourceType mostCommonResource = (from r in processingResources
			group r by r into grp
			orderby grp.Count<ResourceType>() descending
			select grp.Key).First<ResourceType>();
		bool flag = processingResources.All((ResourceType r) => r == mostCommonResource);
		float requiredCoalForSteel = this.GetRequiredCoalForSteel();
		if (mostCommonResource == ResourceType.Iron && this.CoalAmount > requiredCoalForSteel)
		{
			mostCommonResource = ResourceType.Steel;
		}
		if (flag)
		{
			return mostCommonResource;
		}
		bool flag2 = processingResources.All((ResourceType r) => r == ResourceType.Iron || r == ResourceType.Steel);
		if (!flag && flag2 && this.CoalAmount > requiredCoalForSteel)
		{
			return ResourceType.Steel;
		}
		return ResourceType.Slag;
	}

	// Token: 0x06000105 RID: 261 RVA: 0x00006A04 File Offset: 0x00004C04
	private void CreateOutputOrePiece(ResourceType resourceType)
	{
		using (List<CastingFurnaceMoldArea>.Enumerator enumerator = this.MoldAreas.GetEnumerator())
		{
			Predicate<CastingFurnaceRecipie> <>9__1;
			while (enumerator.MoveNext())
			{
				CastingFurnaceMoldArea area = enumerator.Current;
				CastingFurnaceMoldRecipieSet castingFurnaceMoldRecipieSet = this._moldRecipieSets.Find((CastingFurnaceMoldRecipieSet set) => set.CastingMoldType == area.CastingMoldType);
				CastingFurnaceRecipie castingFurnaceRecipie;
				if (castingFurnaceMoldRecipieSet == null)
				{
					castingFurnaceRecipie = null;
				}
				else
				{
					List<CastingFurnaceRecipie> recipies = castingFurnaceMoldRecipieSet.Recipies;
					Predicate<CastingFurnaceRecipie> predicate;
					if ((predicate = <>9__1) == null)
					{
						predicate = (<>9__1 = (CastingFurnaceRecipie rec) => rec.InputResourceType == resourceType);
					}
					castingFurnaceRecipie = recipies.Find(predicate);
				}
				CastingFurnaceRecipie castingFurnaceRecipie2 = castingFurnaceRecipie;
				if (castingFurnaceRecipie2 != null)
				{
					if (castingFurnaceRecipie2.OutputPrefab != null)
					{
						global::Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(castingFurnaceRecipie2.OutputPrefab, area.ProductEjectTransform.position, area.ProductEjectTransform.rotation, null);
					}
					if (castingFurnaceRecipie2.SecondaryOutputPrefab != null && area.SecondaryProductEjectTransform != null)
					{
						global::Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(castingFurnaceRecipie2.SecondaryOutputPrefab, area.SecondaryProductEjectTransform.position, area.SecondaryProductEjectTransform.rotation, null);
					}
				}
				else
				{
					Debug.Log("CastingFurnace: No recipie found for mold: " + area.CastingMoldType.ToString() + ", resource: " + resourceType.ToString());
				}
			}
		}
	}

	// Token: 0x06000106 RID: 262 RVA: 0x00006BA0 File Offset: 0x00004DA0
	public void AddCoal(int amount)
	{
		this.CoalAmount += (float)amount;
		this.CoalAmount = Mathf.Clamp(this.CoalAmount, 0f, 100f);
	}

	// Token: 0x06000107 RID: 263 RVA: 0x00006BCC File Offset: 0x00004DCC
	public CastingMoldRendererInfo GetCastingMoldRendererInfo(CastingMoldType type)
	{
		if (type == CastingMoldType.None)
		{
			return null;
		}
		return this._castingMoldRendererInfos.Find((CastingMoldRendererInfo info) => info.CastingMoldType == type);
	}

	// Token: 0x06000108 RID: 264 RVA: 0x00006C08 File Offset: 0x00004E08
	public virtual void LoadFromSave(string json)
	{
		CastingFurnaceSaveData castingFurnaceSaveData = JsonUtility.FromJson<CastingFurnaceSaveData>(json);
		if (castingFurnaceSaveData == null)
		{
			castingFurnaceSaveData = new CastingFurnaceSaveData();
		}
		this.CoalAmount = castingFurnaceSaveData.CoalAmount;
		this.MoldAreas[0].InsertMoldFromLoading(castingFurnaceSaveData.Mold1Type);
		this.MoldAreas[1].InsertMoldFromLoading(castingFurnaceSaveData.Mold2Type);
		this.MoldAreas[2].InsertMoldFromLoading(castingFurnaceSaveData.Mold3Type);
		this.RecalculateMaterialAmountRequired();
	}

	// Token: 0x06000109 RID: 265 RVA: 0x00006C7C File Offset: 0x00004E7C
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new CastingFurnaceSaveData
		{
			CoalAmount = this.CoalAmount,
			Mold1Type = this.MoldAreas[0].CastingMoldType,
			Mold2Type = this.MoldAreas[1].CastingMoldType,
			Mold3Type = this.MoldAreas[2].CastingMoldType
		});
	}

	// Token: 0x040000E1 RID: 225
	public List<CastingFurnaceMoldArea> MoldAreas;

	// Token: 0x040000E2 RID: 226
	public Transform LiquidPlane;

	// Token: 0x040000E3 RID: 227
	public Animator ProcessingAnimator;

	// Token: 0x040000E4 RID: 228
	public float ProcessingTime = 6f;

	// Token: 0x040000E5 RID: 229
	public float CoalBurnedPerSecond = 0.5f;

	// Token: 0x040000E6 RID: 230
	public float MaterialPerCoalConsumed = 3f;

	// Token: 0x040000E7 RID: 231
	public BuildingObject BuildingObject;

	// Token: 0x040000E8 RID: 232
	[SerializeField]
	private Transform PourSoundPosition;

	// Token: 0x040000E9 RID: 233
	[SerializeField]
	private TMP_Text _outputProductText;

	// Token: 0x040000EA RID: 234
	[SerializeField]
	private TMP_Text _contentsListText;

	// Token: 0x040000EB RID: 235
	[SerializeField]
	private TMP_Text _contentsBarText;

	// Token: 0x040000EC RID: 236
	[SerializeField]
	private TMP_Text _requiredAmountBarText;

	// Token: 0x040000ED RID: 237
	[SerializeField]
	private TMP_Text _moreCoalRequiredText;

	// Token: 0x040000EE RID: 238
	[SerializeField]
	private SoundDefinition _pourSound;

	// Token: 0x040000EF RID: 239
	[SerializeField]
	private SoundDefinition _addSound;

	// Token: 0x040000F0 RID: 240
	[SerializeField]
	private LoopingSoundFader _loopingSoundFader;

	// Token: 0x040000F1 RID: 241
	[SerializeField]
	private List<CastingMoldRendererInfo> _castingMoldRendererInfos;

	// Token: 0x040000F2 RID: 242
	[SerializeField]
	private List<CastingFurnaceMoldRecipieSet> _moldRecipieSets;

	// Token: 0x040000F3 RID: 243
	private Queue<ResourceType> resourceQueue = new Queue<ResourceType>();

	// Token: 0x040000F4 RID: 244
	private Queue<ResourceType> visualResourceQueue = new Queue<ResourceType>();

	// Token: 0x040000F5 RID: 245
	private List<OrePiece> waitingList = new List<OrePiece>();

	// Token: 0x040000F6 RID: 246
	private bool isProcessing;

	// Token: 0x040000F7 RID: 247
	private const int MaxQueueCount = 200;

	// Token: 0x040000F8 RID: 248
	private const int MaxVisualQueueCount = 29;

	// Token: 0x040000F9 RID: 249
	private float initialLiquidPlaneY;

	// Token: 0x040000FA RID: 250
	private int _materialRequiredToSmelt;

	// Token: 0x040000FB RID: 251
	public float CoalAmount;

	// Token: 0x040000FC RID: 252
	public const float MaxCoalAmount = 100f;
}
