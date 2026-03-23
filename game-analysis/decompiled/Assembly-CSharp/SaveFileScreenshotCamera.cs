using System;
using ScreenSpaceCavityCurvature;
using UnityEngine;

// Token: 0x020000BF RID: 191
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(SSCC))]
public class SaveFileScreenshotCamera : MonoBehaviour
{
	// Token: 0x1700001C RID: 28
	// (get) Token: 0x06000523 RID: 1315 RVA: 0x0001AC92 File Offset: 0x00018E92
	// (set) Token: 0x06000524 RID: 1316 RVA: 0x0001AC9A File Offset: 0x00018E9A
	public Camera Camera { get; private set; }

	// Token: 0x1700001D RID: 29
	// (get) Token: 0x06000525 RID: 1317 RVA: 0x0001ACA3 File Offset: 0x00018EA3
	// (set) Token: 0x06000526 RID: 1318 RVA: 0x0001ACAB File Offset: 0x00018EAB
	public SSCC SSCC { get; private set; }

	// Token: 0x06000527 RID: 1319 RVA: 0x0001ACB4 File Offset: 0x00018EB4
	private void Awake()
	{
		this.Camera = base.GetComponent<Camera>();
		this.Camera.enabled = false;
		this.SSCC = base.GetComponent<SSCC>();
		this.SSCC.enabled = false;
	}
}
