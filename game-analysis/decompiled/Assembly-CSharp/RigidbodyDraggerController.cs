using System;
using UnityEngine;

// Token: 0x020000A9 RID: 169
public class RigidbodyDraggerController : MonoBehaviour
{
	// Token: 0x060004B0 RID: 1200 RVA: 0x000193AC File Offset: 0x000175AC
	private void OnJointBreak(float breakForce)
	{
		if (this.playerController != null)
		{
			this.playerController.ReleaseObject();
		}
	}

	// Token: 0x0400053C RID: 1340
	public PlayerController playerController;
}
