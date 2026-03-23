using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Token: 0x02000033 RID: 51
public class ConveyorBelt : MonoBehaviour
{
	// Token: 0x17000010 RID: 16
	// (get) Token: 0x0600016E RID: 366 RVA: 0x000082A4 File Offset: 0x000064A4
	// (set) Token: 0x0600016F RID: 367 RVA: 0x000082AB File Offset: 0x000064AB
	public static List<ConveyorBelt> AllConveyorBelts { get; private set; } = new List<ConveyorBelt>();

	// Token: 0x06000170 RID: 368 RVA: 0x000082B3 File Offset: 0x000064B3
	protected virtual void OnEnable()
	{
		this._pushVelocity = base.transform.forward * this.Speed;
		ConveyorBelt.AllConveyorBelts.Add(this);
	}

	// Token: 0x06000171 RID: 369 RVA: 0x000082DC File Offset: 0x000064DC
	public void ChangeSpeed(float newSpeed)
	{
		this.Speed = newSpeed;
		this._pushVelocity = base.transform.forward * this.Speed;
	}

	// Token: 0x06000172 RID: 370 RVA: 0x00008304 File Offset: 0x00006504
	protected virtual void FixedUpdate()
	{
		if (this.Disabled || this._physicsObjectsOnBelt.Count == 0)
		{
			return;
		}
		for (int i = this._physicsObjectsOnBelt.Count - 1; i >= 0; i--)
		{
			this._physicsObjectsOnBelt[i].AddConveyorVelocity(this._pushVelocity, this.RetainYVelocity);
		}
	}

	// Token: 0x06000173 RID: 371 RVA: 0x0000835C File Offset: 0x0000655C
	protected virtual void OnDisable()
	{
		foreach (BasePhysicsObject basePhysicsObject in this._physicsObjectsOnBelt)
		{
			if (basePhysicsObject != null)
			{
				basePhysicsObject.RemoveTouchingConveyorBelt(this);
			}
		}
		this._physicsObjectsOnBelt.Clear();
		ConveyorBelt.AllConveyorBelts.Remove(this);
	}

	// Token: 0x06000174 RID: 372 RVA: 0x000083D0 File Offset: 0x000065D0
	private void OnTriggerEnter(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (attachedRigidbody != null && !attachedRigidbody.isKinematic)
		{
			BasePhysicsObject basePhysicsObject = attachedRigidbody.GetComponent<BasePhysicsObject>();
			if (basePhysicsObject != null)
			{
				this.AddPhysicsObject(basePhysicsObject);
				return;
			}
			basePhysicsObject = attachedRigidbody.AddComponent<BasePhysicsObject>();
			if (basePhysicsObject != null)
			{
				this.AddPhysicsObject(basePhysicsObject);
			}
		}
	}

	// Token: 0x06000175 RID: 373 RVA: 0x00008424 File Offset: 0x00006624
	public void AddPhysicsObject(BasePhysicsObject obj)
	{
		this._physicsObjectsOnBelt.Add(obj);
		obj.AddTouchingConveyorBelt(this);
	}

	// Token: 0x06000176 RID: 374 RVA: 0x00008439 File Offset: 0x00006639
	public void RemovePhysicsObject(BasePhysicsObject obj)
	{
		this._physicsObjectsOnBelt.Remove(obj);
	}

	// Token: 0x06000177 RID: 375 RVA: 0x00008448 File Offset: 0x00006648
	public void ClearNullObjectsOnBelt()
	{
		for (int i = this._physicsObjectsOnBelt.Count - 1; i >= 0; i--)
		{
			BasePhysicsObject basePhysicsObject = this._physicsObjectsOnBelt[i];
			if (basePhysicsObject == null || !basePhysicsObject.isActiveAndEnabled || basePhysicsObject.Rb.isKinematic)
			{
				this._physicsObjectsOnBelt.RemoveAt(i);
			}
		}
	}

	// Token: 0x06000178 RID: 376 RVA: 0x000084A4 File Offset: 0x000066A4
	private void OnTriggerExit(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			BasePhysicsObject component = attachedRigidbody.GetComponent<BasePhysicsObject>();
			if (component != null)
			{
				this._physicsObjectsOnBelt.Remove(component);
				component.RemoveTouchingConveyorBelt(this);
			}
		}
	}

	// Token: 0x0400015C RID: 348
	public float Speed = 0.8f;

	// Token: 0x0400015D RID: 349
	public bool Disabled;

	// Token: 0x0400015E RID: 350
	public bool RetainYVelocity;

	// Token: 0x0400015F RID: 351
	protected List<BasePhysicsObject> _physicsObjectsOnBelt = new List<BasePhysicsObject>();

	// Token: 0x04000160 RID: 352
	protected Vector3 _pushVelocity;
}
