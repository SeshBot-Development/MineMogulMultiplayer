using System;
using UnityEngine;

// Token: 0x02000036 RID: 54
public class ConveyorBeltShakerHorizontal : ConveyorBelt
{
	// Token: 0x06000184 RID: 388 RVA: 0x000087A2 File Offset: 0x000069A2
	protected override void OnEnable()
	{
		base.OnEnable();
		this._rightDirection = base.transform.right;
	}

	// Token: 0x06000185 RID: 389 RVA: 0x000087BC File Offset: 0x000069BC
	protected override void FixedUpdate()
	{
		if (this.Disabled || this._physicsObjectsOnBelt.Count == 0)
		{
			return;
		}
		float num = Mathf.Sign(Mathf.Sin(Time.fixedTime * 3.1415927f * 2f * this.ShakeFrequency));
		Vector3 vector = this._rightDirection * (this.ShakeSpeed * num);
		for (int i = this._physicsObjectsOnBelt.Count - 1; i >= 0; i--)
		{
			BasePhysicsObject basePhysicsObject = this._physicsObjectsOnBelt[i];
			Vector3 vector2 = this._pushVelocity + vector;
			basePhysicsObject.AddConveyorVelocity(vector2, this.RetainYVelocity);
		}
	}

	// Token: 0x0400016A RID: 362
	[Header("Shaker Settings")]
	[Tooltip("Sideways speed magnitude applied on top of the normal conveyor speed.")]
	public float ShakeSpeed = 2f;

	// Token: 0x0400016B RID: 363
	[Tooltip("How many times per second the direction flips left/right.")]
	public float ShakeFrequency = 2f;

	// Token: 0x0400016C RID: 364
	protected Vector3 _rightDirection;
}
