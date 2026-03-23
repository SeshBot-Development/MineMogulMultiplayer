using System;
using TMPro;
using UnityEngine;

// Token: 0x020000CF RID: 207
public abstract class BaseSettingOption : MonoBehaviour
{
	// Token: 0x06000569 RID: 1385 RVA: 0x0001CA78 File Offset: 0x0001AC78
	protected virtual void OnEnable()
	{
		this.UpdateLabel();
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x0001CA80 File Offset: 0x0001AC80
	protected virtual void OnValidate()
	{
		this.UpdateLabel();
	}

	// Token: 0x0600056B RID: 1387 RVA: 0x0001CA88 File Offset: 0x0001AC88
	private void UpdateLabel()
	{
		if (this._label != null)
		{
			this._label.text = this.displayName;
		}
	}

	// Token: 0x040006A8 RID: 1704
	[SerializeField]
	private TMP_Text _label;

	// Token: 0x040006A9 RID: 1705
	[SerializeField]
	protected string displayName = "Unnamed Setting";
}
