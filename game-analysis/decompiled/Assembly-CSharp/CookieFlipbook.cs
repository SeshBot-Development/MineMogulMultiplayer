using System;
using UnityEngine;

// Token: 0x02000003 RID: 3
[RequireComponent(typeof(Light))]
public class CookieFlipbook : MonoBehaviour
{
	// Token: 0x06000013 RID: 19 RVA: 0x000023DC File Offset: 0x000005DC
	private void Awake()
	{
		this._light = base.GetComponent<Light>();
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000023EC File Offset: 0x000005EC
	private void Update()
	{
		if (this.frames == null || this.frames.Length == 0)
		{
			return;
		}
		this._t += Time.deltaTime * this.fps;
		int num = (int)this._t % this.frames.Length;
		if (num != this._i)
		{
			this._i = num;
			this._light.cookie = this.frames[this._i];
		}
	}

	// Token: 0x0400001E RID: 30
	public Texture[] frames;

	// Token: 0x0400001F RID: 31
	public float fps = 20f;

	// Token: 0x04000020 RID: 32
	private Light _light;

	// Token: 0x04000021 RID: 33
	private int _i;

	// Token: 0x04000022 RID: 34
	private float _t;
}
