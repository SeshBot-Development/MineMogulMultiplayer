using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000106 RID: 262
public class ParticleCollision : MonoBehaviour
{
	// Token: 0x060006F5 RID: 1781 RVA: 0x0002360C File Offset: 0x0002180C
	private void Start()
	{
		this.m_ParticleSystem = base.GetComponent<ParticleSystem>();
	}

	// Token: 0x060006F6 RID: 1782 RVA: 0x0002361C File Offset: 0x0002181C
	private void OnParticleCollision(GameObject other)
	{
		int collisionEvents = this.m_ParticleSystem.GetCollisionEvents(other, this.m_CollisionEvents);
		for (int i = 0; i < collisionEvents; i++)
		{
			ExtinguishableFire component = this.m_CollisionEvents[i].colliderComponent.GetComponent<ExtinguishableFire>();
			if (component != null)
			{
				component.Extinguish();
			}
		}
	}

	// Token: 0x04000803 RID: 2051
	private List<ParticleCollisionEvent> m_CollisionEvents = new List<ParticleCollisionEvent>();

	// Token: 0x04000804 RID: 2052
	private ParticleSystem m_ParticleSystem;
}
