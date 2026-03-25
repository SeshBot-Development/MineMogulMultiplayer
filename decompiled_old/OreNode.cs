using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OreNode : MonoBehaviour, IDamageable, ISaveLoadableStaticBreakable
{
	public ResourceType ResourceType;

	public float Health;

	public int MinDrops;

	public int MaxDrops;

	[SerializeField]
	private List<WeightedNodeDrop> _possibleDrops = new List<WeightedNodeDrop>();

	[SerializeField]
	private GameObject[] _models;

	[SerializeField]
	private SoundDefinition _takeDamageSoundDefinition;

	private void Start()
	{
		if (_models.Length != 0)
		{
			int num = Random.Range(0, _models.Length);
			for (int i = 0; i < _models.Length; i++)
			{
				_models[i].SetActive(i == num);
			}
		}
	}

	public void TakeDamage(float damage, Vector3 position)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_takeDamageSoundDefinition, position);
		Health -= damage;
		if (Health <= 0f)
		{
			BreakNode(position);
		}
	}

	public void BreakNode(Vector3 position)
	{
		int num = Random.Range(MinDrops, MaxDrops + 1);
		Vector3 position2 = (base.transform.position + position) * 0.5f;
		for (int i = 0; i < num; i++)
		{
			position2 += Random.insideUnitSphere * 0.15f;
			Rigidbody component = Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(GetOrePrefab(), position2, Quaternion.identity).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(2f, 4f), Random.Range(-1.5f, 1.5f));
				component.angularVelocity = Random.insideUnitSphere * Random.Range(1f, 50f);
			}
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Node_Break, base.transform.position);
		Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.BreakOreNodeParticlePrefab, position);
		UpdateSupportsAbove();
		MarkStaticPositionAsBroken();
		Object.Destroy(base.gameObject);
	}

	public OrePiece GetFirstOrePrefab()
	{
		return _possibleDrops.FirstOrDefault()?.OrePrefab;
	}

	public OrePiece GetOrePrefab()
	{
		if (_possibleDrops == null || _possibleDrops.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (WeightedNodeDrop possibleDrop in _possibleDrops)
		{
			num += possibleDrop.Weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		foreach (WeightedNodeDrop possibleDrop2 in _possibleDrops)
		{
			num3 += possibleDrop2.Weight;
			if (num2 <= num3)
			{
				return possibleDrop2.OrePrefab;
			}
		}
		return _possibleDrops[_possibleDrops.Count - 1].OrePrefab;
	}

	public void UpdateSupportsAbove()
	{
		if (Physics.Raycast(base.transform.position, Vector3.up, out var hitInfo, 20f, Singleton<BuildingManager>.Instance.BuildingSupportsCollisionLayers))
		{
			ModularBuildingSupports componentInParent = hitInfo.collider.GetComponentInParent<ModularBuildingSupports>();
			if (componentInParent != null)
			{
				componentInParent.RespawnSupports(RespawnNextFrame: true);
			}
		}
	}

	public Vector3 GetPosition()
	{
		return MathExtensions.TruncateVector3(base.transform.position);
	}

	public void MarkStaticPositionAsBroken()
	{
		Singleton<SavingLoadingManager>.Instance.AddDestroyedStaticBreakablePosition(GetPosition());
	}

	public void DestroyFromLoading()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Object.Destroy(base.gameObject);
	}
}
