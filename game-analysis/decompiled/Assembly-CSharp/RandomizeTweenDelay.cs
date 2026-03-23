using System;
using DG.Tweening;
using UnityEngine;

// Token: 0x0200009E RID: 158
public class RandomizeTweenDelay : MonoBehaviour
{
	// Token: 0x06000445 RID: 1093 RVA: 0x0001782E File Offset: 0x00015A2E
	private void Awake()
	{
		this.DOTweenAnimation = base.GetComponent<DOTweenAnimation>();
		this.DOTweenAnimation.delay = Random.Range(this.minDelay, this.maxDelay);
		this.DOTweenAnimation.DORestart();
	}

	// Token: 0x040004EA RID: 1258
	public DOTweenAnimation DOTweenAnimation;

	// Token: 0x040004EB RID: 1259
	public float minDelay;

	// Token: 0x040004EC RID: 1260
	public float maxDelay = 1f;
}
