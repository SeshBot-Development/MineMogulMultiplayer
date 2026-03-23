using System;
using System.Collections.Generic;
using UnityEngine;

public class OrePiecePoolManager : Singleton<OrePiecePoolManager>
{
	private readonly struct OreKey : IEquatable<OreKey>
	{
		private readonly int _resourceType;

		private readonly int _pieceType;

		private readonly int _polished;

		private OreKey(int resourceType, int pieceType, int polished)
		{
			_resourceType = resourceType;
			_pieceType = pieceType;
			_polished = polished;
		}

		public static OreKey From(ResourceType r, PieceType p, bool isPolished)
		{
			int polished = (isPolished ? 1 : 0);
			return new OreKey((int)r, (int)p, polished);
		}

		public bool Equals(OreKey other)
		{
			if (_resourceType == other._resourceType && _pieceType == other._pieceType)
			{
				return _polished == other._polished;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is OreKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((17 * 31 + _resourceType) * 31 + _pieceType) * 31 + _polished;
		}

		public override string ToString()
		{
			return $"ResourceType={(ResourceType)_resourceType}, PieceType={(PieceType)_pieceType}, Polished={_polished}";
		}
	}

	private readonly Dictionary<OreKey, Queue<OrePiece>> _pools = new Dictionary<OreKey, Queue<OrePiece>>();

	private readonly Dictionary<OreKey, OrePiece> _prefabByKey = new Dictionary<OreKey, OrePiece>();

	private Transform _root;

	protected override void Awake()
	{
		base.Awake();
		_root = new GameObject("[OrePiecePools]").transform;
		_root.SetParent(base.transform);
		_prefabByKey.Clear();
		List<OrePiece> allOrePiecePrefabs = Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs;
		for (int i = 0; i < allOrePiecePrefabs.Count; i++)
		{
			OrePiece orePiece = allOrePiecePrefabs[i];
			if (!(orePiece == null))
			{
				OrePiece orePiece2 = orePiece;
				OreKey oreKey = OreKey.From(orePiece2.ResourceType, orePiece2.PieceType, orePiece2.IsPolished);
				if (_prefabByKey.ContainsKey(oreKey))
				{
					Debug.LogWarning($"Duplicate OrePiece prefab key found: {oreKey}. Keeping the first one.");
				}
				else
				{
					_prefabByKey.Add(oreKey, orePiece2);
				}
			}
		}
	}

	public OrePiece SpawnPooledOre(ResourceType resourceType, PieceType pieceType, bool isPolished, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
	{
		OreKey oreKey = OreKey.From(resourceType, pieceType, isPolished);
		if (!_prefabByKey.TryGetValue(oreKey, out var value))
		{
			Debug.LogError($"No OrePiece prefab registered for key: {oreKey}. Add it to OrePiecePoolManager.prefabs.");
			return null;
		}
		if (!_pools.TryGetValue(oreKey, out var value2))
		{
			value2 = new Queue<OrePiece>();
			_pools.Add(oreKey, value2);
		}
		OrePiece orePiece;
		if (value2.Count > 0)
		{
			orePiece = value2.Dequeue();
		}
		else
		{
			orePiece = UnityEngine.Object.Instantiate(value, _root);
			orePiece.gameObject.name = value.gameObject.name + " [Pooled]";
		}
		Transform obj = orePiece.transform;
		obj.SetParent(parent, worldPositionStays: false);
		obj.SetPositionAndRotation(position, rotation);
		orePiece.gameObject.SetActive(value: true);
		return orePiece;
	}

	public OrePiece SpawnPooledOre(OrePiece prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
	{
		return SpawnPooledOre(prefab.ResourceType, prefab.PieceType, prefab.IsPolished, position, rotation, parent);
	}

	public OrePiece TrySpawnPooledOre(GameObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
	{
		OrePiece component = prefab.GetComponent<OrePiece>();
		if ((object)component != null)
		{
			return SpawnPooledOre(component, position, rotation, parent);
		}
		UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
		return null;
	}

	public void ReturnToPool(OrePiece piece)
	{
		if (!(piece == null) && piece.gameObject.activeSelf)
		{
			OreKey key = OreKey.From(piece.ResourceType, piece.PieceType, piece.IsPolished);
			if (!_pools.TryGetValue(key, out var value))
			{
				value = new Queue<OrePiece>();
				_pools.Add(key, value);
			}
			piece.gameObject.SetActive(value: false);
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
			piece.transform.SetParent(_root, worldPositionStays: false);
			value.Enqueue(piece);
		}
	}

	public int GetInactiveCount()
	{
		int num = 0;
		foreach (Queue<OrePiece> value in _pools.Values)
		{
			num += value.Count;
		}
		return num;
	}
}
