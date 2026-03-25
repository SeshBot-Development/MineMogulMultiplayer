using UnityEngine;

public class PhysicsSoundPlayer : MonoBehaviour
{
	public SoundDefinition ImpactSound;

	private float _minImpactVelocity = 1f;

	private float _maxVolumeVelocity = 30f;

	private float _cooldown = 0.1f;

	private float _lastPlayTime;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contactCount == 0 || Time.time - _lastPlayTime < _cooldown)
		{
			return;
		}
		_lastPlayTime = Time.time;
		Vector3 point = collision.GetContact(0).point;
		float sqrMagnitude = (Singleton<SoundManager>.Instance.PlayerTransform.position - point).sqrMagnitude;
		float num = ImpactSound.maxRange * 1.1f;
		if (!(sqrMagnitude > num * num))
		{
			float sqrMagnitude2 = collision.relativeVelocity.sqrMagnitude;
			if (sqrMagnitude2 > _minImpactVelocity)
			{
				_lastPlayTime = Time.time;
				float volumeMultiplier = Mathf.InverseLerp(_minImpactVelocity, _maxVolumeVelocity, sqrMagnitude2);
				float pitchMultiplier = Mathf.Lerp(0.7f, 1.4f, Mathf.InverseLerp(_minImpactVelocity, _maxVolumeVelocity, sqrMagnitude2));
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(ImpactSound, point, volumeMultiplier, pitchMultiplier, dontPlayIfTooFarFromPlayer: false);
			}
		}
	}

	public void PlayImpactSound(float volume = 1f, float pitch = 1f)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(ImpactSound, base.transform.position, volume, pitch);
	}
}
