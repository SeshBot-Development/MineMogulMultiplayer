using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
	private List<ParticleCollisionEvent> m_CollisionEvents = new List<ParticleCollisionEvent>();

	private ParticleSystem m_ParticleSystem;

	private void Start()
	{
		m_ParticleSystem = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject other)
	{
		int collisionEvents = m_ParticleSystem.GetCollisionEvents(other, m_CollisionEvents);
		for (int i = 0; i < collisionEvents; i++)
		{
			ExtinguishableFire component = m_CollisionEvents[i].colliderComponent.GetComponent<ExtinguishableFire>();
			if (component != null)
			{
				component.Extinguish();
			}
		}
	}
}
