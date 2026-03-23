using System;
using UnityEngine;

// Token: 0x020000B9 RID: 185
[Serializable]
public class LevelInfo
{
	// Token: 0x0400061F RID: 1567
	public string LevelID;

	// Token: 0x04000620 RID: 1568
	public string DisplayName;

	// Token: 0x04000621 RID: 1569
	public string SceneName;

	// Token: 0x04000622 RID: 1570
	[TextArea]
	public string Description = "The ultimate adventure of the game";

	// Token: 0x04000623 RID: 1571
	public Texture Thumbnail;

	// Token: 0x04000624 RID: 1572
	public bool ShouldAppearInMapSelect;
}
