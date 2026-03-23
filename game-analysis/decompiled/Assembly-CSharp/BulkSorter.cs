using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x0200001C RID: 28
public class BulkSorter : MonoBehaviour
{
	// Token: 0x060000E8 RID: 232 RVA: 0x00005BF4 File Offset: 0x00003DF4
	private void Awake()
	{
		this._leftCols = this.LeftCollider.GetComponentsInChildren<Collider>();
		this._rightCols = this.RightCollider.GetComponentsInChildren<Collider>();
		this._centerCols = this.CenterColliders.GetComponentsInChildren<Collider>();
	}

	// Token: 0x060000E9 RID: 233 RVA: 0x00005C29 File Offset: 0x00003E29
	private void OnEnable()
	{
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces += this.OnClearedAllPhysicsOrePieces;
		base.StartCoroutine(this.WaitThenReSortOre());
	}

	// Token: 0x060000EA RID: 234 RVA: 0x00005C4E File Offset: 0x00003E4E
	private void OnDisable()
	{
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces -= this.OnClearedAllPhysicsOrePieces;
	}

	// Token: 0x060000EB RID: 235 RVA: 0x00005C68 File Offset: 0x00003E68
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this.SortOre(component);
			return;
		}
		this.SetCollisions(other.gameObject, BulkSorter.SortDirection.Straight);
	}

	// Token: 0x060000EC RID: 236 RVA: 0x00005C9C File Offset: 0x00003E9C
	private void SortOre(OrePiece ore)
	{
		if (this._directionByOre.ContainsKey(ore))
		{
			return;
		}
		bool flag = this.FilterLeft.OreMatchesFilter(ore);
		bool flag2 = this.FilterRight.OreMatchesFilter(ore);
		if (flag && flag2)
		{
			BulkSorter.SortDirection sortDirection = (BulkSorter.SortDirection)Random.Range(1, 3);
			this.SetOreCollisions(ore, sortDirection);
			return;
		}
		if (flag)
		{
			this.SetOreCollisions(ore, BulkSorter.SortDirection.Left);
			return;
		}
		if (flag2)
		{
			this.SetOreCollisions(ore, BulkSorter.SortDirection.Right);
			return;
		}
		this.SetOreCollisions(ore, BulkSorter.SortDirection.Straight);
	}

	// Token: 0x060000ED RID: 237 RVA: 0x00005D08 File Offset: 0x00003F08
	private void OnClearedAllPhysicsOrePieces()
	{
		this.ResetAllOreIgnores();
	}

	// Token: 0x060000EE RID: 238 RVA: 0x00005D10 File Offset: 0x00003F10
	private void ResetAllOreIgnores()
	{
		foreach (OrePiece orePiece in new List<OrePiece>(this._ignoredWith.Keys))
		{
			HashSet<OrePiece> hashSet;
			if (!(orePiece == null) && this._ignoredWith.TryGetValue(orePiece, out hashSet))
			{
				foreach (OrePiece orePiece2 in new List<OrePiece>(hashSet))
				{
					if (!(orePiece2 == null) && this._collidersByOre.ContainsKey(orePiece) && this._collidersByOre.ContainsKey(orePiece2))
					{
						this.IgnoreOrePair(orePiece, orePiece2, false);
					}
				}
			}
		}
		this._ignoredWith.Clear();
		this._directionByOre.Clear();
		this._collidersByOre.Clear();
	}

	// Token: 0x060000EF RID: 239 RVA: 0x00005E14 File Offset: 0x00004014
	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component == null)
		{
			return;
		}
		HashSet<OrePiece> hashSet;
		if (this._ignoredWith.TryGetValue(component, out hashSet))
		{
			foreach (OrePiece orePiece in new List<OrePiece>(hashSet))
			{
				this.IgnoreOrePair(component, orePiece, false);
			}
		}
		this._ignoredWith.Remove(component);
		this._directionByOre.Remove(component);
		this._collidersByOre.Remove(component);
	}

	// Token: 0x060000F0 RID: 240 RVA: 0x00005EB4 File Offset: 0x000040B4
	private void SetOreCollisions(OrePiece ore, BulkSorter.SortDirection direction)
	{
		this._directionByOre[ore] = direction;
		if (!this._ignoredWith.ContainsKey(ore))
		{
			this._ignoredWith[ore] = new HashSet<OrePiece>();
		}
		if (!this._collidersByOre.ContainsKey(ore))
		{
			this._collidersByOre[ore] = ore.GetComponentsInChildren<Collider>();
		}
		Collider[] array = this._collidersByOre[ore];
		foreach (OrePiece orePiece in this._directionByOre.Keys)
		{
			if (!(orePiece == ore) && this._directionByOre[orePiece] != direction)
			{
				this.IgnoreOrePair(ore, orePiece, true);
			}
		}
		this.SetCollisions(ore.gameObject, direction);
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x00005F8C File Offset: 0x0000418C
	private void SetCollisions(GameObject obj, BulkSorter.SortDirection direction)
	{
		Collider[] components = obj.GetComponents<Collider>();
		if (direction == BulkSorter.SortDirection.Straight)
		{
			PhysicsUtils.IgnoreAllCollisions(components, this._centerCols, false);
			PhysicsUtils.IgnoreAllCollisions(components, this._leftCols, true);
			PhysicsUtils.IgnoreAllCollisions(components, this._rightCols, true);
			return;
		}
		if (direction == BulkSorter.SortDirection.Left)
		{
			PhysicsUtils.IgnoreAllCollisions(components, this._centerCols, true);
			PhysicsUtils.IgnoreAllCollisions(components, this._leftCols, false);
			PhysicsUtils.IgnoreAllCollisions(components, this._rightCols, true);
			return;
		}
		if (direction == BulkSorter.SortDirection.Right)
		{
			PhysicsUtils.IgnoreAllCollisions(components, this._centerCols, true);
			PhysicsUtils.IgnoreAllCollisions(components, this._leftCols, true);
			PhysicsUtils.IgnoreAllCollisions(components, this._rightCols, false);
		}
	}

	// Token: 0x060000F2 RID: 242 RVA: 0x00006024 File Offset: 0x00004224
	private void IgnoreOrePair(OrePiece a, OrePiece b, bool ignore)
	{
		if (!this._collidersByOre.ContainsKey(b))
		{
			this._collidersByOre[b] = b.GetComponentsInChildren<Collider>();
		}
		Collider[] array = this._collidersByOre[a];
		Collider[] array2 = this._collidersByOre[b];
		foreach (Collider collider in array)
		{
			foreach (Collider collider2 in array2)
			{
				Physics.IgnoreCollision(collider, collider2, ignore);
			}
		}
		if (ignore)
		{
			this._ignoredWith[a].Add(b);
			if (!this._ignoredWith.ContainsKey(b))
			{
				this._ignoredWith[b] = new HashSet<OrePiece>();
			}
			this._ignoredWith[b].Add(a);
			return;
		}
		this._ignoredWith[a].Remove(b);
		this._ignoredWith[b].Remove(a);
	}

	// Token: 0x060000F3 RID: 243 RVA: 0x00006112 File Offset: 0x00004312
	private IEnumerator WaitThenReSortOre()
	{
		yield return new WaitForFixedUpdate();
		if (base.gameObject == null)
		{
			yield break;
		}
		if (this._directionByOre.Count == 0)
		{
			yield break;
		}
		List<OrePiece> list = this._directionByOre.Keys.ToList<OrePiece>();
		this._directionByOre.Clear();
		using (List<OrePiece>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				OrePiece orePiece = enumerator.Current;
				this.SortOre(orePiece);
			}
			yield break;
		}
		yield break;
	}

	// Token: 0x040000D6 RID: 214
	public GameObject LeftCollider;

	// Token: 0x040000D7 RID: 215
	public GameObject RightCollider;

	// Token: 0x040000D8 RID: 216
	public GameObject CenterColliders;

	// Token: 0x040000D9 RID: 217
	public SorterFilterBasket FilterLeft;

	// Token: 0x040000DA RID: 218
	public SorterFilterBasket FilterRight;

	// Token: 0x040000DB RID: 219
	private Collider[] _leftCols;

	// Token: 0x040000DC RID: 220
	private Collider[] _rightCols;

	// Token: 0x040000DD RID: 221
	private Collider[] _centerCols;

	// Token: 0x040000DE RID: 222
	private readonly Dictionary<OrePiece, BulkSorter.SortDirection> _directionByOre = new Dictionary<OrePiece, BulkSorter.SortDirection>();

	// Token: 0x040000DF RID: 223
	private readonly Dictionary<OrePiece, HashSet<OrePiece>> _ignoredWith = new Dictionary<OrePiece, HashSet<OrePiece>>();

	// Token: 0x040000E0 RID: 224
	private readonly Dictionary<OrePiece, Collider[]> _collidersByOre = new Dictionary<OrePiece, Collider[]>();

	// Token: 0x0200011A RID: 282
	private enum SortDirection
	{
		// Token: 0x0400084C RID: 2124
		Straight,
		// Token: 0x0400084D RID: 2125
		Left,
		// Token: 0x0400084E RID: 2126
		Right
	}
}
