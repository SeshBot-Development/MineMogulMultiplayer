using System;
using UnityEngine;

// Token: 0x02000065 RID: 101
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(LoopingSoundPlayer))]
public class LoopingSoundFader : MonoBehaviour
{
	// Token: 0x06000299 RID: 665 RVA: 0x0000CAB8 File Offset: 0x0000ACB8
	private void Awake()
	{
		this._loopingSoundPlayer = base.GetComponent<LoopingSoundPlayer>();
		base.enabled = false;
	}

	// Token: 0x0600029A RID: 666 RVA: 0x0000CACD File Offset: 0x0000ACCD
	public float GetCurrentTargetVolume()
	{
		return this._targetVolume;
	}

	// Token: 0x0600029B RID: 667 RVA: 0x0000CAD8 File Offset: 0x0000ACD8
	public void FadeTo(float targetVolume, float duration = -1f)
	{
		this._targetVolume = Mathf.Clamp01(targetVolume);
		if (duration <= 0f)
		{
			this._fading = false;
			base.enabled = false;
			this._loopingSoundPlayer.AudioSource.volume = this._targetVolume;
			if (this._targetVolume <= 0f)
			{
				this._loopingSoundPlayer.Stop();
				return;
			}
			if (!this._loopingSoundPlayer.AudioSource.isPlaying)
			{
				this._loopingSoundPlayer.Play();
			}
			return;
		}
		else
		{
			if (!this._loopingSoundPlayer.AudioSource.isPlaying)
			{
				this._loopingSoundPlayer.Play();
			}
			this._fadeSpeed = Mathf.Abs(this._targetVolume - this._loopingSoundPlayer.AudioSource.volume) / duration;
			if (this._fadeSpeed <= 0f)
			{
				this._fading = false;
				base.enabled = false;
				if (this._targetVolume <= 0f)
				{
					this._loopingSoundPlayer.Stop();
				}
				return;
			}
			this._fading = true;
			base.enabled = true;
			return;
		}
	}

	// Token: 0x0600029C RID: 668 RVA: 0x0000CBD8 File Offset: 0x0000ADD8
	private void Update()
	{
		if (!this._fading)
		{
			return;
		}
		float num = Mathf.MoveTowards(this._loopingSoundPlayer.AudioSource.volume, this._targetVolume, this._fadeSpeed * Time.deltaTime);
		this._loopingSoundPlayer.AudioSource.volume = num;
		if (Mathf.Approximately(num, this._targetVolume))
		{
			this._fading = false;
			base.enabled = false;
			if (this._targetVolume <= 0f)
			{
				this._loopingSoundPlayer.Stop();
			}
		}
	}

	// Token: 0x04000274 RID: 628
	private LoopingSoundPlayer _loopingSoundPlayer;

	// Token: 0x04000275 RID: 629
	private float _targetVolume;

	// Token: 0x04000276 RID: 630
	private float _fadeSpeed;

	// Token: 0x04000277 RID: 631
	private bool _fading;
}
