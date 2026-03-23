using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007B RID: 123
public class OrePiecePoolManager : Singleton<OrePiecePoolManager>
{
	// Token: 0x0600033C RID: 828 RVA: 0x00010948 File Offset: 0x0000EB48
	protected override void Awake()
	{
		base.Awake();
		this._root = new GameObject("[OrePiecePools]").transform;
		this._root.SetParent(base.transform);
		this._prefabByKey.Clear();
		List<OrePiece> allOrePiecePrefabs = Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs;
		for (int i = 0; i < allOrePiecePrefabs.Count; i++)
		{
			OrePiece orePiece = allOrePiecePrefabs[i];
			if (!(orePiece == null))
			{
				OrePiece orePiece2 = orePiece;
				OrePiecePoolManager.OreKey oreKey = OrePiecePoolManager.OreKey.From(orePiece2.ResourceType, orePiece2.PieceType, orePiece2.IsPolished);
				if (this._prefabByKey.ContainsKey(oreKey))
				{
					Debug.LogWarning(string.Format("Duplicate OrePiece prefab key found: {0}. Keeping the first one.", oreKey));
				}
				else
				{
					this._prefabByKey.Add(oreKey, orePiece2);
				}
			}
		}
	}

	// Token: 0x0600033D RID: 829 RVA: 0x00010A0C File Offset: 0x0000EC0C
	public OrePiece SpawnPooledOre(ResourceType resourceType, PieceType pieceType, bool isPolished, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
	{
		OrePiecePoolManager.OreKey oreKey = OrePiecePoolManager.OreKey.From(resourceType, pieceType, isPolished);
		OrePiece orePiece;
		if (!this._prefabByKey.TryGetValue(oreKey, out orePiece))
		{
			Debug.LogError(string.Format("No OrePiece prefab registered for key: {0}. Add it to OrePiecePoolManager.prefabs.", oreKey));
			return null;
		}
		Queue<OrePiece> queue;
		if (!this._pools.TryGetValue(oreKey, out queue))
		{
			queue = new Queue<OrePiece>();
			this._pools.Add(oreKey, queue);
		}
		OrePiece orePiece2;
		if (queue.Count > 0)
		{
			orePiece2 = queue.Dequeue();
		}
		else
		{
			orePiece2 = Object.Instantiate<OrePiece>(orePiece, this._root);
			orePiece2.gameObject.name = orePiece.gameObject.name + " [Pooled]";
		}
		Transform transform = orePiece2.transform;
		transform.SetParent(parent, false);
		transform.SetPositionAndRotation(position, rotation);
		orePiece2.gameObject.SetActive(true);
		return orePiece2;
	}

	// Token: 0x0600033E RID: 830 RVA: 0x00010AD0 File Offset: 0x0000ECD0
	public OrePiece SpawnPooledOre(OrePiece prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
	{
		return this.SpawnPooledOre(prefab.ResourceType, prefab.PieceType, prefab.IsPolished, position, rotation, parent);
	}

	// Token: 0x0600033F RID: 831 RVA: 0x00010AF0 File Offset: 0x0000ECF0
	public OrePiece TrySpawnPooledOre(GameObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
	{
		OrePiece component = prefab.GetComponent<OrePiece>();
		if (component != null)
		{
			return this.SpawnPooledOre(component, position, rotation, parent);
		}
		Object.Instantiate<GameObject>(prefab, position, rotation, parent);
		return null;
	}

	// Token: 0x06000340 RID: 832 RVA: 0x00010B20 File Offset: 0x0000ED20
	public void ReturnToPool(OrePiece piece)
	{
		if (piece == null)
		{
			return;
		}
		if (!piece.gameObject.activeSelf)
		{
			return;
		}
		OrePiecePoolManager.OreKey oreKey = OrePiecePoolManager.OreKey.From(piece.ResourceType, piece.PieceType, piece.IsPolished);
		Queue<OrePiece> queue;
		if (!this._pools.TryGetValue(oreKey, out queue))
		{
			queue = new Queue<OrePiece>();
			this._pools.Add(oreKey, queue);
		}
		piece.gameObject.SetActive(false);
		piece.Rb.linearVelocity = Vector3.zero;
		piece.Rb.angularVelocity = Vector3.zero;
		piece.Rb.Sleep();
		piece.Rb.linearDamping = 0.2f;
		piece.Rb.angularDamping = 0.05f;
		piece.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		piece.BasketsThisIsInside.Clear();
		piece.SievePercent = 0f;
		piece.CurrentMagnetTool = null;
		piece.PolishedPercent = (piece.IsPolished ? 1f : 0f);
		piece.ClearTouchingConveyorBelts();
		piece.gameObject.tag = "Grabbable";
		piece.transform.SetParent(this._root, false);
		queue.Enqueue(piece);
	}

	// Token: 0x06000341 RID: 833 RVA: 0x00010C54 File Offset: 0x0000EE54
	public int GetInactiveCount()
	{
		int num = 0;
		foreach (Queue<OrePiece> queue in this._pools.Values)
		{
			num += queue.Count;
		}
		return num;
	}

	// Token: 0x04000349 RID: 841
	private readonly Dictionary<OrePiecePoolManager.OreKey, Queue<OrePiece>> _pools = new Dictionary<OrePiecePoolManager.OreKey, Queue<OrePiece>>();

	// Token: 0x0400034A RID: 842
	private readonly Dictionary<OrePiecePoolManager.OreKey, OrePiece> _prefabByKey = new Dictionary<OrePiecePoolManager.OreKey, OrePiece>();

	// Token: 0x0400034B RID: 843
	private Transform _root;

	// Token: 0x0200014A RID: 330
	private readonly struct OreKey : IEquatable<OrePiecePoolManager.OreKey>
	{
		// Token: 0x0600083F RID: 2111 RVA: 0x000271D8 File Offset: 0x000253D8
		private OreKey(int resourceType, int pieceType, int polished)
		{
			this._resourceType = resourceType;
			this._pieceType = pieceType;
			this._polished = polished;
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x000271F0 File Offset: 0x000253F0
		public static OrePiecePoolManager.OreKey From(ResourceType r, PieceType p, bool isPolished)
		{
			int num = (isPolished ? 1 : 0);
			return new OrePiecePoolManager.OreKey((int)r, (int)p, num);
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0002720D File Offset: 0x0002540D
		public bool Equals(OrePiecePoolManager.OreKey other)
		{
			return this._resourceType == other._resourceType && this._pieceType == other._pieceType && this._polished == other._polished;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0002723C File Offset: 0x0002543C
		public override bool Equals(object obj)
		{
			if (obj is OrePiecePoolManager.OreKey)
			{
				OrePiecePoolManager.OreKey oreKey = (OrePiecePoolManager.OreKey)obj;
				return this.Equals(oreKey);
			}
			return false;
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x00027261 File Offset: 0x00025461
		public override int GetHashCode()
		{
			return ((17 * 31 + this._resourceType) * 31 + this._pieceType) * 31 + this._polished;
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x00027283 File Offset: 0x00025483
		public override string ToString()
		{
			return string.Format("ResourceType={0}, PieceType={1}, Polished={2}", (ResourceType)this._resourceType, (PieceType)this._pieceType, this._polished);
		}

		// Token: 0x040008E1 RID: 2273
		private readonly int _resourceType;

		// Token: 0x040008E2 RID: 2274
		private readonly int _pieceType;

		// Token: 0x040008E3 RID: 2275
		private readonly int _polished;
	}
}
