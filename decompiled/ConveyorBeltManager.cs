using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class ConveyorBeltManager : Singleton<ConveyorBeltManager>
{
	private static readonly List<BasePhysicsObject> Objects = new List<BasePhysicsObject>();

	private int _currentBeltIndex;

	public static void Register(BasePhysicsObject obj)
	{
		if (obj != null && !Objects.Contains(obj))
		{
			Objects.Add(obj);
		}
	}

	public static void Unregister(BasePhysicsObject obj)
	{
		if (obj != null)
		{
			Objects.Remove(obj);
		}
	}

	private void Update()
	{
		List<ConveyorBelt> allConveyorBelts = ConveyorBelt.AllConveyorBelts;
		if (allConveyorBelts.Count == 0)
		{
			_currentBeltIndex = 0;
			return;
		}
		if (_currentBeltIndex >= allConveyorBelts.Count)
		{
			_currentBeltIndex = 0;
		}
		ConveyorBelt conveyorBelt = allConveyorBelts[_currentBeltIndex];
		if (conveyorBelt == null)
		{
			allConveyorBelts.RemoveAt(_currentBeltIndex);
		}
		else
		{
			conveyorBelt.ClearNullObjectsOnBelt();
		}
		_currentBeltIndex++;
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			BasePhysicsObject basePhysicsObject = Objects[i];
			if (basePhysicsObject.Count == 0)
			{
				basePhysicsObject.ResetAccum();
				continue;
			}
			Rigidbody rb = basePhysicsObject.Rb;
			Vector3 linearVelocity = basePhysicsObject.SumVelocity / basePhysicsObject.Count;
			if (basePhysicsObject.RetainY)
			{
				linearVelocity.y = rb.linearVelocity.y;
			}
			else if (basePhysicsObject.BestY > 0f)
			{
				linearVelocity.y = basePhysicsObject.BestY;
			}
			rb.linearVelocity = linearVelocity;
			basePhysicsObject.ResetAccum();
		}
	}
}
