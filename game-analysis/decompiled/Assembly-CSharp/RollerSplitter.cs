using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AC RID: 172
public class RollerSplitter : MonoBehaviour
{
	// Token: 0x060004C2 RID: 1218 RVA: 0x00019970 File Offset: 0x00017B70
	private void OnTriggerEnter(Collider other)
	{
		GameObject gameObject = other.gameObject;
		if ((in this._timeSinceLastCleared) > 3)
		{
			this._timeSinceLastCleared = 0f;
			this._recentObjects.Clear();
		}
		else if (this._recentObjects.Contains(gameObject))
		{
			return;
		}
		this._recentObjects.Enqueue(gameObject);
		if (this._recentObjects.Count > 10)
		{
			this._recentObjects.Dequeue();
		}
		this.SetCollisions(other.gameObject, this._nextGoStraight);
		this._nextGoStraight = !this._nextGoStraight;
	}

	// Token: 0x060004C3 RID: 1219 RVA: 0x00019A08 File Offset: 0x00017C08
	private void SetCollisions(GameObject obj, bool goStraight)
	{
		Collider[] components = obj.GetComponents<Collider>();
		if (goStraight)
		{
			PhysicsUtils.IgnoreAllCollisions(components, this.LeftConveyors, false);
			PhysicsUtils.IgnoreAllCollisions(components, this.RightConveyors, true);
			return;
		}
		PhysicsUtils.IgnoreAllCollisions(components, this.LeftConveyors, true);
		PhysicsUtils.IgnoreAllCollisions(components, this.RightConveyors, false);
	}

	// Token: 0x0400055B RID: 1371
	public Collider[] LeftConveyors;

	// Token: 0x0400055C RID: 1372
	public Collider[] RightConveyors;

	// Token: 0x0400055D RID: 1373
	private bool _nextGoStraight = true;

	// Token: 0x0400055E RID: 1374
	private readonly Queue<GameObject> _recentObjects = new Queue<GameObject>();

	// Token: 0x0400055F RID: 1375
	private TimeSince _timeSinceLastCleared;
}
