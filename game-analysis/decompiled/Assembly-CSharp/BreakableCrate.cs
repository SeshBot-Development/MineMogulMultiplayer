using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000012 RID: 18
public class BreakableCrate : BaseSellableItem, IDamageable, ISaveLoadableObject
{
	// Token: 0x06000088 RID: 136 RVA: 0x00003D38 File Offset: 0x00001F38
	protected override void Awake()
	{
		base.Awake();
		if (this.GibsContainer != null)
		{
			this.GibsContainer.SetActive(false);
			this._gibs = new List<PhysicsGib>();
			foreach (PhysicsGib physicsGib in this.GibsContainer.GetComponentsInChildren<PhysicsGib>(true))
			{
				this._gibs.Add(physicsGib);
			}
		}
	}

	// Token: 0x06000089 RID: 137 RVA: 0x00003D9C File Offset: 0x00001F9C
	public void TakeDamage(float damage, Vector3 position)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._takeDamageSoundDefinition, position, 1f, 1f, true, false);
		this.Health -= damage;
		if (this.Health <= 0f)
		{
			this.BreakCrate(position);
		}
	}

	// Token: 0x0600008A RID: 138 RVA: 0x00003DE8 File Offset: 0x00001FE8
	public void BreakCrate(Vector3 position)
	{
		int num = Random.Range(this.MinDrops, this.MaxDrops + 1);
		Vector3 vector = (base.transform.position + position) * 0.5f;
		for (int i = 0; i < num; i++)
		{
			vector += Random.insideUnitSphere * 0.15f;
			OrePiece orePrefab = this.GetOrePrefab();
			if (orePrefab != null)
			{
				Rigidbody component = Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePrefab, vector, Quaternion.identity, null).GetComponent<Rigidbody>();
				if (component != null)
				{
					component.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(2f, 4f), Random.Range(-1.5f, 1.5f));
					component.angularVelocity = Random.insideUnitSphere * Random.Range(1f, 50f);
				}
			}
		}
		if (this._gibs != null)
		{
			Vector3 vector2 = base.Rb.linearVelocity * 0.75f;
			foreach (PhysicsGib physicsGib in this._gibs)
			{
				if (Random.Range(0f, 1f) <= this._individualGibSpawnChance)
				{
					physicsGib.DetatchAndDespawn(new Vector3?(vector2));
				}
			}
		}
		PhysicsUtils.SimpleExplosion(base.transform.position, 1f, 2f, 0.5f);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._breakSoundDefinition, base.transform.position, 1f, 1f, true, false);
		Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.BreakOreNodeParticlePrefab, position, default(Quaternion), default(Vector3));
		Object.Destroy(base.gameObject);
	}

	// Token: 0x0600008B RID: 139 RVA: 0x00003FD8 File Offset: 0x000021D8
	public OrePiece GetOrePrefab()
	{
		if (this._possibleDrops == null || this._possibleDrops.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (WeightedNodeDrop weightedNodeDrop in this._possibleDrops)
		{
			num += weightedNodeDrop.Weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		foreach (WeightedNodeDrop weightedNodeDrop2 in this._possibleDrops)
		{
			num3 += weightedNodeDrop2.Weight;
			if (num2 <= num3)
			{
				return weightedNodeDrop2.OrePrefab;
			}
		}
		return this._possibleDrops[this._possibleDrops.Count - 1].OrePrefab;
	}

	// Token: 0x0600008C RID: 140 RVA: 0x000040D0 File Offset: 0x000022D0
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contactCount == 0)
		{
			return;
		}
		if (Time.time - this._lastPlayTime < this._cooldown)
		{
			return;
		}
		this._lastPlayTime = Time.time;
		float sqrMagnitude = collision.relativeVelocity.sqrMagnitude;
		if (sqrMagnitude > this._minDamageVelocity)
		{
			float num = (sqrMagnitude - this._minDamageVelocity) * 0.1f;
			this.TakeDamage(num, base.transform.position);
		}
		Vector3 point = collision.GetContact(0).point;
		float sqrMagnitude2 = (Singleton<SoundManager>.Instance.PlayerTransform.position - point).sqrMagnitude;
		float num2 = this.PhysicsImpactSound.maxRange * 1.1f;
		if (sqrMagnitude2 > num2 * num2)
		{
			return;
		}
		if (sqrMagnitude > this._minImpactVelocity)
		{
			float num3 = Mathf.InverseLerp(this._minImpactVelocity, this._maxVolumeVelocity, sqrMagnitude);
			float num4 = Mathf.Lerp(0.7f, 1.4f, Mathf.InverseLerp(this._minImpactVelocity, this._maxVolumeVelocity, sqrMagnitude));
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.PhysicsImpactSound, point, num3, num4, false, false);
		}
	}

	// Token: 0x0600008D RID: 141 RVA: 0x000041DF File Offset: 0x000023DF
	public void PlayImpactSound(float volume = 1f, float pitch = 1f)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.PhysicsImpactSound, base.transform.position, volume, pitch, true, false);
	}

	// Token: 0x0600008E RID: 142 RVA: 0x00004200 File Offset: 0x00002400
	public bool ShouldBeSaved()
	{
		return true;
	}

	// Token: 0x0600008F RID: 143 RVA: 0x00004203 File Offset: 0x00002403
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x06000090 RID: 144 RVA: 0x00004210 File Offset: 0x00002410
	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	// Token: 0x06000091 RID: 145 RVA: 0x00004230 File Offset: 0x00002430
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x1700000B RID: 11
	// (get) Token: 0x06000092 RID: 146 RVA: 0x00004238 File Offset: 0x00002438
	// (set) Token: 0x06000093 RID: 147 RVA: 0x00004240 File Offset: 0x00002440
	public bool HasBeenSaved { get; set; }

	// Token: 0x06000094 RID: 148 RVA: 0x00004249 File Offset: 0x00002449
	public virtual void LoadFromSave(string json)
	{
	}

	// Token: 0x06000095 RID: 149 RVA: 0x0000424B File Offset: 0x0000244B
	public virtual string GetCustomSaveData()
	{
		return null;
	}

	// Token: 0x04000077 RID: 119
	public SavableObjectID SavableObjectID;

	// Token: 0x04000078 RID: 120
	public float Health;

	// Token: 0x04000079 RID: 121
	public int MinDrops;

	// Token: 0x0400007A RID: 122
	public int MaxDrops;

	// Token: 0x0400007B RID: 123
	[SerializeField]
	private float _individualGibSpawnChance = 0.6f;

	// Token: 0x0400007C RID: 124
	[SerializeField]
	private List<WeightedNodeDrop> _possibleDrops = new List<WeightedNodeDrop>();

	// Token: 0x0400007D RID: 125
	[SerializeField]
	private GameObject GibsContainer;

	// Token: 0x0400007E RID: 126
	[SerializeField]
	private SoundDefinition _takeDamageSoundDefinition;

	// Token: 0x0400007F RID: 127
	[SerializeField]
	private SoundDefinition _breakSoundDefinition;

	// Token: 0x04000080 RID: 128
	public List<PhysicsGib> _gibs = new List<PhysicsGib>();

	// Token: 0x04000081 RID: 129
	public SoundDefinition PhysicsImpactSound;

	// Token: 0x04000082 RID: 130
	private float _minImpactVelocity = 1f;

	// Token: 0x04000083 RID: 131
	private float _maxVolumeVelocity = 30f;

	// Token: 0x04000084 RID: 132
	private float _cooldown = 0.1f;

	// Token: 0x04000085 RID: 133
	private float _minDamageVelocity = 40f;

	// Token: 0x04000086 RID: 134
	private float _lastPlayTime;
}
