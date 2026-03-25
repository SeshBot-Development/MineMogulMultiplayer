using UnityEngine;

public class DamageableOrePiece : OrePiece, IDamageable
{
	[Header("Damageable Ore Piece Settings")]
	public float Health = 10f;

	public SoundDefinition PhysicsImpactSound;

	private float _minImpactVelocity = 1f;

	private float _maxVolumeVelocity = 30f;

	private float _cooldown = 0.1f;

	private float _minDamageVelocity = 30f;

	private float _lastPlayTime;

	public void TakeDamage(float damage, Vector3 position)
	{
		Health -= damage;
		if (Health <= 0f)
		{
			Vector3 position2 = base.transform.position;
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Node_Break, position2);
			Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.BreakOreNodeParticlePrefab, position2);
			CompleteClusterBreaking();
			PhysicsUtils.SimpleExplosion(position2, 0.5f, 2f, 0.1f);
		}
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

	private void OnValidate()
	{
		if (GetComponent<PhysicsSoundPlayer>() != null)
		{
			Debug.LogError("DamageableOrePiece already contains PhysicsSoundPlayer functionality. Please remove PhysicsSoundPlayer from " + base.name, this);
		}
	}
}
