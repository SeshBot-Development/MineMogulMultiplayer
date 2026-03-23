using System;
using UnityEngine;

// Token: 0x020000FB RID: 251
public static class PhysicsUtils
{
	// Token: 0x060006AB RID: 1707 RVA: 0x00022BC4 File Offset: 0x00020DC4
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

	// Token: 0x060006AC RID: 1708 RVA: 0x00022C04 File Offset: 0x00020E04
	public static void IgnoreAllCollisions(GameObject object1, GameObject object2, bool ignore)
	{
		Collider[] components = object1.GetComponents<Collider>();
		Collider[] components2 = object2.GetComponents<Collider>();
		PhysicsUtils.IgnoreAllCollisions(components, components2, ignore);
	}

	// Token: 0x060006AD RID: 1709 RVA: 0x00022C28 File Offset: 0x00020E28
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

	// Token: 0x060006AE RID: 1710 RVA: 0x00022C6C File Offset: 0x00020E6C
	public static void SetLayerRecursively(GameObject obj, int layer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = layer;
		foreach (object obj2 in obj.transform)
		{
			PhysicsUtils.SetLayerRecursively(((Transform)obj2).gameObject, layer);
		}
	}
}
