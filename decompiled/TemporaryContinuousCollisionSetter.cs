using System.Collections.Generic;
using UnityEngine;

public class TemporaryContinuousCollisionSetter : MonoBehaviour
{
	public float TimeToStayContinuous = 1f;

	private readonly Dictionary<Rigidbody, float> _revertAt = new Dictionary<Rigidbody, float>(256);

	private static readonly List<Rigidbody> _toRemove = new List<Rigidbody>(256);

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if ((bool)attachedRigidbody)
		{
			attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			_revertAt[attachedRigidbody] = Time.time + TimeToStayContinuous;
		}
	}

	private void Update()
	{
		if (_revertAt.Count == 0)
		{
			return;
		}
		_toRemove.Clear();
		float time = Time.time;
		foreach (KeyValuePair<Rigidbody, float> item in _revertAt)
		{
			Rigidbody key = item.Key;
			if (!key)
			{
				_toRemove.Add(key);
			}
			else if (time >= item.Value)
			{
				key.collisionDetectionMode = CollisionDetectionMode.Discrete;
				_toRemove.Add(key);
			}
		}
		for (int i = 0; i < _toRemove.Count; i++)
		{
			_revertAt.Remove(_toRemove[i]);
		}
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<Rigidbody, float> item in _revertAt)
		{
			Rigidbody key = item.Key;
			if ((bool)key)
			{
				key.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}
		_revertAt.Clear();
	}
}
