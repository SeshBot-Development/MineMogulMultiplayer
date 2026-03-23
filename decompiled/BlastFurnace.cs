using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BlastFurnace : MonoBehaviour
{
	public Transform OutputTransform;

	public Transform LiquidPlane;

	public Animator ProcessingAnimator;

	public float ProcessingTime = 3f;

	public GameObject GoldIngotPrefab;

	public GameObject IronIngotPrefab;

	public GameObject CopperIngotPrefab;

	public GameObject SlagPrefab;

	[SerializeField]
	private TMP_Text _outputProductText;

	[SerializeField]
	private TMP_Text _contentsListText;

	[SerializeField]
	private TMP_Text _contentsBarText;

	[SerializeField]
	private SoundDefinition _pourSound;

	[SerializeField]
	private SoundDefinition _addSound;

	[SerializeField]
	private LoopingSoundFader _loopingSoundFader;

	private Queue<ResourceType> resourceQueue = new Queue<ResourceType>();

	private Queue<ResourceType> visualResourceQueue = new Queue<ResourceType>();

	private List<OrePiece> waitingList = new List<OrePiece>();

	private bool isProcessing;

	private const int MaxQueueCount = 30;

	private const int MaxVisualQueueCount = 17;

	private const int AmountRequiredToProcess = 6;

	private float initialLiquidPlaneY;

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
		RefreshContentsDisplay();
		_outputProductText.text = "";
	}

	private void Update()
	{
		waitingList.RemoveAll((OrePiece o) => o == null || !o.gameObject.activeInHierarchy);
		if (resourceQueue.Count < 30 && waitingList.Count > 0)
		{
			ProcessWaitingOrePiece();
		}
		if (resourceQueue.Count >= 6 && !isProcessing)
		{
			StartCoroutine(ProcessOre());
		}
		UpdateLiquidPlane();
	}

	private void UpdateLiquidPlane()
	{
		if (!(LiquidPlane == null))
		{
			float num = Mathf.Clamp((float)resourceQueue.Count / 30f, 0f, 1f);
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
		if (resourceQueue.Count < 30)
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
		bool flag = visualResourceQueue.Count < 17;
		for (int i = 0; i < num; i++)
		{
			resourceQueue.Enqueue(ore.ResourceType);
			visualResourceQueue.Enqueue(ore.ResourceType);
		}
		if (flag)
		{
			RefreshContentsDisplay();
			if (visualResourceQueue.Count <= 6)
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
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_pourSound, OutputTransform.position);
		ProcessingAnimator.Play("BlastFurnace_Process");
		List<ResourceType> list = new List<ResourceType>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(resourceQueue.Dequeue());
		}
		ResourceType outputResourceType = DetermineOutputResourceType(list);
		_outputProductText.text = Singleton<OreManager>.Instance.GetColoredResourceTypeString(outputResourceType);
		yield return new WaitForSeconds(ProcessingTime);
		CreateOutputOrePiece(outputResourceType);
		for (int j = 0; j < 6; j++)
		{
			visualResourceQueue.Dequeue();
		}
		RefreshContentsDisplay();
		isProcessing = false;
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
		for (int num = 0; num < 17; num++)
		{
			if (queue.Count == 0)
			{
				break;
			}
			ResourceType resourceType = queue.Dequeue();
			TMP_Text contentsBarText = _contentsBarText;
			contentsBarText.text = contentsBarText.text + "<color=#" + Singleton<OreManager>.Instance.GetResourceColor(resourceType).ToHexString() + ">|</color>";
			if (num == 5)
			{
				_contentsBarText.text += " ";
			}
		}
	}

	private ResourceType DetermineOutputResourceType(List<ResourceType> processingResources)
	{
		ResourceType mostCommonResource = (from r in processingResources
			group r by r into grp
			orderby grp.Count() descending
			select grp.Key).First();
		if (!processingResources.All((ResourceType r) => r == mostCommonResource))
		{
			return ResourceType.Slag;
		}
		return mostCommonResource;
	}

	private void CreateOutputOrePiece(ResourceType resourceType)
	{
		GameObject gameObject = null;
		gameObject = resourceType switch
		{
			ResourceType.Gold => GoldIngotPrefab, 
			ResourceType.Iron => IronIngotPrefab, 
			ResourceType.Copper => CopperIngotPrefab, 
			_ => SlagPrefab, 
		};
		if (gameObject != null)
		{
			Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(gameObject, OutputTransform.position, OutputTransform.rotation);
		}
	}
}
