using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

// Token: 0x020000E9 RID: 233
[RequireComponent(typeof(TMP_Text))]
public class TMPBounceEffect : MonoBehaviour
{
	// Token: 0x06000635 RID: 1589 RVA: 0x000206AE File Offset: 0x0001E8AE
	private void Awake()
	{
		this._text = base.GetComponent<TMP_Text>();
	}

	// Token: 0x06000636 RID: 1590 RVA: 0x000206BC File Offset: 0x0001E8BC
	private void OnEnable()
	{
		this.StartBounce();
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x000206C4 File Offset: 0x0001E8C4
	private void OnDisable()
	{
		this.StopBounce();
	}

	// Token: 0x06000638 RID: 1592 RVA: 0x000206CC File Offset: 0x0001E8CC
	public void StartBounce()
	{
		if (this._seq != null && this._seq.IsActive())
		{
			return;
		}
		this._text.ForceMeshUpdate(false, false);
		this._animator = new DOTweenTMPAnimator(this._text);
		this._seq = DOTween.Sequence().SetTarget(this._text);
		int characterCount = this._animator.textInfo.characterCount;
		for (int i = 0; i < characterCount; i++)
		{
			if (this._animator.textInfo.characterInfo[i].isVisible)
			{
				float num = (float)i * this.stagger;
				this._seq.Join(this._animator.DOOffsetChar(i, new Vector3(0f, this.bounceHeight, 0f), this.duration).SetLoops(int.MaxValue, LoopType.Yoyo).SetEase(this.ease)
					.SetDelay(num));
				this._seq.Join(this._animator.DOColorChar(i, this.targetColor, this.colorDuration).SetLoops(int.MaxValue, LoopType.Yoyo).SetDelay(num));
			}
		}
		float num2 = Random.Range(this.randomStartDelayMin, this.randomStartDelayMax);
		this._seq.SetDelay(num2);
	}

	// Token: 0x06000639 RID: 1593 RVA: 0x00020814 File Offset: 0x0001EA14
	public void StopBounce()
	{
		if (this._seq != null && this._seq.IsActive())
		{
			this._seq.Kill(false);
		}
		this._seq = null;
		this._animator = null;
		this._text.color = this.startColor;
	}

	// Token: 0x0600063A RID: 1594 RVA: 0x00020861 File Offset: 0x0001EA61
	public void Rebuild()
	{
		this.StopBounce();
		this.StartBounce();
	}

	// Token: 0x04000778 RID: 1912
	[Header("Bounce Settings")]
	public float bounceHeight = 2.5f;

	// Token: 0x04000779 RID: 1913
	public float duration = 0.35f;

	// Token: 0x0400077A RID: 1914
	public float stagger = 0.04f;

	// Token: 0x0400077B RID: 1915
	public Ease ease = Ease.InOutSine;

	// Token: 0x0400077C RID: 1916
	[Header("Color Settings")]
	public Color startColor = Color.white;

	// Token: 0x0400077D RID: 1917
	public Color targetColor = Color.red;

	// Token: 0x0400077E RID: 1918
	public float colorDuration = 0.8f;

	// Token: 0x0400077F RID: 1919
	[Header("Randomization")]
	[Tooltip("Randomized start delay for the whole effect.")]
	public float randomStartDelayMin;

	// Token: 0x04000780 RID: 1920
	public float randomStartDelayMax = 0.2f;

	// Token: 0x04000781 RID: 1921
	private TMP_Text _text;

	// Token: 0x04000782 RID: 1922
	private DOTweenTMPAnimator _animator;

	// Token: 0x04000783 RID: 1923
	private Sequence _seq;
}
