using System;
using UnityEngine;

// Token: 0x02000107 RID: 263
[Serializable]
public class ParticleExamples
{
	// Token: 0x04000805 RID: 2053
	public string title;

	// Token: 0x04000806 RID: 2054
	[TextArea]
	public string description;

	// Token: 0x04000807 RID: 2055
	public bool isWeaponEffect;

	// Token: 0x04000808 RID: 2056
	public GameObject particleSystemGO;

	// Token: 0x04000809 RID: 2057
	public Vector3 particlePosition;

	// Token: 0x0400080A RID: 2058
	public Vector3 particleRotation;
}
