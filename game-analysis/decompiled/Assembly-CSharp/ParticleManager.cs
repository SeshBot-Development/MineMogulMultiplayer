using System;
using UnityEngine;

// Token: 0x0200007F RID: 127
public class ParticleManager : Singleton<ParticleManager>
{
	// Token: 0x0600035E RID: 862 RVA: 0x00010F64 File Offset: 0x0000F164
	public void CreateParticle(GameObject particlePrefab, Vector3 position, Vector3 rotation, Vector3 scale = default(Vector3))
	{
		this.CreateParticle(particlePrefab, position, Quaternion.Euler(rotation), scale);
	}

	// Token: 0x0600035F RID: 863 RVA: 0x00010F78 File Offset: 0x0000F178
	public void CreateParticle(GameObject particlePrefab, Vector3 position, Quaternion rotation = default(Quaternion), Vector3 scale = default(Vector3))
	{
		GameObject gameObject = Object.Instantiate<GameObject>(particlePrefab, position, rotation);
		if (scale != default(Vector3))
		{
			gameObject.transform.localScale = scale;
		}
	}

	// Token: 0x0400035C RID: 860
	public GameObject GenericHitImpactParticle;

	// Token: 0x0400035D RID: 861
	public GameObject OreNodeHitParticlePrefab;

	// Token: 0x0400035E RID: 862
	public GameObject BreakOreNodeParticlePrefab;
}
