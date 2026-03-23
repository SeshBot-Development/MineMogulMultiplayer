using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000081 RID: 129
public class PhysicsGib : BaseSellableItem
{
	// Token: 0x0600037F RID: 895 RVA: 0x00011534 File Offset: 0x0000F734
	public void DetatchAndDespawn(Vector3? velocity = null)
	{
		base.transform.SetParent(null);
		base.gameObject.SetActive(true);
		PhysicsUtils.IgnoreAllCollisions(base.gameObject, Singleton<SoundManager>.Instance.PlayerTransform.gameObject, true);
		if (velocity != null)
		{
			base.Rb.linearVelocity = velocity.Value;
		}
		base.StartCoroutine(this.WaitThenDespawn());
	}

	// Token: 0x06000380 RID: 896 RVA: 0x0001159C File Offset: 0x0000F79C
	private IEnumerator WaitThenDespawn()
	{
		float num = this._despawnTime * Random.Range(0.7f, 1.3f);
		yield return new WaitForSeconds(num);
		if (this != null)
		{
			Object.Destroy(base.gameObject);
		}
		yield break;
	}

	// Token: 0x0400036E RID: 878
	private float _despawnTime = 8f;
}
