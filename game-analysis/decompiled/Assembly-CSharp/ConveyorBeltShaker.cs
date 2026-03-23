using System;
using UnityEngine;

// Token: 0x02000035 RID: 53
public class ConveyorBeltShaker : ConveyorBelt
{
	// Token: 0x06000181 RID: 385 RVA: 0x0000866E File Offset: 0x0000686E
	protected override void OnEnable()
	{
		base.OnEnable();
		this._rightDirection = base.transform.right;
		this._upDirection = base.transform.up;
	}

	// Token: 0x06000182 RID: 386 RVA: 0x00008698 File Offset: 0x00006898
	protected override void FixedUpdate()
	{
		if (this.Disabled || this._physicsObjectsOnBelt.Count == 0)
		{
			return;
		}
		float fixedTime = Time.fixedTime;
		float num = Mathf.Sign(Mathf.Sin(fixedTime * 3.1415927f * 2f * this.ShakeFrequency));
		Vector3 vector = this._rightDirection * (this.ShakeSpeed * num);
		float num2 = Mathf.Sin(fixedTime * 3.1415927f * 2f * this.VerticalShakeFrequency);
		Vector3 vector2 = this._upDirection * (this.VerticalShakeSpeed * num2);
		Vector3 vector3 = vector + vector2;
		for (int i = this._physicsObjectsOnBelt.Count - 1; i >= 0; i--)
		{
			BasePhysicsObject basePhysicsObject = this._physicsObjectsOnBelt[i];
			Vector3 vector4 = this._pushVelocity + vector3;
			basePhysicsObject.AddConveyorVelocity(vector4, this.RetainYVelocity);
		}
	}

	// Token: 0x04000164 RID: 356
	[Header("Shaker Settings")]
	[Tooltip("Sideways speed magnitude applied on top of the normal conveyor speed.")]
	public float ShakeSpeed = 2f;

	// Token: 0x04000165 RID: 357
	[Tooltip("How many times per second the direction flips left/right.")]
	public float ShakeFrequency = 2f;

	// Token: 0x04000166 RID: 358
	[Tooltip("Up/down speed magnitude applied on top of the normal conveyor speed.")]
	public float VerticalShakeSpeed = 1f;

	// Token: 0x04000167 RID: 359
	[Tooltip("Vertical shake oscillations per second.")]
	public float VerticalShakeFrequency = 3f;

	// Token: 0x04000168 RID: 360
	protected Vector3 _rightDirection;

	// Token: 0x04000169 RID: 361
	protected Vector3 _upDirection;
}
