using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000066 RID: 102
[RequireComponent(typeof(AudioSource))]
[DefaultExecutionOrder(-1)]
public class LoopingSoundPlayer : MonoBehaviour
{
	// Token: 0x0600029E RID: 670 RVA: 0x0000CC64 File Offset: 0x0000AE64
	private void Awake()
	{
		int num = LayerMask.NameToLayer(LoopingSoundPlayer._triggerLayerName);
		if (num != -1)
		{
			base.gameObject.layer = num;
		}
		this.AudioSource = base.GetComponent<AudioSource>();
		this.AudioSource.loop = true;
		this.AudioSource.playOnAwake = false;
		SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
		sphereCollider.isTrigger = true;
		sphereCollider.radius = this.AudioSource.maxDistance;
	}

	// Token: 0x0600029F RID: 671 RVA: 0x0000CCD2 File Offset: 0x0000AED2
	private void OnEnable()
	{
		if (this.ShouldPlay)
		{
			base.StartCoroutine(this.WaitThenPlay());
		}
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x0000CCE9 File Offset: 0x0000AEE9
	private IEnumerator WaitThenPlay()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForSeconds((float)Random.Range(0, 1));
		if (this.ShouldPlay && this._isInRange)
		{
			this.AudioSource.Play();
		}
		yield break;
	}

	// Token: 0x060002A1 RID: 673 RVA: 0x0000CCF8 File Offset: 0x0000AEF8
	public void Toggle(bool shouldPlay)
	{
		if (shouldPlay)
		{
			this.Play();
			return;
		}
		this.Pause();
	}

	// Token: 0x060002A2 RID: 674 RVA: 0x0000CD0A File Offset: 0x0000AF0A
	public void Play()
	{
		this.ShouldPlay = true;
		if (this._isInRange)
		{
			this.AudioSource.Play();
		}
	}

	// Token: 0x060002A3 RID: 675 RVA: 0x0000CD26 File Offset: 0x0000AF26
	public void Pause()
	{
		this.ShouldPlay = false;
		if (this.AudioSource.isPlaying)
		{
			this.AudioSource.Pause();
		}
	}

	// Token: 0x060002A4 RID: 676 RVA: 0x0000CD47 File Offset: 0x0000AF47
	public void Stop()
	{
		this.ShouldPlay = false;
		if (this.AudioSource.isPlaying)
		{
			this.AudioSource.Stop();
		}
	}

	// Token: 0x060002A5 RID: 677 RVA: 0x0000CD68 File Offset: 0x0000AF68
	private void OnTriggerEnter(Collider other)
	{
		this._isInRange = true;
		if (this.ShouldPlay && !this.AudioSource.isPlaying)
		{
			this.AudioSource.Play();
		}
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x0000CD91 File Offset: 0x0000AF91
	private void OnTriggerExit(Collider other)
	{
		this._isInRange = false;
		if (this.AudioSource.isPlaying)
		{
			this.AudioSource.Stop();
		}
	}

	// Token: 0x04000278 RID: 632
	[HideInInspector]
	public AudioSource AudioSource;

	// Token: 0x04000279 RID: 633
	public bool ShouldPlay = true;

	// Token: 0x0400027A RID: 634
	private static string _triggerLayerName = "LocalPlayerOnlyTrigger";

	// Token: 0x0400027B RID: 635
	private bool _isInRange;
}
