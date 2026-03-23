using System;
using UnityEngine;

// Token: 0x02000109 RID: 265
public class Readme : ScriptableObject
{
	// Token: 0x04000813 RID: 2067
	public Texture2D icon;

	// Token: 0x04000814 RID: 2068
	public string title;

	// Token: 0x04000815 RID: 2069
	public Readme.Section[] sections;

	// Token: 0x04000816 RID: 2070
	public bool loadedLayout;

	// Token: 0x0200018D RID: 397
	[Serializable]
	public class Section
	{
		// Token: 0x040009B0 RID: 2480
		public string heading;

		// Token: 0x040009B1 RID: 2481
		public string text;

		// Token: 0x040009B2 RID: 2482
		public string linkText;

		// Token: 0x040009B3 RID: 2483
		public string url;
	}
}
