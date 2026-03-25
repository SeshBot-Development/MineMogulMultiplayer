using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CastingFurnace : MonoBehaviour, ICustomSaveDataProvider
{
	public List<CastingFurnaceMoldArea> MoldAreas;

	public Transform LiquidPlane;

	public Animator ProcessingAnimator;

	public float ProcessingTime = 6f;

	public float CoalBurnedPerSecond = 0.5f;

	public float MaterialPerCoalConsumed = 3f;

	public BuildingObject BuildingObject;

	[SerializeField]
	private Transform PourSoundPosition;

	[SerializeField]
	private TMP_Text _outputProductText;

	[SerializeField]
	private TMP_Text _contentsListText;

	[SerializeField]
	private TMP_Text _contentsBarText;

	[SerializeField]
	private TMP_Text _requiredAmountBarText;

	[SerializeField]
	private TMP_Text _moreCoalRequiredText;

	[SerializeField]
	private SoundDefinition _pourSound;

	[SerializeField]
	private SoundDefinition _addSound;

	[SerializeField]
	private LoopingSoundFader _loopingSoundFader;

	[SerializeField]
	private List<CastingMoldRendererInfo> _castingMoldRendererInfos;

	[SerializeField]
	private List<CastingFurnaceMoldRecipieSet> _moldRecipieSets;

	private Queue<ResourceType> resourceQueue = new Queue<ResourceType>();

	private Queue<ResourceType> visualResourceQueue = new Queue<ResourceType>();

	private List<OrePiece> waitingList = new List<OrePiece>();

	private bool isProcessing;

	private const int MaxQueueCount = 200;

	private const int MaxVisualQueueCount = 29;

	private float initialLiquidPlaneY;

	private int _materialRequiredToSmelt;

	public float CoalAmount;

	public const float MaxCoalAmount = 100f;

	private void Start()
	{
		if (LiquidPlane != null)
		{
			initialLiquidPlaneY = LiquidPlane.localPosition.y;
		}
		if (ProcessingAnimator != null)
		{
			ProcessingAnimator.speed = 1f / ProcessingTime;
		}
	}

	private void OnEnable()
	{
		BuildingObject.OnBuildingRemoved += OnBuildingRemoved;
		RefreshContentsDisplay();
		_outputProductText.text = "";
		for (int i = 0; i < MoldAreas.Count; i++)
		{
			MoldAreas[i].Initialize(this, i, CastingMoldType.None);
		}
		RecalculateMaterialAmountRequired();
	}

	private void OnDisable()
	{
		BuildingObject.OnBuildingRemoved -= OnBuildingRemoved;
	}

	public float GetRequiredCoalForSteel()
	{
		return (float)_materialRequiredToSmelt / MaterialPerCoalConsumed;
	}

	private void OnBuildingRemoved()
	{
		foreach (CastingFurnaceMoldArea moldArea in MoldAreas)
		{
			moldArea.EjectMold();
		}
	}

	public void RecalculateMaterialAmountRequired()
	{
		_materialRequiredToSmelt = 0;
		foreach (CastingFurnaceMoldArea area in MoldAreas)
		{
			CastingFurnaceMoldRecipieSet castingFurnaceMoldRecipieSet = _moldRecipieSets.Find((CastingFurnaceMoldRecipieSet set) => set.CastingMoldType == area.CastingMoldType);
			if (castingFurnaceMoldRecipieSet != null)
			{
				_materialRequiredToSmelt += castingFurnaceMoldRecipieSet.AmountOfMaterialRequired;
			}
			else
			{
				_materialRequiredToSmelt += 6;
			}
		}
		if (_materialRequiredToSmelt < 29)
		{
			_requiredAmountBarText.text = "";
			for (int num = 0; num < _materialRequiredToSmelt - 1; num++)
			{
				_requiredAmountBarText.text += " ";
			}
			_requiredAmountBarText.text += "|";
		}
		else
		{
			_requiredAmountBarText.text = "";
		}
	}

	private void Update()
	{
		waitingList.RemoveAll((OrePiece o) => o == null || !o.gameObject.activeInHierarchy);
		if (resourceQueue.Count < 200 && waitingList.Count > 0)
		{
			ProcessWaitingOrePiece();
		}
		if (resourceQueue.Count >= _materialRequiredToSmelt && !isProcessing)
		{
			StartCoroutine(ProcessOre());
		}
		UpdateLiquidPlane();
		if (CoalAmount > 0f)
		{
			float num = CoalBurnedPerSecond * Time.deltaTime;
			CoalAmount -= num;
			if (CoalAmount < 0f)
			{
				CoalAmount = 0f;
			}
		}
	}

	private void UpdateLiquidPlane()
	{
		if (!(LiquidPlane == null))
		{
			float num = Mathf.Clamp((float)resourceQueue.Count / 200f, 0f, 1f);
			Vector3 localPosition = LiquidPlane.localPosition;
			localPosition.y = initialLiquidPlaneY + 1f * num;
			LiquidPlane.localPosition = localPosition;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (!(componentInParent != null))
		{
			return;
		}
		if (resourceQueue.Count < 200)
		{
			EnqueueOrePiece(componentInParent);
			if (_loopingSoundFader.GetCurrentTargetVolume() == 0f)
			{
				_loopingSoundFader.FadeTo(1f, 1f);
			}
		}
		else
		{
			waitingList.Add(componentInParent);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			waitingList.Remove(componentInParent);
		}
	}

	private void ProcessWaitingOrePiece()
	{
		if (waitingList.Count > 0)
		{
			OrePiece ore = waitingList[0];
			EnqueueOrePiece(ore);
			waitingList.RemoveAt(0);
		}
	}

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
		case PieceType.Gear:
			num = 5;
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
		case PieceType.JunkCast:
			num = 3;
			break;
		}
		bool flag = visualResourceQueue.Count < 29;
		for (int i = 0; i < num; i++)
		{
			resourceQueue.Enqueue(ore.ResourceType);
			visualResourceQueue.Enqueue(ore.ResourceType);
		}
		if (flag)
		{
			RefreshContentsDisplay();
			if (visualResourceQueue.Count <= _materialRequiredToSmelt)
			{
				UpdateProjectedOutputResource();
			}
		}
		ore.Delete();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_addSound, LiquidPlane.position);
	}

	private IEnumerator ProcessOre()
	{
		isProcessing = true;
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_pourSound, PourSoundPosition.position);
		ProcessingAnimator.Play("BlastFurnace_Process");
		List<ResourceType> list = new List<ResourceType>();
		for (int i = 0; i < _materialRequiredToSmelt; i++)
		{
			list.Add(resourceQueue.Dequeue());
		}
		ResourceType outputResourceType = DetermineOutputResourceType(list);
		_outputProductText.text = Singleton<OreManager>.Instance.GetColoredResourceTypeString(outputResourceType);
		_moreCoalRequiredText.text = "Processing...";
		if (outputResourceType == ResourceType.Steel)
		{
			CoalAmount -= GetRequiredCoalForSteel();
			if (CoalAmount < 0f)
			{
				CoalAmount = 0f;
			}
		}
		yield return new WaitForSeconds(ProcessingTime);
		CreateOutputOrePiece(outputResourceType);
		for (int j = 0; j < _materialRequiredToSmelt; j++)
		{
			if (visualResourceQueue.Count == 0)
			{
				break;
			}
			visualResourceQueue.Dequeue();
		}
		isProcessing = false;
		RefreshContentsDisplay();
		if (resourceQueue.Count == 0)
		{
			_loopingSoundFader.FadeTo(0f, 5f);
		}
	}

	private void UpdateProjectedOutputResource()
	{
		if (resourceQueue.Count == 0)
		{
			_outputProductText.text = "";
			return;
		}
		ResourceType resourceType = DetermineOutputResourceType(new List<ResourceType>(resourceQueue));
		ResourceType resourceType2 = ResourceType.Slag;
		_outputProductText.text = Singleton<OreManager>.Instance.GetColoredResourceTypeString(resourceType switch
		{
			ResourceType.Gold => ResourceType.Gold, 
			ResourceType.Iron => ResourceType.Iron, 
			ResourceType.Copper => ResourceType.Copper, 
			ResourceType.Steel => ResourceType.Steel, 
			_ => ResourceType.Slag, 
		});
	}

	private void RefreshContentsDisplay()
	{
		HashSet<ResourceType> hashSet = new HashSet<ResourceType>();
		foreach (ResourceType item in visualResourceQueue)
		{
			hashSet.Add(item);
		}
		_contentsListText.text = "Contents:\n* ";
		_contentsListText.text += string.Join("\n* ", hashSet.Select((ResourceType r) => Singleton<OreManager>.Instance.GetColoredResourceTypeString(r)));
		Queue<ResourceType> queue = new Queue<ResourceType>(visualResourceQueue);
		_contentsBarText.text = "";
		for (int num = 0; num < 29; num++)
		{
			if (queue.Count == 0)
			{
				break;
			}
			ResourceType resourceType = queue.Dequeue();
			TMP_Text contentsBarText = _contentsBarText;
			contentsBarText.text = contentsBarText.text + "<color=#" + Singleton<OreManager>.Instance.GetResourceColor(resourceType).ToHexString() + ">|</color>";
			if (num == _materialRequiredToSmelt - 1)
			{
				_contentsBarText.text += " ";
			}
		}
		if (isProcessing)
		{
			_moreCoalRequiredText.text = "Processing...";
		}
		else if (CoalAmount > GetRequiredCoalForSteel())
		{
			_moreCoalRequiredText.text = "Sufficient " + Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Coal) + " for " + Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Steel);
		}
		else
		{
			_moreCoalRequiredText.text = "More " + Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Coal) + " required for " + Singleton<OreManager>.Instance.GetColoredResourceTypeString(ResourceType.Steel);
		}
	}

	private ResourceType DetermineOutputResourceType(List<ResourceType> processingResources)
	{
		ResourceType mostCommonResource = (from r in processingResources
			group r by r into grp
			orderby grp.Count() descending
			select grp.Key).First();
		bool flag = processingResources.All((ResourceType r) => r == mostCommonResource);
		float requiredCoalForSteel = GetRequiredCoalForSteel();
		if (mostCommonResource == ResourceType.Iron && CoalAmount > requiredCoalForSteel)
		{
			mostCommonResource = ResourceType.Steel;
		}
		if (flag)
		{
			return mostCommonResource;
		}
		bool flag2 = processingResources.All((ResourceType r) => r == ResourceType.Iron || r == ResourceType.Steel);
		if (!flag && flag2 && CoalAmount > requiredCoalForSteel)
		{
			return ResourceType.Steel;
		}
		return ResourceType.Slag;
	}

	private void CreateOutputOrePiece(ResourceType resourceType)
	{
		foreach (CastingFurnaceMoldArea area in MoldAreas)
		{
			CastingFurnaceRecipie castingFurnaceRecipie = _moldRecipieSets.Find((CastingFurnaceMoldRecipieSet set) => set.CastingMoldType == area.CastingMoldType)?.Recipies.Find((CastingFurnaceRecipie rec) => rec.InputResourceType == resourceType);
			if (castingFurnaceRecipie != null)
			{
				if (castingFurnaceRecipie.OutputPrefab != null)
				{
					Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(castingFurnaceRecipie.OutputPrefab, area.ProductEjectTransform.position, area.ProductEjectTransform.rotation);
				}
				if (castingFurnaceRecipie.SecondaryOutputPrefab != null && area.SecondaryProductEjectTransform != null)
				{
					Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(castingFurnaceRecipie.SecondaryOutputPrefab, area.SecondaryProductEjectTransform.position, area.SecondaryProductEjectTransform.rotation);
				}
			}
			else
			{
				Debug.Log("CastingFurnace: No recipie found for mold: " + area.CastingMoldType.ToString() + ", resource: " + resourceType);
			}
		}
	}

	public void AddCoal(int amount)
	{
		CoalAmount += amount;
		CoalAmount = Mathf.Clamp(CoalAmount, 0f, 100f);
	}

	public CastingMoldRendererInfo GetCastingMoldRendererInfo(CastingMoldType type)
	{
		if (type == CastingMoldType.None)
		{
			return null;
		}
		return _castingMoldRendererInfos.Find((CastingMoldRendererInfo info) => info.CastingMoldType == type);
	}

	public virtual void LoadFromSave(string json)
	{
		CastingFurnaceSaveData castingFurnaceSaveData = JsonUtility.FromJson<CastingFurnaceSaveData>(json);
		if (castingFurnaceSaveData == null)
		{
			castingFurnaceSaveData = new CastingFurnaceSaveData();
		}
		CoalAmount = castingFurnaceSaveData.CoalAmount;
		MoldAreas[0].InsertMoldFromLoading(castingFurnaceSaveData.Mold1Type);
		MoldAreas[1].InsertMoldFromLoading(castingFurnaceSaveData.Mold2Type);
		MoldAreas[2].InsertMoldFromLoading(castingFurnaceSaveData.Mold3Type);
		RecalculateMaterialAmountRequired();
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new CastingFurnaceSaveData
		{
			CoalAmount = CoalAmount,
			Mold1Type = MoldAreas[0].CastingMoldType,
			Mold2Type = MoldAreas[1].CastingMoldType,
			Mold3Type = MoldAreas[2].CastingMoldType
		});
	}
}
