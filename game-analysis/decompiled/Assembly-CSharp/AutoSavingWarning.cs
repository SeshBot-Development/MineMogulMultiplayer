using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Token: 0x020000B1 RID: 177
public class AutoSavingWarning : MonoBehaviour
{
	// Token: 0x060004E1 RID: 1249 RVA: 0x0001A06D File Offset: 0x0001826D
	public void OnEnable()
	{
		this.IsSaving = true;
		this.Text.text = "Auto Saving";
		this._timer = 0f;
		this._dotCount = 0;
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x0001A098 File Offset: 0x00018298
	private void Update()
	{
		if (!this.IsSaving)
		{
			return;
		}
		this._timer += Time.deltaTime;
		float num = 0.25f;
		if (this._timer >= num)
		{
			this._timer = 0f;
			this._dotCount = (this._dotCount + 1) % 4;
			string text = new string('.', this._dotCount);
			this.Text.text = "Auto Saving" + text;
		}
	}

	// Token: 0x060004E3 RID: 1251 RVA: 0x0001A10E File Offset: 0x0001830E
	public void OnSavingFinished()
	{
		if (!this.IsSaving)
		{
			return;
		}
		this.IsSaving = false;
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		base.StartCoroutine(this.WaitThenClose());
	}

	// Token: 0x060004E4 RID: 1252 RVA: 0x0001A136 File Offset: 0x00018336
	private IEnumerator WaitThenClose()
	{
		this.Text.text = "Auto Save Complete!";
		yield return new WaitForSeconds(2.5f);
		base.gameObject.SetActive(false);
		yield break;
	}

	// Token: 0x04000582 RID: 1410
	public TMP_Text Text;

	// Token: 0x04000583 RID: 1411
	public bool IsSaving = true;

	// Token: 0x04000584 RID: 1412
	private float _timer;

	// Token: 0x04000585 RID: 1413
	private int _dotCount;
}
