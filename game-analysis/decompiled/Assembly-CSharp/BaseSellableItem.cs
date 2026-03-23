using System;

// Token: 0x0200000D RID: 13
public class BaseSellableItem : BasePhysicsObject
{
	// Token: 0x06000063 RID: 99 RVA: 0x00003374 File Offset: 0x00001574
	public virtual float GetSellValue()
	{
		return this.BaseSellValue;
	}

	// Token: 0x04000055 RID: 85
	public float BaseSellValue = 1f;
}
