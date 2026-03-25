using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
	public float Speed = 0.8f;

	public bool Disabled;

	public bool RetainYVelocity;

	protected List<BasePhysicsObject> _physicsObjectsOnBelt = new List<BasePhysicsObject>();

	protected Vector3 _pushVelocity;

	public static List<ConveyorBelt> AllConveyorBelts { get; private set; } = new List<ConveyorBelt>();

	protected virtual void OnEnable()
	{
		_pushVelocity = base.transform.forward * Speed;
		AllConveyorBelts.Add(this);
	}

	public void ChangeSpeed(float newSpeed)
	{
		Speed = newSpeed;
		_pushVelocity = base.transform.forward * Speed;
	}

	protected virtual void FixedUpdate()
	{
		if (!Disabled && _physicsObjectsOnBelt.Count != 0)
		{
			for (int num = _physicsObjectsOnBelt.Count - 1; num >= 0; num--)
			{
				_physicsObjectsOnBelt[num].AddConveyorVelocity(_pushVelocity, RetainYVelocity);
			}
		}
	}

	protected virtual void OnDisable()
	{
		foreach (BasePhysicsObject item in _physicsObjectsOnBelt)
		{
			if (item != null)
			{
				item.RemoveTouchingConveyorBelt(this);
			}
		}
		_physicsObjectsOnBelt.Clear();
		AllConveyorBelts.Remove(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (!(attachedRigidbody != null) || attachedRigidbody.isKinematic)
		{
			return;
		}
		BasePhysicsObject component = attachedRigidbody.GetComponent<BasePhysicsObject>();
		if (component != null)
		{
			AddPhysicsObject(component);
			return;
		}
		component = attachedRigidbody.AddComponent<BasePhysicsObject>();
		if (component != null)
		{
			AddPhysicsObject(component);
		}
	}

	public void AddPhysicsObject(BasePhysicsObject obj)
	{
		_physicsObjectsOnBelt.Add(obj);
		obj.AddTouchingConveyorBelt(this);
	}

	public void RemovePhysicsObject(BasePhysicsObject obj)
	{
		_physicsObjectsOnBelt.Remove(obj);
	}

	public void ClearNullObjectsOnBelt()
	{
		for (int num = _physicsObjectsOnBelt.Count - 1; num >= 0; num--)
		{
			BasePhysicsObject basePhysicsObject = _physicsObjectsOnBelt[num];
			if (basePhysicsObject == null || !basePhysicsObject.isActiveAndEnabled || basePhysicsObject.Rb.isKinematic)
			{
				_physicsObjectsOnBelt.RemoveAt(num);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			BasePhysicsObject component = attachedRigidbody.GetComponent<BasePhysicsObject>();
			if (component != null)
			{
				_physicsObjectsOnBelt.Remove(component);
				component.RemoveTouchingConveyorBelt(this);
			}
		}
	}
}
