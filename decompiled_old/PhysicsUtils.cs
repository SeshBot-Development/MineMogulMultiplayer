using UnityEngine;

public static class PhysicsUtils
{
	public static void SimpleExplosion(Vector3 position, float radius, float force, float upwardsModifier = 0f)
	{
		Collider[] array = Physics.OverlapSphere(position, radius);
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody attachedRigidbody = array[i].attachedRigidbody;
			if (!(attachedRigidbody == null))
			{
				attachedRigidbody.AddExplosionForce(force, position, radius, upwardsModifier, ForceMode.Impulse);
			}
		}
	}

	public static void IgnoreAllCollisions(GameObject object1, GameObject object2, bool ignore)
	{
		Collider[] components = object1.GetComponents<Collider>();
		Collider[] components2 = object2.GetComponents<Collider>();
		IgnoreAllCollisions(components, components2, ignore);
	}

	public static void IgnoreAllCollisions(Collider[] aCols, Collider[] bCols, bool ignore)
	{
		foreach (Collider collider in aCols)
		{
			foreach (Collider collider2 in bCols)
			{
				Physics.IgnoreCollision(collider, collider2, ignore);
			}
		}
	}

	public static void SetLayerRecursively(GameObject obj, int layer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = layer;
		foreach (Transform item in obj.transform)
		{
			SetLayerRecursively(item.gameObject, layer);
		}
	}
}
