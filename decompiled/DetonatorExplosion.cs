using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonatorExplosion : MonoBehaviour, ISaveLoadableWorldEvent
{
	public int DetonatorID;

	public float CostToBuy;

	public GameObject ParticleObject;

	public GameObject StuffToDisable;

	public GameObject TNTObject;

	[SerializeField]
	private SoundDefinition _explosionSound;

	public List<DetonatorTrigger> BoomBoxHandles;

	public List<DetonatorBuySign> BuySigns;

	[Tooltip("This is also where the prefabs spawns from")]
	[SerializeField]
	private List<Transform> _physicsExplosionPositions;

	[Tooltip("Generally these should be physics ore pieces")]
	[SerializeField]
	private List<GameObject> _possiblePrefabsToSpawnDuringExplosion;

	[SerializeField]
	private int _minObjectsToSpawn;

	[SerializeField]
	private int _maxObjectsToSpawn;

	[SerializeField]
	private float _spawnOffsetRadius = 0.5f;

	private DetonatorExplosionState _detonatorExplosionState;

	private void Awake()
	{
		Initialize(_detonatorExplosionState);
	}

	public void Initialize(DetonatorExplosionState detonatorExplosionState)
	{
		_detonatorExplosionState = detonatorExplosionState;
		if (HasExploded())
		{
			StuffToDisable.SetActive(value: false);
			if (TNTObject != null)
			{
				TNTObject.gameObject.SetActive(value: false);
			}
		}
		else if (TNTObject != null)
		{
			TNTObject.gameObject.SetActive(HasPurchased());
		}
		foreach (DetonatorTrigger boomBoxHandle in BoomBoxHandles)
		{
			boomBoxHandle.Initialize(this);
		}
		foreach (DetonatorBuySign buySign in BuySigns)
		{
			buySign.Initialize(this);
		}
	}

	public bool HasPurchased()
	{
		if (_detonatorExplosionState == DetonatorExplosionState.Available && BuySigns.Count == 0)
		{
			_detonatorExplosionState = DetonatorExplosionState.Purchased;
			return true;
		}
		if (_detonatorExplosionState != DetonatorExplosionState.Purchased)
		{
			return _detonatorExplosionState == DetonatorExplosionState.Exploded;
		}
		return true;
	}

	public bool HasExploded()
	{
		return _detonatorExplosionState == DetonatorExplosionState.Exploded;
	}

	[Obsolete]
	public static void TriggerExplosionID(int detonatorID, bool fromLoadingSaveFile = false)
	{
		DetonatorExplosion[] array = UnityEngine.Object.FindObjectsOfType<DetonatorExplosion>();
		foreach (DetonatorExplosion detonatorExplosion in array)
		{
			if (detonatorExplosion.DetonatorID == detonatorID)
			{
				detonatorExplosion.Explode(fromLoadingSaveFile);
			}
		}
	}

	public void PurchaseTNT()
	{
		if (TNTObject != null)
		{
			TNTObject.SetActive(value: true);
		}
		_detonatorExplosionState = DetonatorExplosionState.Purchased;
		foreach (DetonatorBuySign buySign in BuySigns)
		{
			buySign.gameObject.SetActive(value: false);
		}
		foreach (DetonatorTrigger boomBoxHandle in BoomBoxHandles)
		{
			boomBoxHandle.ToggleHandle(enabled: true);
		}
	}

	public void Explode(bool fromLoadingSaveFile = false, DetonatorTrigger activatingTrigger = null)
	{
		if (!HasExploded())
		{
			_detonatorExplosionState = DetonatorExplosionState.Exploded;
			if (!fromLoadingSaveFile)
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(_explosionSound, ParticleObject.transform.position);
				ParticleObject.SetActive(value: true);
			}
			StuffToDisable.SetActive(value: false);
			if (TNTObject != null)
			{
				TNTObject.SetActive(value: false);
			}
			if (!fromLoadingSaveFile)
			{
				StartCoroutine(ExplosionCoroutine(activatingTrigger));
			}
		}
	}

	public IEnumerator ExplosionCoroutine(DetonatorTrigger activatingTrigger)
	{
		yield return new WaitForFixedUpdate();
		int num = UnityEngine.Random.Range(_minObjectsToSpawn, _maxObjectsToSpawn + 1);
		for (int i = 0; i < num; i++)
		{
			GameObject prefab = _possiblePrefabsToSpawnDuringExplosion[UnityEngine.Random.Range(0, _possiblePrefabsToSpawnDuringExplosion.Count)];
			Transform transform = _physicsExplosionPositions[UnityEngine.Random.Range(0, _physicsExplosionPositions.Count)];
			Vector3 vector = UnityEngine.Random.insideUnitSphere * _spawnOffsetRadius;
			Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(prefab, transform.position + vector, transform.rotation);
		}
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		foreach (Transform physicsExplosionPosition in _physicsExplosionPositions)
		{
			PhysicsUtils.SimpleExplosion(physicsExplosionPosition.position, 8f, 8f, 0.5f);
		}
		foreach (DetonatorTrigger boomBoxHandle in BoomBoxHandles)
		{
			if (boomBoxHandle != activatingTrigger)
			{
				boomBoxHandle.RemoveDetonatorTrigger();
			}
		}
	}

	public SavableWorldEventType GetWorldEventType()
	{
		return SavableWorldEventType.TNTDetonator;
	}

	public bool GetHasHappened()
	{
		return _detonatorExplosionState != DetonatorExplosionState.Available;
	}

	public int GetWorldEventID()
	{
		return DetonatorID;
	}

	public virtual void LoadFromSave(string json)
	{
		DetonatorExplosionSaveData detonatorExplosionSaveData = JsonUtility.FromJson<DetonatorExplosionSaveData>(json);
		if (detonatorExplosionSaveData == null)
		{
			detonatorExplosionSaveData = new DetonatorExplosionSaveData();
		}
		Initialize(detonatorExplosionSaveData.DetonatorExplosionState);
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new DetonatorExplosionSaveData
		{
			DetonatorExplosionState = _detonatorExplosionState
		});
	}
}
