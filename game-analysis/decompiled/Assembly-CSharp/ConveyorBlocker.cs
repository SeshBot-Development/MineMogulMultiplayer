using System;
using UnityEngine;

// Token: 0x02000037 RID: 55
public class ConveyorBlocker : MonoBehaviour
{
	// Token: 0x06000187 RID: 391 RVA: 0x00008874 File Offset: 0x00006A74
	private void Update()
	{
		if (this.Hinge && this.Conveyor)
		{
			bool flag = this.Hinge.angle < this.closedAngle;
			this.Conveyor.Disabled = flag;
		}
	}

	// Token: 0x0400016D RID: 365
	public HingeJoint Hinge;

	// Token: 0x0400016E RID: 366
	public ConveyorBelt Conveyor;

	// Token: 0x0400016F RID: 367
	private float closedAngle = -80f;
}
