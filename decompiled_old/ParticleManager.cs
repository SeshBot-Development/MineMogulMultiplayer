using UnityEngine;

public class ParticleManager : Singleton<ParticleManager>
{
	public GameObject GenericHitImpactParticle;

	public GameObject OreNodeHitParticlePrefab;

	public GameObject BreakOreNodeParticlePrefab;

	public void CreateParticle(GameObject particlePrefab, Vector3 position, Vector3 rotation, Vector3 scale = default(Vector3))
	{
		CreateParticle(particlePrefab, position, Quaternion.Euler(rotation), scale);
	}

	public void CreateParticle(GameObject particlePrefab, Vector3 position, Quaternion rotation = default(Quaternion), Vector3 scale = default(Vector3))
	{
		GameObject gameObject = Object.Instantiate(particlePrefab, position, rotation);
		if (scale != default(Vector3))
		{
			gameObject.transform.localScale = scale;
		}
	}
}
