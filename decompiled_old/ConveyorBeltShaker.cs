using System;
using UnityEngine;

public class ConveyorBeltShaker : ConveyorBelt
{
	[Header("Shaker Settings")]
	[Tooltip("Sideways speed magnitude applied on top of the normal conveyor speed.")]
	public float ShakeSpeed = 2f;

	[Tooltip("How many times per second the direction flips left/right.")]
	public float ShakeFrequency = 2f;

	[Tooltip("Up/down speed magnitude applied on top of the normal conveyor speed.")]
	public float VerticalShakeSpeed = 1f;

	[Tooltip("Vertical shake oscillations per second.")]
	public float VerticalShakeFrequency = 3f;

	protected Vector3 _rightDirection;

	protected Vector3 _upDirection;

	protected override void OnEnable()
	{
		base.OnEnable();
		_rightDirection = base.transform.right;
		_upDirection = base.transform.up;
	}

	protected override void FixedUpdate()
	{
		if (!Disabled && _physicsObjectsOnBelt.Count != 0)
		{
			float fixedTime = Time.fixedTime;
			float num = Mathf.Sign(Mathf.Sin(fixedTime * MathF.PI * 2f * ShakeFrequency));
			Vector3 vector = _rightDirection * (ShakeSpeed * num);
			float num2 = Mathf.Sin(fixedTime * MathF.PI * 2f * VerticalShakeFrequency);
			Vector3 vector2 = _upDirection * (VerticalShakeSpeed * num2);
			Vector3 vector3 = vector + vector2;
			for (int num3 = _physicsObjectsOnBelt.Count - 1; num3 >= 0; num3--)
			{
				BasePhysicsObject basePhysicsObject = _physicsObjectsOnBelt[num3];
				Vector3 velocity = _pushVelocity + vector3;
				basePhysicsObject.AddConveyorVelocity(velocity, RetainYVelocity);
			}
		}
	}
}
