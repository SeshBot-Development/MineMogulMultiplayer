using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(LoopingSoundPlayer))]
public class LoopingSoundFader : MonoBehaviour
{
	private LoopingSoundPlayer _loopingSoundPlayer;

	private float _targetVolume;

	private float _fadeSpeed;

	private bool _fading;

	private void Awake()
	{
		_loopingSoundPlayer = GetComponent<LoopingSoundPlayer>();
		base.enabled = false;
	}

	public float GetCurrentTargetVolume()
	{
		return _targetVolume;
	}

	public void FadeTo(float targetVolume, float duration = -1f)
	{
		_targetVolume = Mathf.Clamp01(targetVolume);
		if (duration <= 0f)
		{
			_fading = false;
			base.enabled = false;
			_loopingSoundPlayer.AudioSource.volume = _targetVolume;
			if (_targetVolume <= 0f)
			{
				_loopingSoundPlayer.Stop();
			}
			else if (!_loopingSoundPlayer.AudioSource.isPlaying)
			{
				_loopingSoundPlayer.Play();
			}
			return;
		}
		if (!_loopingSoundPlayer.AudioSource.isPlaying)
		{
			_loopingSoundPlayer.Play();
		}
		_fadeSpeed = Mathf.Abs(_targetVolume - _loopingSoundPlayer.AudioSource.volume) / duration;
		if (_fadeSpeed <= 0f)
		{
			_fading = false;
			base.enabled = false;
			if (_targetVolume <= 0f)
			{
				_loopingSoundPlayer.Stop();
			}
		}
		else
		{
			_fading = true;
			base.enabled = true;
		}
	}

	private void Update()
	{
		if (!_fading)
		{
			return;
		}
		float num = Mathf.MoveTowards(_loopingSoundPlayer.AudioSource.volume, _targetVolume, _fadeSpeed * Time.deltaTime);
		_loopingSoundPlayer.AudioSource.volume = num;
		if (Mathf.Approximately(num, _targetVolume))
		{
			_fading = false;
			base.enabled = false;
			if (_targetVolume <= 0f)
			{
				_loopingSoundPlayer.Stop();
			}
		}
	}
}
