using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class ConveyorSplitterT2 : MonoBehaviour
{
	// Token: 0x06000198 RID: 408 RVA: 0x00008BB9 File Offset: 0x00006DB9
	private void OnEnable()
	{
		this.swingRoutine = base.StartCoroutine(this.SwingLoop());
		this._timeSinceLastObject = 0f;
	}

	// Token: 0x06000199 RID: 409 RVA: 0x00008BDD File Offset: 0x00006DDD
	private void OnDisable()
	{
		if (this.swingRoutine != null)
		{
			base.StopCoroutine(this.swingRoutine);
		}
	}

	// Token: 0x0600019A RID: 410 RVA: 0x00008BF3 File Offset: 0x00006DF3
	private void OnTriggerEnter(Collider other)
	{
		this._timeSinceLastObject = 0f;
	}

	// Token: 0x0600019B RID: 411 RVA: 0x00008C05 File Offset: 0x00006E05
	private IEnumerator SwingLoop()
	{
		bool goingToMax = true;
		for (;;)
		{
			if ((in this._timeSinceLastObject) > this.IdleTime)
			{
				yield return new WaitForSeconds(0.25f);
			}
			else
			{
				float startAngle = (goingToMax ? this.minY : this.maxY);
				float endAngle = (goingToMax ? this.maxY : this.minY);
				float elapsed = 0f;
				while (elapsed < this.duration)
				{
					elapsed += Time.deltaTime;
					float num = Mathf.Clamp01(elapsed / this.duration);
					float num2 = Mathf.SmoothStep(0f, 1f, num);
					float num3 = Mathf.Lerp(startAngle, endAngle, num2);
					Vector3 localEulerAngles = this.RotatingThing.localEulerAngles;
					this.RotatingThing.localEulerAngles = new Vector3(localEulerAngles.x, num3, localEulerAngles.z);
					yield return null;
				}
				yield return new WaitForSeconds(this.pauseTime);
				goingToMax = !goingToMax;
			}
		}
		yield break;
	}

	// Token: 0x0400017C RID: 380
	public float minY = -35f;

	// Token: 0x0400017D RID: 381
	public float maxY = 35f;

	// Token: 0x0400017E RID: 382
	public float duration = 1.5f;

	// Token: 0x0400017F RID: 383
	public float pauseTime = 2f;

	// Token: 0x04000180 RID: 384
	public float IdleTime = 1f;

	// Token: 0x04000181 RID: 385
	public Transform RotatingThing;

	// Token: 0x04000182 RID: 386
	private Coroutine swingRoutine;

	// Token: 0x04000183 RID: 387
	private TimeSince _timeSinceLastObject;
}
