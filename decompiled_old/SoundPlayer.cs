using System.Collections;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
	public bool IsPoolMember = true;

	[SerializeField]
	private AudioSource _audioSource;

	public void PlaySound(SoundDefinition soundDefinition, float volumeMultiplier = 1f, float pitchMultiplier = 1f, bool isUISound = false)
	{
		if (!IsPoolMember)
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
			_audioSource.spatialBlend = ((!isUISound) ? 1 : 0);
			_audioSource.volume = sound.volume * volumeMultiplier;
			_audioSource.maxDistance = sound.maxRange;
			_audioSource.pitch = sound.pitch * pitchMultiplier;
			_audioSource.priority = sound.priority;
			_audioSource.PlayOneShot(sound.clip);
			if (IsPoolMember)
			{
				StartCoroutine(ReturnToPoolAfterSound(sound.clip.length));
			}
		}
		else if (IsPoolMember)
		{
			StartCoroutine(ReturnToPoolAfterSound(0f));
		}
	}

	public void PlaySound(SoundDefinition soundDefinition)
	{
		PlaySound(soundDefinition, 1f, 1f, isUISound: false);
	}

	public void Pause()
	{
		_audioSource.Pause();
	}

	public void UnPause()
	{
		_audioSource.UnPause();
	}

	public void PlaySound(AudioClip clip, float volume = 1f, float maxRange = 20f)
	{
		_audioSource.spatialBlend = 1f;
		_audioSource.volume = volume;
		_audioSource.maxDistance = maxRange;
		_audioSource.priority = 180;
		_audioSource.PlayOneShot(clip);
		if (IsPoolMember)
		{
			StartCoroutine(ReturnToPoolAfterSound(clip.length));
		}
	}

	private IEnumerator ReturnToPoolAfterSound(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		Singleton<SoundManager>.Instance.ReturnToPool(this);
	}
}
