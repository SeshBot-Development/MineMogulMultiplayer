using System;
using UnityEngine;

// Token: 0x02000029 RID: 41
public class CollisionDisabler : MonoBehaviour
{
	// Token: 0x06000135 RID: 309 RVA: 0x00007540 File Offset: 0x00005740
	private void Start()
	{
		for (int i = 0; i < this.colliders.Length; i++)
		{
			for (int j = i + 1; j < this.colliders.Length; j++)
			{
				if (this.colliders[i] != null && this.colliders[j] != null)
				{
					Physics.IgnoreCollision(this.colliders[i], this.colliders[j], true);
				}
			}
		}
	}

	// Token: 0x04000131 RID: 305
	public Collider[] colliders;
}
