using System;
using UnityEngine;

// Token: 0x02000100 RID: 256
[DefaultExecutionOrder(-10000)]
public class VersionManager : Singleton<VersionManager>
{
	// Token: 0x060006DF RID: 1759 RVA: 0x000231A6 File Offset: 0x000213A6
	public string GetFormattedVersionText()
	{
		return "Version: " + this.VersionNumber;
	}

	// Token: 0x060006E0 RID: 1760 RVA: 0x000231B8 File Offset: 0x000213B8
	public string GetVersionTextWithoutLabel()
	{
		return this.VersionNumber;
	}

	// Token: 0x040007E8 RID: 2024
	public string VersionNumber;
}
