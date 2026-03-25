using System.Collections.Generic;
using UnityEngine;

public class BasePhysicsObject : MonoBehaviour
{
	public const float STANDARD_LINEAR_DAMPING = 0.2f;

	public const float STANDARD_ANGULAR_DAMPING = 0.05f;

	[HideInInspector]
	public Vector3 SumVelocity;

	[HideInInspector]
	public float BestY;

	[HideInInspector]
	public int Count;

	[HideInInspector]
	public bool RetainY;

	private List<ConveyorBelt> _conveyorBeltsTouchingThis = new List<ConveyorBelt>();

	public Rigidbody Rb { get; private set; }

	protected virtual void Awake()
	{
		Rb = GetComponent<Rigidbody>();
		if (Rb != null)
		{
			Rb.linearDamping = 0.2f;
			Rb.angularDamping = 0.05f;
		}
	}

	protected virtual void OnEnable()
	{
		ConveyorBeltManager.Register(this);
	}

	protected virtual void OnDisable()
	{
		ConveyorBeltManager.Unregister(this);
	}

	public void ClearTouchingConveyorBelts()
	{
		foreach (ConveyorBelt conveyorBeltsTouchingThi in _conveyorBeltsTouchingThis)
		{
			conveyorBeltsTouchingThi.RemovePhysicsObject(this);
		}
		_conveyorBeltsTouchingThis.Clear();
	}

	public void AddTouchingConveyorBelt(ConveyorBelt belt)
	{
		_conveyorBeltsTouchingThis.Add(belt);
	}

	public bool IsOnAnyConveyor()
	{
		return _conveyorBeltsTouchingThis.Count > 0;
	}

	public void RemoveTouchingConveyorBelt(ConveyorBelt belt)
	{
		_conveyorBeltsTouchingThis.Remove(belt);
	}

	public void AddConveyorVelocity(Vector3 velocity, bool retainY)
	{
		if (Count == 0)
		{
			SumVelocity = velocity;
		}
		else
		{
			SumVelocity += velocity;
		}
		if (velocity.y > BestY)
		{
			BestY = velocity.y;
		}
		Count++;
		if (retainY)
		{
			RetainY = true;
		}
	}

	public void ResetAccum()
	{
		SumVelocity = default(Vector3);
		BestY = 0f;
		Count = 0;
		RetainY = false;
	}
}
