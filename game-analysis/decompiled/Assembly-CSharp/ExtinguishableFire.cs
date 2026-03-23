using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000103 RID: 259
public class ExtinguishableFire : MonoBehaviour
{
	// Token: 0x060006E7 RID: 1767 RVA: 0x00023230 File Offset: 0x00021430
	private void Start()
	{
		this.m_isExtinguished = true;
		this.smokeParticleSystem.Stop();
		this.fireParticleSystem.Stop();
		base.StartCoroutine(this.StartingFire());
	}

	// Token: 0x060006E8 RID: 1768 RVA: 0x0002325C File Offset: 0x0002145C
	public void Extinguish()
	{
		if (this.m_isExtinguished)
		{
			return;
		}
		this.m_isExtinguished = true;
		base.StartCoroutine(this.Extinguishing());
	}

	// Token: 0x060006E9 RID: 1769 RVA: 0x0002327B File Offset: 0x0002147B
	private IEnumerator Extinguishing()
	{
		this.fireParticleSystem.Stop();
		this.smokeParticleSystem.time = 0f;
		this.smokeParticleSystem.Play();
		for (float elapsedTime = 0f; elapsedTime < 2f; elapsedTime += Time.deltaTime)
		{
			float num = Mathf.Max(0f, 1f - elapsedTime / 2f);
			this.fireParticleSystem.transform.localScale = Vector3.one * num;
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		this.smokeParticleSystem.Stop();
		this.fireParticleSystem.transform.localScale = Vector3.one;
		yield return new WaitForSeconds(4f);
		base.StartCoroutine(this.StartingFire());
		yield break;
	}

	// Token: 0x060006EA RID: 1770 RVA: 0x0002328A File Offset: 0x0002148A
	private IEnumerator StartingFire()
	{
		this.smokeParticleSystem.Stop();
		this.fireParticleSystem.time = 0f;
		this.fireParticleSystem.Play();
		for (float elapsedTime = 0f; elapsedTime < 2f; elapsedTime += Time.deltaTime)
		{
			float num = Mathf.Min(1f, elapsedTime / 2f);
			this.fireParticleSystem.transform.localScale = Vector3.one * num;
			yield return null;
		}
		this.fireParticleSystem.transform.localScale = Vector3.one;
		this.m_isExtinguished = false;
		yield break;
	}

	// Token: 0x040007EA RID: 2026
	public ParticleSystem fireParticleSystem;

	// Token: 0x040007EB RID: 2027
	public ParticleSystem smokeParticleSystem;

	// Token: 0x040007EC RID: 2028
	protected bool m_isExtinguished;

	// Token: 0x040007ED RID: 2029
	private const float m_FireStartingTime = 2f;
}
