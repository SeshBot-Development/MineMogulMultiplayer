using System;
using UnityEngine;

// Token: 0x02000104 RID: 260
public class GunAim : MonoBehaviour
{
	// Token: 0x060006EC RID: 1772 RVA: 0x000232A1 File Offset: 0x000214A1
	private void Start()
	{
		this.parentCamera = base.GetComponentInParent<Camera>();
	}

	// Token: 0x060006ED RID: 1773 RVA: 0x000232B0 File Offset: 0x000214B0
	private void Update()
	{
		float x = Input.mousePosition.x;
		float y = Input.mousePosition.y;
		if (x <= (float)this.borderLeft || x >= (float)(Screen.width - this.borderRight) || y <= (float)this.borderBottom || y >= (float)(Screen.height - this.borderTop))
		{
			this.isOutOfBounds = true;
		}
		else
		{
			this.isOutOfBounds = false;
		}
		if (!this.isOutOfBounds)
		{
			base.transform.LookAt(this.parentCamera.ScreenToWorldPoint(new Vector3(x, y, 5f)));
		}
	}

	// Token: 0x060006EE RID: 1774 RVA: 0x00023341 File Offset: 0x00021541
	public bool GetIsOutOfBounds()
	{
		return this.isOutOfBounds;
	}

	// Token: 0x040007EE RID: 2030
	public int borderLeft;

	// Token: 0x040007EF RID: 2031
	public int borderRight;

	// Token: 0x040007F0 RID: 2032
	public int borderTop;

	// Token: 0x040007F1 RID: 2033
	public int borderBottom;

	// Token: 0x040007F2 RID: 2034
	private Camera parentCamera;

	// Token: 0x040007F3 RID: 2035
	private bool isOutOfBounds;
}
