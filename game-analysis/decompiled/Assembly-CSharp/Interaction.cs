using System;
using UnityEngine;

// Token: 0x02000059 RID: 89
[CreateAssetMenu(fileName = "New Interaction", menuName = "Interactions/Interaction")]
public class Interaction : ScriptableObject
{
	// Token: 0x0400020C RID: 524
	public string Name;

	// Token: 0x0400020D RID: 525
	public string Description;

	// Token: 0x0400020E RID: 526
	public Sprite Icon;
}
