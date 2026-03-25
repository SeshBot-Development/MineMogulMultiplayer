using UnityEngine;

public class WaterVolume : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<PlayerController>(out var component))
		{
			component.IsInWater = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<PlayerController>(out var component))
		{
			component.IsInWater = false;
		}
	}
}
