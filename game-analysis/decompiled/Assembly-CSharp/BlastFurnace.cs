using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Token: 0x0200000E RID: 14
public class BlastFurnace : MonoBehaviour
{
	// Token: 0x06000065 RID: 101 RVA: 0x00003390 File Offset: 0x00001590
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

	// Token: 0x06000066 RID: 102 RVA: 0x000033E6 File Offset: 0x000015E6
	private void OnEnable()
	{
		this.RefreshContentsDisplay();
		this._outputProductText.text = "";
	}

	// Token: 0x06000067 RID: 103 RVA: 0x00003400 File Offset: 0x00001600
	private void Update()
	{
		this.waitingList.RemoveAll((OrePiece o) => o == null || !o.gameObject.activeInHierarchy);
		if (this.resourceQueue.Count < 30 && this.waitingList.Count > 0)
		{
			this.ProcessWaitingOrePiece();
		}
		if (this.resourceQueue.Count >= 6 && !this.isProcessing)
		{
			base.StartCoroutine(this.ProcessOre());
		}
		this.UpdateLiquidPlane();
	}

	// Token: 0x06000068 RID: 104 RVA: 0x00003484 File Offset: 0x00001684
	private void UpdateLiquidPlane()
	{
		if (this.LiquidPlane == null)
		{
			return;
		}
		float num = Mathf.Clamp((float)this.resourceQueue.Count / 30f, 0f, 1f);
		Vector3 localPosition = this.LiquidPlane.localPosition;
		localPosition.y = this.initialLiquidPlaneY + 1f * num;
		this.LiquidPlane.localPosition = localPosition;
	}

	// Token: 0x06000069 RID: 105 RVA: 0x000034F0 File Offset: 0x000016F0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			if (this.resourceQueue.Count < 30)
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

	// Token: 0x0600006A RID: 106 RVA: 0x00003558 File Offset: 0x00001758
	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this.waitingList.Remove(componentInParent);
		}
	}

	// Token: 0x0600006B RID: 107 RVA: 0x00003584 File Offset: 0x00001784
	private void ProcessWaitingOrePiece()
	{
		if (this.waitingList.Count > 0)
		{
			OrePiece orePiece = this.waitingList[0];
			this.EnqueueOrePiece(orePiece);
			this.waitingList.RemoveAt(0);
		}
	}

	// Token: 0x0600006C RID: 108 RVA: 0x000035C0 File Offset: 0x000017C0
	private void EnqueueOrePiece(OrePiece ore)
	{
		if (ore == null)
		{
			return;
		}
		int num = 1;
		bool flag = this.visualResourceQueue.Count < 17;
		for (int i = 0; i < num; i++)
		{
			this.resourceQueue.Enqueue(ore.ResourceType);
			this.visualResourceQueue.Enqueue(ore.ResourceType);
		}
		if (flag)
		{
			this.RefreshContentsDisplay();
			if (this.visualResourceQueue.Count <= 6)
			{
				this.UpdateProjectedOutputResource();
			}
		}
		ore.Delete();
		global::Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._addSound, this.LiquidPlane.position, 1f, 1f, true, false);
	}

	// Token: 0x0600006D RID: 109 RVA: 0x00003661 File Offset: 0x00001861
	private IEnumerator ProcessOre()
	{
		this.isProcessing = true;
		global::Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._pourSound, this.OutputTransform.position, 1f, 1f, true, false);
		this.ProcessingAnimator.Play("BlastFurnace_Process");
		List<ResourceType> list = new List<ResourceType>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(this.resourceQueue.Dequeue());
		}
		ResourceType outputResourceType = this.DetermineOutputResourceType(list);
		this._outputProductText.text = global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(outputResourceType);
		yield return new WaitForSeconds(this.ProcessingTime);
		this.CreateOutputOrePiece(outputResourceType);
		for (int j = 0; j < 6; j++)
		{
			this.visualResourceQueue.Dequeue();
		}
		this.RefreshContentsDisplay();
		this.isProcessing = false;
		if (this.resourceQueue.Count == 0)
		{
			this._loopingSoundFader.FadeTo(0f, 5f);
		}
		yield break;
	}

	// Token: 0x0600006E RID: 110 RVA: 0x00003670 File Offset: 0x00001870
	private void UpdateProjectedOutputResource()
	{
		if (this.resourceQueue.Count == 0)
		{
			this._outputProductText.text = "";
			return;
		}
		ResourceType resourceType = this.DetermineOutputResourceType(new List<ResourceType>(this.resourceQueue));
		ResourceType resourceType2;
		if (resourceType != ResourceType.Iron)
		{
			if (resourceType != ResourceType.Gold)
			{
				if (resourceType != ResourceType.Copper)
				{
					resourceType2 = ResourceType.Slag;
				}
				else
				{
					resourceType2 = ResourceType.Copper;
				}
			}
			else
			{
				resourceType2 = ResourceType.Gold;
			}
		}
		else
		{
			resourceType2 = ResourceType.Iron;
		}
		this._outputProductText.text = global::Singleton<OreManager>.Instance.GetColoredResourceTypeString(resourceType2);
	}

	// Token: 0x0600006F RID: 111 RVA: 0x000036E4 File Offset: 0x000018E4
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
		while (num < 17 && queue.Count != 0)
		{
			ResourceType resourceType2 = queue.Dequeue();
			TMP_Text contentsBarText = this._contentsBarText;
			contentsBarText.text = contentsBarText.text + "<color=#" + global::Singleton<OreManager>.Instance.GetResourceColor(resourceType2).ToHexString() + ">|</color>";
			if (num == 5)
			{
				TMP_Text contentsBarText2 = this._contentsBarText;
				contentsBarText2.text += " ";
			}
			num++;
		}
	}

	// Token: 0x06000070 RID: 112 RVA: 0x00003824 File Offset: 0x00001A24
	private ResourceType DetermineOutputResourceType(List<ResourceType> processingResources)
	{
		ResourceType mostCommonResource = (from r in processingResources
			group r by r into grp
			orderby grp.Count<ResourceType>() descending
			select grp.Key).First<ResourceType>();
		if (!processingResources.All((ResourceType r) => r == mostCommonResource))
		{
			return ResourceType.Slag;
		}
		return mostCommonResource;
	}

	// Token: 0x06000071 RID: 113 RVA: 0x000038CC File Offset: 0x00001ACC
	private void CreateOutputOrePiece(ResourceType resourceType)
	{
		GameObject gameObject;
		switch (resourceType)
		{
		case ResourceType.Iron:
			gameObject = this.IronIngotPrefab;
			goto IL_004A;
		case ResourceType.Gold:
			gameObject = this.GoldIngotPrefab;
			goto IL_004A;
		case ResourceType.Copper:
			gameObject = this.CopperIngotPrefab;
			goto IL_004A;
		}
		gameObject = this.SlagPrefab;
		IL_004A:
		if (gameObject != null)
		{
			global::Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(gameObject, this.OutputTransform.position, this.OutputTransform.rotation, null);
		}
	}

	// Token: 0x04000056 RID: 86
	public Transform OutputTransform;

	// Token: 0x04000057 RID: 87
	public Transform LiquidPlane;

	// Token: 0x04000058 RID: 88
	public Animator ProcessingAnimator;

	// Token: 0x04000059 RID: 89
	public float ProcessingTime = 3f;

	// Token: 0x0400005A RID: 90
	public GameObject GoldIngotPrefab;

	// Token: 0x0400005B RID: 91
	public GameObject IronIngotPrefab;

	// Token: 0x0400005C RID: 92
	public GameObject CopperIngotPrefab;

	// Token: 0x0400005D RID: 93
	public GameObject SlagPrefab;

	// Token: 0x0400005E RID: 94
	[SerializeField]
	private TMP_Text _outputProductText;

	// Token: 0x0400005F RID: 95
	[SerializeField]
	private TMP_Text _contentsListText;

	// Token: 0x04000060 RID: 96
	[SerializeField]
	private TMP_Text _contentsBarText;

	// Token: 0x04000061 RID: 97
	[SerializeField]
	private SoundDefinition _pourSound;

	// Token: 0x04000062 RID: 98
	[SerializeField]
	private SoundDefinition _addSound;

	// Token: 0x04000063 RID: 99
	[SerializeField]
	private LoopingSoundFader _loopingSoundFader;

	// Token: 0x04000064 RID: 100
	private Queue<ResourceType> resourceQueue = new Queue<ResourceType>();

	// Token: 0x04000065 RID: 101
	private Queue<ResourceType> visualResourceQueue = new Queue<ResourceType>();

	// Token: 0x04000066 RID: 102
	private List<OrePiece> waitingList = new List<OrePiece>();

	// Token: 0x04000067 RID: 103
	private bool isProcessing;

	// Token: 0x04000068 RID: 104
	private const int MaxQueueCount = 30;

	// Token: 0x04000069 RID: 105
	private const int MaxVisualQueueCount = 17;

	// Token: 0x0400006A RID: 106
	private const int AmountRequiredToProcess = 6;

	// Token: 0x0400006B RID: 107
	private float initialLiquidPlaneY;
}
