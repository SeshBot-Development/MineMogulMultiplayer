using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulkSorter : MonoBehaviour
{
	private enum SortDirection
	{
		Straight,
		Left,
		Right
	}

	public GameObject LeftCollider;

	public GameObject RightCollider;

	public GameObject CenterColliders;

	public SorterFilterBasket FilterLeft;

	public SorterFilterBasket FilterRight;

	private Collider[] _leftCols;

	private Collider[] _rightCols;

	private Collider[] _centerCols;

	private readonly Dictionary<OrePiece, SortDirection> _directionByOre = new Dictionary<OrePiece, SortDirection>();

	private readonly Dictionary<OrePiece, HashSet<OrePiece>> _ignoredWith = new Dictionary<OrePiece, HashSet<OrePiece>>();

	private readonly Dictionary<OrePiece, Collider[]> _collidersByOre = new Dictionary<OrePiece, Collider[]>();

	private void Awake()
	{
		_leftCols = LeftCollider.GetComponentsInChildren<Collider>();
		_rightCols = RightCollider.GetComponentsInChildren<Collider>();
		_centerCols = CenterColliders.GetComponentsInChildren<Collider>();
	}

	private void OnEnable()
	{
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces += OnClearedAllPhysicsOrePieces;
		StartCoroutine(WaitThenReSortOre());
	}

	private void OnDisable()
	{
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces -= OnClearedAllPhysicsOrePieces;
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			SortOre(component);
		}
		else
		{
			SetCollisions(other.gameObject, SortDirection.Straight);
		}
	}

	private void SortOre(OrePiece ore)
	{
		if (!_directionByOre.ContainsKey(ore))
		{
			bool flag = FilterLeft.OreMatchesFilter(ore);
			bool flag2 = FilterRight.OreMatchesFilter(ore);
			if (flag && flag2)
			{
				SortDirection direction = (SortDirection)Random.Range(1, 3);
				SetOreCollisions(ore, direction);
			}
			else if (flag)
			{
				SetOreCollisions(ore, SortDirection.Left);
			}
			else if (flag2)
			{
				SetOreCollisions(ore, SortDirection.Right);
			}
			else
			{
				SetOreCollisions(ore, SortDirection.Straight);
			}
		}
	}

	private void OnClearedAllPhysicsOrePieces()
	{
		ResetAllOreIgnores();
	}

	private void ResetAllOreIgnores()
	{
		foreach (OrePiece item in new List<OrePiece>(_ignoredWith.Keys))
		{
			if (item == null || !_ignoredWith.TryGetValue(item, out var value))
			{
				continue;
			}
			foreach (OrePiece item2 in new List<OrePiece>(value))
			{
				if (!(item2 == null) && _collidersByOre.ContainsKey(item) && _collidersByOre.ContainsKey(item2))
				{
					IgnoreOrePair(item, item2, ignore: false);
				}
			}
		}
		_ignoredWith.Clear();
		_directionByOre.Clear();
		_collidersByOre.Clear();
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component == null)
		{
			return;
		}
		if (_ignoredWith.TryGetValue(component, out var value))
		{
			foreach (OrePiece item in new List<OrePiece>(value))
			{
				IgnoreOrePair(component, item, ignore: false);
			}
		}
		_ignoredWith.Remove(component);
		_directionByOre.Remove(component);
		_collidersByOre.Remove(component);
	}

	private void SetOreCollisions(OrePiece ore, SortDirection direction)
	{
		_directionByOre[ore] = direction;
		if (!_ignoredWith.ContainsKey(ore))
		{
			_ignoredWith[ore] = new HashSet<OrePiece>();
		}
		if (!_collidersByOre.ContainsKey(ore))
		{
			_collidersByOre[ore] = ore.GetComponentsInChildren<Collider>();
		}
		_ = _collidersByOre[ore];
		foreach (OrePiece key in _directionByOre.Keys)
		{
			if (!(key == ore) && _directionByOre[key] != direction)
			{
				IgnoreOrePair(ore, key, ignore: true);
			}
		}
		SetCollisions(ore.gameObject, direction);
	}

	private void SetCollisions(GameObject obj, SortDirection direction)
	{
		Collider[] components = obj.GetComponents<Collider>();
		switch (direction)
		{
		case SortDirection.Straight:
			PhysicsUtils.IgnoreAllCollisions(components, _centerCols, ignore: false);
			PhysicsUtils.IgnoreAllCollisions(components, _leftCols, ignore: true);
			PhysicsUtils.IgnoreAllCollisions(components, _rightCols, ignore: true);
			break;
		case SortDirection.Left:
			PhysicsUtils.IgnoreAllCollisions(components, _centerCols, ignore: true);
			PhysicsUtils.IgnoreAllCollisions(components, _leftCols, ignore: false);
			PhysicsUtils.IgnoreAllCollisions(components, _rightCols, ignore: true);
			break;
		case SortDirection.Right:
			PhysicsUtils.IgnoreAllCollisions(components, _centerCols, ignore: true);
			PhysicsUtils.IgnoreAllCollisions(components, _leftCols, ignore: true);
			PhysicsUtils.IgnoreAllCollisions(components, _rightCols, ignore: false);
			break;
		}
	}

	private void IgnoreOrePair(OrePiece a, OrePiece b, bool ignore)
	{
		if (!_collidersByOre.ContainsKey(b))
		{
			_collidersByOre[b] = b.GetComponentsInChildren<Collider>();
		}
		Collider[] array = _collidersByOre[a];
		Collider[] array2 = _collidersByOre[b];
		Collider[] array3 = array;
		foreach (Collider collider in array3)
		{
			Collider[] array4 = array2;
			foreach (Collider collider2 in array4)
			{
				Physics.IgnoreCollision(collider, collider2, ignore);
			}
		}
		if (ignore)
		{
			_ignoredWith[a].Add(b);
			if (!_ignoredWith.ContainsKey(b))
			{
				_ignoredWith[b] = new HashSet<OrePiece>();
			}
			_ignoredWith[b].Add(a);
		}
		else
		{
			_ignoredWith[a].Remove(b);
			_ignoredWith[b].Remove(a);
		}
	}

	private IEnumerator WaitThenReSortOre()
	{
		yield return new WaitForFixedUpdate();
		if (base.gameObject == null || _directionByOre.Count == 0)
		{
			yield break;
		}
		List<OrePiece> list = _directionByOre.Keys.ToList();
		_directionByOre.Clear();
		foreach (OrePiece item in list)
		{
			SortOre(item);
		}
	}
}
