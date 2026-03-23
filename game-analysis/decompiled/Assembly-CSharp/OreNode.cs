using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000076 RID: 118
public class OreNode : MonoBehaviour, IDamageable, ISaveLoadableStaticBreakable
{
	// Token: 0x0600031C RID: 796 RVA: 0x0000FA58 File Offset: 0x0000DC58
	private void Start()
	{
		if (this._models.Length == 0)
		{
			return;
		}
		int num = Random.Range(0, this._models.Length);
		for (int i = 0; i < this._models.Length; i++)
		{
			this._models[i].SetActive(i == num);
		}
	}

	// Token: 0x0600031D RID: 797 RVA: 0x0000FAA4 File Offset: 0x0000DCA4
	public void TakeDamage(float damage, Vector3 position)
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._takeDamageSoundDefinition, position, 1f, 1f, true, false);
		this.Health -= damage;
		if (this.Health <= 0f)
		{
			this.BreakNode(position);
		}
	}

	// Token: 0x0600031E RID: 798 RVA: 0x0000FAF0 File Offset: 0x0000DCF0
	public void BreakNode(Vector3 position)
	{
		int num = Random.Range(this.MinDrops, this.MaxDrops + 1);
		Vector3 vector = (base.transform.position + position) * 0.5f;
		for (int i = 0; i < num; i++)
		{
			vector += Random.insideUnitSphere * 0.15f;
			Rigidbody component = Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(this.GetOrePrefab(), vector, Quaternion.identity, null).GetComponent<Rigidbody>();
			if (component != null)
			{
				component.linearVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(2f, 4f), Random.Range(-1.5f, 1.5f));
				component.angularVelocity = Random.insideUnitSphere * Random.Range(1f, 50f);
			}
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(Singleton<SoundManager>.Instance.Sound_Node_Break, base.transform.position, 1f, 1f, true, false);
		Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.BreakOreNodeParticlePrefab, position, default(Quaternion), default(Vector3));
		this.UpdateSupportsAbove();
		this.MarkStaticPositionAsBroken();
		Object.Destroy(base.gameObject);
	}

	// Token: 0x0600031F RID: 799 RVA: 0x0000FC3D File Offset: 0x0000DE3D
	public OrePiece GetFirstOrePrefab()
	{
		WeightedNodeDrop weightedNodeDrop = this._possibleDrops.FirstOrDefault<WeightedNodeDrop>();
		if (weightedNodeDrop == null)
		{
			return null;
		}
		return weightedNodeDrop.OrePrefab;
	}

	// Token: 0x06000320 RID: 800 RVA: 0x0000FC58 File Offset: 0x0000DE58
	public OrePiece GetOrePrefab()
	{
		if (this._possibleDrops == null || this._possibleDrops.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (WeightedNodeDrop weightedNodeDrop in this._possibleDrops)
		{
			num += weightedNodeDrop.Weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		foreach (WeightedNodeDrop weightedNodeDrop2 in this._possibleDrops)
		{
			num3 += weightedNodeDrop2.Weight;
			if (num2 <= num3)
			{
				return weightedNodeDrop2.OrePrefab;
			}
		}
		return this._possibleDrops[this._possibleDrops.Count - 1].OrePrefab;
	}

	// Token: 0x06000321 RID: 801 RVA: 0x0000FD50 File Offset: 0x0000DF50
	public void UpdateSupportsAbove()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, Vector3.up, out raycastHit, 20f, Singleton<BuildingManager>.Instance.BuildingSupportsCollisionLayers))
		{
			ModularBuildingSupports componentInParent = raycastHit.collider.GetComponentInParent<ModularBuildingSupports>();
			if (componentInParent != null)
			{
				componentInParent.RespawnSupports(true);
			}
		}
	}

	// Token: 0x06000322 RID: 802 RVA: 0x0000FDA7 File Offset: 0x0000DFA7
	public Vector3 GetPosition()
	{
		return MathExtensions.TruncateVector3(base.transform.position);
	}

	// Token: 0x06000323 RID: 803 RVA: 0x0000FDB9 File Offset: 0x0000DFB9
	public void MarkStaticPositionAsBroken()
	{
		Singleton<SavingLoadingManager>.Instance.AddDestroyedStaticBreakablePosition(this.GetPosition());
	}

	// Token: 0x06000324 RID: 804 RVA: 0x0000FDCC File Offset: 0x0000DFCC
	public void DestroyFromLoading()
	{
		Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x04000300 RID: 768
	public ResourceType ResourceType;

	// Token: 0x04000301 RID: 769
	public float Health;

	// Token: 0x04000302 RID: 770
	public int MinDrops;

	// Token: 0x04000303 RID: 771
	public int MaxDrops;

	// Token: 0x04000304 RID: 772
	[SerializeField]
	private List<WeightedNodeDrop> _possibleDrops = new List<WeightedNodeDrop>();

	// Token: 0x04000305 RID: 773
	[SerializeField]
	private GameObject[] _models;

	// Token: 0x04000306 RID: 774
	[SerializeField]
	private SoundDefinition _takeDamageSoundDefinition;
}
