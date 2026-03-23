using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000032 RID: 50
public class ConveyorBatchRenderingComponent : MonoBehaviour
{
	// Token: 0x06000169 RID: 361 RVA: 0x000081EC File Offset: 0x000063EC
	private void OnEnable()
	{
		this.CachedMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		ConveyorBatchRenderingComponent.AllConveyors.Add(this);
		ConveyorBatchRenderingComponent.NeedsUpdate = true;
		base.GetComponent<Renderer>().enabled = false;
	}

	// Token: 0x0600016A RID: 362 RVA: 0x00008242 File Offset: 0x00006442
	private void OnDisable()
	{
		ConveyorBatchRenderingComponent.AllConveyors.Remove(this);
		ConveyorBatchRenderingComponent.NeedsUpdate = true;
	}

	// Token: 0x0600016B RID: 363 RVA: 0x00008256 File Offset: 0x00006456
	public void RefreshMatrix()
	{
		this.CachedMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		ConveyorBatchRenderingComponent.NeedsUpdate = true;
	}

	// Token: 0x04000158 RID: 344
	public static readonly List<ConveyorBatchRenderingComponent> AllConveyors = new List<ConveyorBatchRenderingComponent>();

	// Token: 0x04000159 RID: 345
	public static bool NeedsUpdate = true;

	// Token: 0x0400015A RID: 346
	public Matrix4x4 CachedMatrix;

	// Token: 0x0400015B RID: 347
	public int MeshIndex;
}
