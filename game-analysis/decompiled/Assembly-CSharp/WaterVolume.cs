using System;
using UnityEngine;

// Token: 0x02000101 RID: 257
public class WaterVolume : MonoBehaviour
{
	// Token: 0x060006E2 RID: 1762 RVA: 0x000231C8 File Offset: 0x000213C8
	private void OnTriggerEnter(Collider other)
	{
		PlayerController playerController;
		if (other.TryGetComponent<PlayerController>(out playerController))
		{
			playerController.IsInWater = true;
		}
	}

	// Token: 0x060006E3 RID: 1763 RVA: 0x000231E8 File Offset: 0x000213E8
	private void OnTriggerExit(Collider other)
	{
		PlayerController playerController;
		if (other.TryGetComponent<PlayerController>(out playerController))
		{
			playerController.IsInWater = false;
		}
	}
}
