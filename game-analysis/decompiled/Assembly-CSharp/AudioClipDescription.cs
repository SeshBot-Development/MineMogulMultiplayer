using System;
using UnityEngine;

// Token: 0x020000E1 RID: 225
[Serializable]
public struct AudioClipDescription
{
	// Token: 0x04000741 RID: 1857
	public AudioClip clip;

	// Token: 0x04000742 RID: 1858
	[Range(0f, 2f)]
	public float volume;

	// Token: 0x04000743 RID: 1859
	[HideInInspector]
	public float maxRange;

	// Token: 0x04000744 RID: 1860
	[Range(0.5f, 2f)]
	public float pitch;

	// Token: 0x04000745 RID: 1861
	[HideInInspector]
	public int priority;
}
