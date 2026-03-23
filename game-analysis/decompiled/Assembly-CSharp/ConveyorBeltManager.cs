using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000034 RID: 52
[DefaultExecutionOrder(-10)]
public class ConveyorBeltManager : Singleton<ConveyorBeltManager>
{
	// Token: 0x0600017B RID: 379 RVA: 0x0000850F File Offset: 0x0000670F
	public static void Register(BasePhysicsObject obj)
	{
		if (obj != null && !ConveyorBeltManager.Objects.Contains(obj))
		{
			ConveyorBeltManager.Objects.Add(obj);
		}
	}

	// Token: 0x0600017C RID: 380 RVA: 0x00008532 File Offset: 0x00006732
	public static void Unregister(BasePhysicsObject obj)
	{
		if (obj != null)
		{
			ConveyorBeltManager.Objects.Remove(obj);
		}
	}

	// Token: 0x0600017D RID: 381 RVA: 0x0000854C File Offset: 0x0000674C
	private void Update()
	{
		List<ConveyorBelt> allConveyorBelts = ConveyorBelt.AllConveyorBelts;
		if (allConveyorBelts.Count == 0)
		{
			this._currentBeltIndex = 0;
			return;
		}
		if (this._currentBeltIndex >= allConveyorBelts.Count)
		{
			this._currentBeltIndex = 0;
		}
		ConveyorBelt conveyorBelt = allConveyorBelts[this._currentBeltIndex];
		if (conveyorBelt == null)
		{
			allConveyorBelts.RemoveAt(this._currentBeltIndex);
		}
		else
		{
			conveyorBelt.ClearNullObjectsOnBelt();
		}
		this._currentBeltIndex++;
	}

	// Token: 0x0600017E RID: 382 RVA: 0x000085BC File Offset: 0x000067BC
	private void FixedUpdate()
	{
		for (int i = 0; i < ConveyorBeltManager.Objects.Count; i++)
		{
			BasePhysicsObject basePhysicsObject = ConveyorBeltManager.Objects[i];
			if (basePhysicsObject.Count == 0)
			{
				basePhysicsObject.ResetAccum();
			}
			else
			{
				Rigidbody rb = basePhysicsObject.Rb;
				Vector3 vector = basePhysicsObject.SumVelocity / (float)basePhysicsObject.Count;
				if (basePhysicsObject.RetainY)
				{
					vector.y = rb.linearVelocity.y;
				}
				else if (basePhysicsObject.BestY > 0f)
				{
					vector.y = basePhysicsObject.BestY;
				}
				rb.linearVelocity = vector;
				basePhysicsObject.ResetAccum();
			}
		}
	}

	// Token: 0x04000162 RID: 354
	private static readonly List<BasePhysicsObject> Objects = new List<BasePhysicsObject>();

	// Token: 0x04000163 RID: 355
	private int _currentBeltIndex;
}
