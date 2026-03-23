using System;
using UnityEngine;

// Token: 0x0200005B RID: 91
[ExecuteAlways]
public class InventoryIconBaker : MonoBehaviour
{
	// Token: 0x04000214 RID: 532
	[Header("Scene References")]
	[SerializeField]
	private InventoryItemPreview _preview;

	// Token: 0x04000215 RID: 533
	[Header("Output")]
	[SerializeField]
	private int _iconSize = 256;

	// Token: 0x04000216 RID: 534
	[SerializeField]
	private string _outputFolder = "Assets/UI/InventoryIconsNew";

	// Token: 0x04000217 RID: 535
	[SerializeField]
	private float _pixelsPerUnit = 100f;
}
