using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000043 RID: 67
public class DetonatorExplosion : MonoBehaviour, ISaveLoadableWorldEvent
{
	// Token: 0x060001C5 RID: 453 RVA: 0x00009B11 File Offset: 0x00007D11
	private void Awake()
	{
		this.Initialize(this._detonatorExplosionState);
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x00009B20 File Offset: 0x00007D20
	public void Initialize(DetonatorExplosionState detonatorExplosionState)
	{
		this._detonatorExplosionState = detonatorExplosionState;
		if (this.HasExploded())
		{
			this.StuffToDisable.SetActive(false);
			if (this.TNTObject != null)
			{
				this.TNTObject.gameObject.SetActive(false);
			}
		}
		else if (this.TNTObject != null)
		{
			this.TNTObject.gameObject.SetActive(this.HasPurchased());
		}
		foreach (DetonatorTrigger detonatorTrigger in this.BoomBoxHandles)
		{
			detonatorTrigger.Initialize(this);
		}
		foreach (DetonatorBuySign detonatorBuySign in this.BuySigns)
		{
			detonatorBuySign.Initialize(this);
		}
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x00009C14 File Offset: 0x00007E14
	public bool HasPurchased()
	{
		if (this._detonatorExplosionState == DetonatorExplosionState.Available && this.BuySigns.Count == 0)
		{
			this._detonatorExplosionState = DetonatorExplosionState.Purchased;
			return true;
		}
		return this._detonatorExplosionState == DetonatorExplosionState.Purchased || this._detonatorExplosionState == DetonatorExplosionState.Exploded;
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x00009C48 File Offset: 0x00007E48
	public bool HasExploded()
	{
		return this._detonatorExplosionState == DetonatorExplosionState.Exploded;
	}

	// Token: 0x060001C9 RID: 457 RVA: 0x00009C54 File Offset: 0x00007E54
	[Obsolete]
	public static void TriggerExplosionID(int detonatorID, bool fromLoadingSaveFile = false)
	{
		foreach (DetonatorExplosion detonatorExplosion in Object.FindObjectsOfType<DetonatorExplosion>())
		{
			if (detonatorExplosion.DetonatorID == detonatorID)
			{
				detonatorExplosion.Explode(fromLoadingSaveFile, null);
			}
		}
	}

	// Token: 0x060001CA RID: 458 RVA: 0x00009C8C File Offset: 0x00007E8C
	public void PurchaseTNT()
	{
		if (this.TNTObject != null)
		{
			this.TNTObject.SetActive(true);
		}
		this._detonatorExplosionState = DetonatorExplosionState.Purchased;
		foreach (DetonatorBuySign detonatorBuySign in this.BuySigns)
		{
			detonatorBuySign.gameObject.SetActive(false);
		}
		foreach (DetonatorTrigger detonatorTrigger in this.BoomBoxHandles)
		{
			detonatorTrigger.ToggleHandle(true);
		}
	}

	// Token: 0x060001CB RID: 459 RVA: 0x00009D44 File Offset: 0x00007F44
	public void Explode(bool fromLoadingSaveFile = false, DetonatorTrigger activatingTrigger = null)
	{
		if (this.HasExploded())
		{
			return;
		}
		this._detonatorExplosionState = DetonatorExplosionState.Exploded;
		if (!fromLoadingSaveFile)
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._explosionSound, this.ParticleObject.transform.position, 1f, 1f, true, false);
			this.ParticleObject.SetActive(true);
		}
		this.StuffToDisable.SetActive(false);
		if (this.TNTObject != null)
		{
			this.TNTObject.SetActive(false);
		}
		if (!fromLoadingSaveFile)
		{
			base.StartCoroutine(this.ExplosionCoroutine(activatingTrigger));
		}
	}

	// Token: 0x060001CC RID: 460 RVA: 0x00009DD3 File Offset: 0x00007FD3
	public IEnumerator ExplosionCoroutine(DetonatorTrigger activatingTrigger)
	{
		yield return new WaitForFixedUpdate();
		int num = Random.Range(this._minObjectsToSpawn, this._maxObjectsToSpawn + 1);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = this._possiblePrefabsToSpawnDuringExplosion[Random.Range(0, this._possiblePrefabsToSpawnDuringExplosion.Count)];
			Transform transform = this._physicsExplosionPositions[Random.Range(0, this._physicsExplosionPositions.Count)];
			Vector3 vector = Random.insideUnitSphere * this._spawnOffsetRadius;
			Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(gameObject, transform.position + vector, transform.rotation, null);
		}
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		foreach (Transform transform2 in this._physicsExplosionPositions)
		{
			PhysicsUtils.SimpleExplosion(transform2.position, 8f, 8f, 0.5f);
		}
		using (List<DetonatorTrigger>.Enumerator enumerator2 = this.BoomBoxHandles.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				DetonatorTrigger detonatorTrigger = enumerator2.Current;
				if (detonatorTrigger != activatingTrigger)
				{
					detonatorTrigger.RemoveDetonatorTrigger();
				}
			}
			yield break;
		}
		yield break;
	}

	// Token: 0x060001CD RID: 461 RVA: 0x00009DE9 File Offset: 0x00007FE9
	public SavableWorldEventType GetWorldEventType()
	{
		return SavableWorldEventType.TNTDetonator;
	}

	// Token: 0x060001CE RID: 462 RVA: 0x00009DEC File Offset: 0x00007FEC
	public bool GetHasHappened()
	{
		return this._detonatorExplosionState > DetonatorExplosionState.Available;
	}

	// Token: 0x060001CF RID: 463 RVA: 0x00009DF7 File Offset: 0x00007FF7
	public int GetWorldEventID()
	{
		return this.DetonatorID;
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x00009E00 File Offset: 0x00008000
	public virtual void LoadFromSave(string json)
	{
		DetonatorExplosionSaveData detonatorExplosionSaveData = JsonUtility.FromJson<DetonatorExplosionSaveData>(json);
		if (detonatorExplosionSaveData == null)
		{
			detonatorExplosionSaveData = new DetonatorExplosionSaveData();
		}
		this.Initialize(detonatorExplosionSaveData.DetonatorExplosionState);
	}

	// Token: 0x060001D1 RID: 465 RVA: 0x00009E29 File Offset: 0x00008029
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new DetonatorExplosionSaveData
		{
			DetonatorExplosionState = this._detonatorExplosionState
		});
	}

	// Token: 0x040001BC RID: 444
	public int DetonatorID;

	// Token: 0x040001BD RID: 445
	public float CostToBuy;

	// Token: 0x040001BE RID: 446
	public GameObject ParticleObject;

	// Token: 0x040001BF RID: 447
	public GameObject StuffToDisable;

	// Token: 0x040001C0 RID: 448
	public GameObject TNTObject;

	// Token: 0x040001C1 RID: 449
	[SerializeField]
	private SoundDefinition _explosionSound;

	// Token: 0x040001C2 RID: 450
	public List<DetonatorTrigger> BoomBoxHandles;

	// Token: 0x040001C3 RID: 451
	public List<DetonatorBuySign> BuySigns;

	// Token: 0x040001C4 RID: 452
	[Tooltip("This is also where the prefabs spawns from")]
	[SerializeField]
	private List<Transform> _physicsExplosionPositions;

	// Token: 0x040001C5 RID: 453
	[Tooltip("Generally these should be physics ore pieces")]
	[SerializeField]
	private List<GameObject> _possiblePrefabsToSpawnDuringExplosion;

	// Token: 0x040001C6 RID: 454
	[SerializeField]
	private int _minObjectsToSpawn;

	// Token: 0x040001C7 RID: 455
	[SerializeField]
	private int _maxObjectsToSpawn;

	// Token: 0x040001C8 RID: 456
	[SerializeField]
	private float _spawnOffsetRadius = 0.5f;

	// Token: 0x040001C9 RID: 457
	private DetonatorExplosionState _detonatorExplosionState;
}
