using System;
using UnityEngine;

// Token: 0x02000083 RID: 131
public class PhysicsSoundPlayer : MonoBehaviour
{
	// Token: 0x06000386 RID: 902 RVA: 0x000116BC File Offset: 0x0000F8BC
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
		Vector3 point = collision.GetContact(0).point;
		float sqrMagnitude = (Singleton<SoundManager>.Instance.PlayerTransform.position - point).sqrMagnitude;
		float num = this.ImpactSound.maxRange * 1.1f;
		if (sqrMagnitude > num * num)
		{
			return;
		}
		float sqrMagnitude2 = collision.relativeVelocity.sqrMagnitude;
		if (sqrMagnitude2 > this._minImpactVelocity)
		{
			this._lastPlayTime = Time.time;
			float num2 = Mathf.InverseLerp(this._minImpactVelocity, this._maxVolumeVelocity, sqrMagnitude2);
			float num3 = Mathf.Lerp(0.7f, 1.4f, Mathf.InverseLerp(this._minImpactVelocity, this._maxVolumeVelocity, sqrMagnitude2));
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.ImpactSound, point, num2, num3, false, false);
		}
	}

	// Token: 0x06000387 RID: 903 RVA: 0x000117AA File Offset: 0x0000F9AA
	public void PlayImpactSound(float volume = 1f, float pitch = 1f)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.ImpactSound, base.transform.position, volume, pitch, true, false);
	}

	// Token: 0x04000372 RID: 882
	public SoundDefinition ImpactSound;

	// Token: 0x04000373 RID: 883
	private float _minImpactVelocity = 1f;

	// Token: 0x04000374 RID: 884
	private float _maxVolumeVelocity = 30f;

	// Token: 0x04000375 RID: 885
	private float _cooldown = 0.1f;

	// Token: 0x04000376 RID: 886
	private float _lastPlayTime;
}
