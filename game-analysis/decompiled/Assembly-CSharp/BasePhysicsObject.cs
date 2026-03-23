using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class BasePhysicsObject : MonoBehaviour
{
	// Token: 0x17000009 RID: 9
	// (get) Token: 0x06000057 RID: 87 RVA: 0x000031EC File Offset: 0x000013EC
	// (set) Token: 0x06000058 RID: 88 RVA: 0x000031F4 File Offset: 0x000013F4
	public Rigidbody Rb { get; private set; }

	// Token: 0x06000059 RID: 89 RVA: 0x000031FD File Offset: 0x000013FD
	protected virtual void Awake()
	{
		this.Rb = base.GetComponent<Rigidbody>();
		if (this.Rb != null)
		{
			this.Rb.linearDamping = 0.2f;
			this.Rb.angularDamping = 0.05f;
		}
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00003239 File Offset: 0x00001439
	protected virtual void OnEnable()
	{
		ConveyorBeltManager.Register(this);
	}

	// Token: 0x0600005B RID: 91 RVA: 0x00003241 File Offset: 0x00001441
	protected virtual void OnDisable()
	{
		ConveyorBeltManager.Unregister(this);
	}

	// Token: 0x0600005C RID: 92 RVA: 0x0000324C File Offset: 0x0000144C
	public void ClearTouchingConveyorBelts()
	{
		foreach (ConveyorBelt conveyorBelt in this._conveyorBeltsTouchingThis)
		{
			conveyorBelt.RemovePhysicsObject(this);
		}
		this._conveyorBeltsTouchingThis.Clear();
	}

	// Token: 0x0600005D RID: 93 RVA: 0x000032A8 File Offset: 0x000014A8
	public void AddTouchingConveyorBelt(ConveyorBelt belt)
	{
		this._conveyorBeltsTouchingThis.Add(belt);
	}

	// Token: 0x0600005E RID: 94 RVA: 0x000032B6 File Offset: 0x000014B6
	public bool IsOnAnyConveyor()
	{
		return this._conveyorBeltsTouchingThis.Count > 0;
	}

	// Token: 0x0600005F RID: 95 RVA: 0x000032C6 File Offset: 0x000014C6
	public void RemoveTouchingConveyorBelt(ConveyorBelt belt)
	{
		this._conveyorBeltsTouchingThis.Remove(belt);
	}

	// Token: 0x06000060 RID: 96 RVA: 0x000032D8 File Offset: 0x000014D8
	public void AddConveyorVelocity(Vector3 velocity, bool retainY)
	{
		if (this.Count == 0)
		{
			this.SumVelocity = velocity;
		}
		else
		{
			this.SumVelocity += velocity;
		}
		if (velocity.y > this.BestY)
		{
			this.BestY = velocity.y;
		}
		this.Count++;
		if (retainY)
		{
			this.RetainY = true;
		}
	}

	// Token: 0x06000061 RID: 97 RVA: 0x0000333A File Offset: 0x0000153A
	public void ResetAccum()
	{
		this.SumVelocity = default(Vector3);
		this.BestY = 0f;
		this.Count = 0;
		this.RetainY = false;
	}

	// Token: 0x0400004D RID: 77
	public const float STANDARD_LINEAR_DAMPING = 0.2f;

	// Token: 0x0400004E RID: 78
	public const float STANDARD_ANGULAR_DAMPING = 0.05f;

	// Token: 0x04000050 RID: 80
	[HideInInspector]
	public Vector3 SumVelocity;

	// Token: 0x04000051 RID: 81
	[HideInInspector]
	public float BestY;

	// Token: 0x04000052 RID: 82
	[HideInInspector]
	public int Count;

	// Token: 0x04000053 RID: 83
	[HideInInspector]
	public bool RetainY;

	// Token: 0x04000054 RID: 84
	private List<ConveyorBelt> _conveyorBeltsTouchingThis = new List<ConveyorBelt>();
}
