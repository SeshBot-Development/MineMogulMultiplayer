using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000102 RID: 258
public class DecalDestroyer : MonoBehaviour
{
	// Token: 0x060006E5 RID: 1765 RVA: 0x0002320E File Offset: 0x0002140E
	private IEnumerator Start()
	{
		yield return new WaitForSeconds(this.lifeTime);
		Object.Destroy(base.gameObject);
		yield break;
	}

	// Token: 0x040007E9 RID: 2025
	public float lifeTime = 5f;
}
