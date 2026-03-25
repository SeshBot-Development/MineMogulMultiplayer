using System;
using UnityEngine;

public class ConveyorBeltShakerHorizontal : ConveyorBelt
{
	[Header("Shaker Settings")]
	[Tooltip("Sideways speed magnitude applied on top of the normal conveyor speed.")]
	public float ShakeSpeed = 2f;

	[Tooltip("How many times per second the direction flips left/right.")]
	public float ShakeFrequency = 2f;

	protected Vector3 _rightDirection;

	protected override void OnEnable()
	{
		base.OnEnable();
		_rightDirection = base.transform.right;
	}

	protected override void FixedUpdate()
	{
		if (!Disabled && _physicsObjectsOnBelt.Count != 0)
		{
			float num = Mathf.Sign(Mathf.Sin(Time.fixedTime * MathF.PI * 2f * ShakeFrequency));
			Vector3 vector = _rightDirection * (ShakeSpeed * num);
			for (int num2 = _physicsObjectsOnBelt.Count - 1; num2 >= 0; num2--)
			{
				BasePhysicsObject basePhysicsObject = _physicsObjectsOnBelt[num2];
				Vector3 velocity = _pushVelocity + vector;
				basePhysicsObject.AddConveyorVelocity(velocity, RetainYVelocity);
			}
		}
	}
}
