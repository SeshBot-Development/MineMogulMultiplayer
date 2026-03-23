using System.Collections;
using UnityEngine;

public class PhysicsGib : BaseSellableItem
{
	private float _despawnTime = 8f;

	public void DetatchAndDespawn(Vector3? velocity = null)
	{
		base.transform.SetParent(null);
		base.gameObject.SetActive(value: true);
		PhysicsUtils.IgnoreAllCollisions(base.gameObject, Singleton<SoundManager>.Instance.PlayerTransform.gameObject, ignore: true);
		if (velocity.HasValue)
		{
			base.Rb.linearVelocity = velocity.Value;
		}
		StartCoroutine(WaitThenDespawn());
	}

	private IEnumerator WaitThenDespawn()
	{
		float seconds = _despawnTime * Random.Range(0.7f, 1.3f);
		yield return new WaitForSeconds(seconds);
		if (this != null)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
