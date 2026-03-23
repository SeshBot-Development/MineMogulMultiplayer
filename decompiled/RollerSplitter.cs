using System.Collections.Generic;
using UnityEngine;

public class RollerSplitter : MonoBehaviour
{
	public Collider[] LeftConveyors;

	public Collider[] RightConveyors;

	private bool _nextGoStraight = true;

	private readonly Queue<GameObject> _recentObjects = new Queue<GameObject>();

	private TimeSince _timeSinceLastCleared;

	private void OnTriggerEnter(Collider other)
	{
		GameObject item = other.gameObject;
		if (_timeSinceLastCleared > 3)
		{
			_timeSinceLastCleared = 0f;
			_recentObjects.Clear();
		}
		else if (_recentObjects.Contains(item))
		{
			return;
		}
		_recentObjects.Enqueue(item);
		if (_recentObjects.Count > 10)
		{
			_recentObjects.Dequeue();
		}
		SetCollisions(other.gameObject, _nextGoStraight);
		_nextGoStraight = !_nextGoStraight;
	}

	private void SetCollisions(GameObject obj, bool goStraight)
	{
		Collider[] components = obj.GetComponents<Collider>();
		if (goStraight)
		{
			PhysicsUtils.IgnoreAllCollisions(components, LeftConveyors, ignore: false);
			PhysicsUtils.IgnoreAllCollisions(components, RightConveyors, ignore: true);
		}
		else
		{
			PhysicsUtils.IgnoreAllCollisions(components, LeftConveyors, ignore: true);
			PhysicsUtils.IgnoreAllCollisions(components, RightConveyors, ignore: false);
		}
	}
}
