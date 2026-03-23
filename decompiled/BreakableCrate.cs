using System.Collections.Generic;
using UnityEngine;

public class BreakableCrate : BaseSellableItem, IDamageable, ISaveLoadableObject
{
	public SavableObjectID SavableObjectID;

	public float Health;

	public int MinDrops;

	public int MaxDrops;

	[SerializeField]
	private float _individualGibSpawnChance = 0.6f;

	[SerializeField]
	private List<WeightedNodeDrop> _possibleDrops = new List<WeightedNodeDrop>();

	[SerializeField]
	private GameObject GibsContainer;

	[SerializeField]
	private SoundDefinition _takeDamageSoundDefinition;

	[SerializeField]
	private SoundDefinition _breakSoundDefinition;

	public List<PhysicsGib> _gibs = new List<PhysicsGib>();

	public SoundDefinition PhysicsImpactSound;

	private float _minImpactVelocity = 1f;

	private float _maxVolumeVelocity = 30f;

	private float _cooldown = 0.1f;

	private float _minDamageVelocity = 40f;

	private float _lastPlayTime;

	public bool HasBeenSaved { get; set; }

	protected override void Awake()
	{
		base.Awake();
		if (GibsContainer != null)
		{
			GibsContainer.SetActive(value: false);
			_gibs = new List<PhysicsGib>();
			PhysicsGib[] componentsInChildren = GibsContainer.GetComponentsInChildren<PhysicsGib>(includeInactive: true);
			foreach (PhysicsGib item in componentsInChildren)
			{
				_gibs.Add(item);
			}
		}
	}

	public void TakeDamage(float damage, Vector3 position)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_takeDamageSoundDefinition, position);
		Health -= damage;
		if (Health <= 0f)
		{
			BreakCrate(position);
		}
	}

	public void BreakCrate(Vector3 position)
	{
		int num = Random.Range(MinDrops, MaxDrops + 1);
		Vector3 position2 = (base.transform.position + position) * 0.5f;
		for (int i = 0; i < num; i++)
		{
			position2 += Random.insideUnitSphere * 0.15f;
			OrePiece orePrefab = GetOrePrefab();
			if (orePrefab != null)
			{
				Rigidbody component = Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePrefab, position2, Quaternion.identity).GetComponent<Rigidbody>();
				if (component != null)
				{
					component.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(2f, 4f), Random.Range(-1.5f, 1.5f));
					component.angularVelocity = Random.insideUnitSphere * Random.Range(1f, 50f);
				}
			}
		}
		if (_gibs != null)
		{
			Vector3 value = base.Rb.linearVelocity * 0.75f;
			foreach (PhysicsGib gib in _gibs)
			{
				if (Random.Range(0f, 1f) <= _individualGibSpawnChance)
				{
					gib.DetatchAndDespawn(value);
				}
			}
		}
		PhysicsUtils.SimpleExplosion(base.transform.position, 1f, 2f, 0.5f);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_breakSoundDefinition, base.transform.position);
		Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.BreakOreNodeParticlePrefab, position);
		Object.Destroy(base.gameObject);
	}

	public OrePiece GetOrePrefab()
	{
		if (_possibleDrops == null || _possibleDrops.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (WeightedNodeDrop possibleDrop in _possibleDrops)
		{
			num += possibleDrop.Weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		foreach (WeightedNodeDrop possibleDrop2 in _possibleDrops)
		{
			num3 += possibleDrop2.Weight;
			if (num2 <= num3)
			{
				return possibleDrop2.OrePrefab;
			}
		}
		return _possibleDrops[_possibleDrops.Count - 1].OrePrefab;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contactCount != 0 && !(Time.time - _lastPlayTime < _cooldown))
		{
			_lastPlayTime = Time.time;
			float sqrMagnitude = collision.relativeVelocity.sqrMagnitude;
			if (sqrMagnitude > _minDamageVelocity)
			{
				float damage = (sqrMagnitude - _minDamageVelocity) * 0.1f;
				TakeDamage(damage, base.transform.position);
			}
			Vector3 point = collision.GetContact(0).point;
			float sqrMagnitude2 = (Singleton<SoundManager>.Instance.PlayerTransform.position - point).sqrMagnitude;
			float num = PhysicsImpactSound.maxRange * 1.1f;
			if (!(sqrMagnitude2 > num * num) && sqrMagnitude > _minImpactVelocity)
			{
				float volumeMultiplier = Mathf.InverseLerp(_minImpactVelocity, _maxVolumeVelocity, sqrMagnitude);
				float pitchMultiplier = Mathf.Lerp(0.7f, 1.4f, Mathf.InverseLerp(_minImpactVelocity, _maxVolumeVelocity, sqrMagnitude));
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(PhysicsImpactSound, point, volumeMultiplier, pitchMultiplier, dontPlayIfTooFarFromPlayer: false);
			}
		}
	}

	public void PlayImpactSound(float volume = 1f, float pitch = 1f)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(PhysicsImpactSound, base.transform.position, volume, pitch);
	}

	public bool ShouldBeSaved()
	{
		return true;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	public SavableObjectID GetSavableObjectID()
	{
		return SavableObjectID;
	}

	public virtual void LoadFromSave(string json)
	{
	}

	public virtual string GetCustomSaveData()
	{
		return null;
	}
}
