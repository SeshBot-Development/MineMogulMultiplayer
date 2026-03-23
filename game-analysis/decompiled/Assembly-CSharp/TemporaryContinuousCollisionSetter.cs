using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E6 RID: 230
public class TemporaryContinuousCollisionSetter : MonoBehaviour
{
	// Token: 0x06000629 RID: 1577 RVA: 0x00020308 File Offset: 0x0001E508
	private void OnTriggerEnter(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (!attachedRigidbody)
		{
			return;
		}
		attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		this._revertAt[attachedRigidbody] = Time.time + this.TimeToStayContinuous;
	}

	// Token: 0x0600062A RID: 1578 RVA: 0x00020344 File Offset: 0x0001E544
	private void Update()
	{
		if (this._revertAt.Count == 0)
		{
			return;
		}
		TemporaryContinuousCollisionSetter._toRemove.Clear();
		float time = Time.time;
		foreach (KeyValuePair<Rigidbody, float> keyValuePair in this._revertAt)
		{
			Rigidbody key = keyValuePair.Key;
			if (!key)
			{
				TemporaryContinuousCollisionSetter._toRemove.Add(key);
			}
			else if (time >= keyValuePair.Value)
			{
				key.collisionDetectionMode = CollisionDetectionMode.Discrete;
				TemporaryContinuousCollisionSetter._toRemove.Add(key);
			}
		}
		for (int i = 0; i < TemporaryContinuousCollisionSetter._toRemove.Count; i++)
		{
			this._revertAt.Remove(TemporaryContinuousCollisionSetter._toRemove[i]);
		}
	}

	// Token: 0x0600062B RID: 1579 RVA: 0x00020418 File Offset: 0x0001E618
	private void OnDisable()
	{
		foreach (KeyValuePair<Rigidbody, float> keyValuePair in this._revertAt)
		{
			Rigidbody key = keyValuePair.Key;
			if (key)
			{
				key.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}
		this._revertAt.Clear();
	}

	// Token: 0x04000768 RID: 1896
	public float TimeToStayContinuous = 1f;

	// Token: 0x04000769 RID: 1897
	private readonly Dictionary<Rigidbody, float> _revertAt = new Dictionary<Rigidbody, float>(256);

	// Token: 0x0400076A RID: 1898
	private static readonly List<Rigidbody> _toRemove = new List<Rigidbody>(256);
}
