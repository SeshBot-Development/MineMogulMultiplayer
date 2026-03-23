using System;
using UnityEngine;

// Token: 0x0200003D RID: 61
public class DamageableOrePiece : OrePiece, IDamageable
{
	// Token: 0x060001A1 RID: 417 RVA: 0x00008D18 File Offset: 0x00006F18
	public void TakeDamage(float damage, Vector3 position)
	{
		this.Health -= damage;
		if (this.Health <= 0f)
		{
			Vector3 position2 = base.transform.position;
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Node_Break, position2, 1f, 1f, true, false);
			Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.BreakOreNodeParticlePrefab, position2, default(Quaternion), default(Vector3));
			base.CompleteClusterBreaking();
			PhysicsUtils.SimpleExplosion(position2, 0.5f, 2f, 0.1f);
		}
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x00008DB0 File Offset: 0x00006FB0
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

	// Token: 0x060001A3 RID: 419 RVA: 0x00008EBF File Offset: 0x000070BF
	public void PlayImpactSound(float volume = 1f, float pitch = 1f)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.PhysicsImpactSound, base.transform.position, volume, pitch, true, false);
	}

	// Token: 0x060001A4 RID: 420 RVA: 0x00008EE0 File Offset: 0x000070E0
	private void OnValidate()
	{
		if (base.GetComponent<PhysicsSoundPlayer>() != null)
		{
			Debug.LogError("DamageableOrePiece already contains PhysicsSoundPlayer functionality. Please remove PhysicsSoundPlayer from " + base.name, this);
		}
	}

	// Token: 0x04000188 RID: 392
	[Header("Damageable Ore Piece Settings")]
	public float Health = 10f;

	// Token: 0x04000189 RID: 393
	public SoundDefinition PhysicsImpactSound;

	// Token: 0x0400018A RID: 394
	private float _minImpactVelocity = 1f;

	// Token: 0x0400018B RID: 395
	private float _maxVolumeVelocity = 30f;

	// Token: 0x0400018C RID: 396
	private float _cooldown = 0.1f;

	// Token: 0x0400018D RID: 397
	private float _minDamageVelocity = 30f;

	// Token: 0x0400018E RID: 398
	private float _lastPlayTime;
}
