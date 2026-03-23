using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000E4 RID: 228
public class SoundPlayer : MonoBehaviour
{
	// Token: 0x06000619 RID: 1561 RVA: 0x0001FE6C File Offset: 0x0001E06C
	public void PlaySound(SoundDefinition soundDefinition, float volumeMultiplier = 1f, float pitchMultiplier = 1f, bool isUISound = false)
	{
		if (!this.IsPoolMember)
		{
			float sqrMagnitude = (Singleton<SoundManager>.Instance.PlayerTransform.position - base.transform.position).sqrMagnitude;
			float num = soundDefinition.maxRange * 1.25f;
			if (sqrMagnitude > num * num)
			{
				return;
			}
		}
		AudioClipDescription sound = soundDefinition.GetSound();
		if (sound.clip != null)
		{
			this._audioSource.spatialBlend = (float)(isUISound ? 0 : 1);
			this._audioSource.volume = sound.volume * volumeMultiplier;
			this._audioSource.maxDistance = sound.maxRange;
			this._audioSource.pitch = sound.pitch * pitchMultiplier;
			this._audioSource.priority = sound.priority;
			this._audioSource.PlayOneShot(sound.clip);
			if (this.IsPoolMember)
			{
				base.StartCoroutine(this.ReturnToPoolAfterSound(sound.clip.length));
				return;
			}
		}
		else if (this.IsPoolMember)
		{
			base.StartCoroutine(this.ReturnToPoolAfterSound(0f));
		}
	}

	// Token: 0x0600061A RID: 1562 RVA: 0x0001FF7B File Offset: 0x0001E17B
	public void PlaySound(SoundDefinition soundDefinition)
	{
		this.PlaySound(soundDefinition, 1f, 1f, false);
	}

	// Token: 0x0600061B RID: 1563 RVA: 0x0001FF8F File Offset: 0x0001E18F
	public void Pause()
	{
		this._audioSource.Pause();
	}

	// Token: 0x0600061C RID: 1564 RVA: 0x0001FF9C File Offset: 0x0001E19C
	public void UnPause()
	{
		this._audioSource.UnPause();
	}

	// Token: 0x0600061D RID: 1565 RVA: 0x0001FFAC File Offset: 0x0001E1AC
	public void PlaySound(AudioClip clip, float volume = 1f, float maxRange = 20f)
	{
		this._audioSource.spatialBlend = 1f;
		this._audioSource.volume = volume;
		this._audioSource.maxDistance = maxRange;
		this._audioSource.priority = 180;
		this._audioSource.PlayOneShot(clip);
		if (this.IsPoolMember)
		{
			base.StartCoroutine(this.ReturnToPoolAfterSound(clip.length));
		}
	}

	// Token: 0x0600061E RID: 1566 RVA: 0x00020018 File Offset: 0x0001E218
	private IEnumerator ReturnToPoolAfterSound(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		Singleton<SoundManager>.Instance.ReturnToPool(this);
		yield break;
	}

	// Token: 0x0400075D RID: 1885
	public bool IsPoolMember = true;

	// Token: 0x0400075E RID: 1886
	[SerializeField]
	private AudioSource _audioSource;
}
