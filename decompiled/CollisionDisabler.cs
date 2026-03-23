using UnityEngine;

public class CollisionDisabler : MonoBehaviour
{
	public Collider[] colliders;

	private void Start()
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			for (int j = i + 1; j < colliders.Length; j++)
			{
				if (colliders[i] != null && colliders[j] != null)
				{
					Physics.IgnoreCollision(colliders[i], colliders[j], ignore: true);
				}
			}
		}
	}
}
