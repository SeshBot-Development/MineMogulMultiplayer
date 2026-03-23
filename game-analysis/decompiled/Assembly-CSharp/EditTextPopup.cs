using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x0200004D RID: 77
public class EditTextPopup : MonoBehaviour
{
	// Token: 0x0600020C RID: 524 RVA: 0x0000A668 File Offset: 0x00008868
	public void StartEditingText(EditableSign editableSign)
	{
		base.gameObject.SetActive(true);
		this._currentlyEditingSign = editableSign;
		this._textField.text = this._currentlyEditingSign.SignText.text;
		this._textField.Select();
		this._textField.ActivateInputField();
		this._textField.onValueChanged.AddListener(new UnityAction<string>(this.UpdateTextOnSign));
	}

	// Token: 0x0600020D RID: 525 RVA: 0x0000A6D5 File Offset: 0x000088D5
	private void UpdateTextOnSign(string text)
	{
		if (this._currentlyEditingSign == null)
		{
			this.FinishAndClose();
			return;
		}
		this._currentlyEditingSign.SignText.text = text;
	}

	// Token: 0x0600020E RID: 526 RVA: 0x0000A6FD File Offset: 0x000088FD
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			this.FinishAndClose();
		}
	}

	// Token: 0x0600020F RID: 527 RVA: 0x0000A710 File Offset: 0x00008910
	public void FinishAndClose()
	{
		this._textField.onValueChanged.RemoveListener(new UnityAction<string>(this.UpdateTextOnSign));
		if (this._currentlyEditingSign == null)
		{
			return;
		}
		this._currentlyEditingSign.UpdateText(this._textField.text);
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000210 RID: 528 RVA: 0x0000A777 File Offset: 0x00008977
	public void OnDisable()
	{
		this.FinishAndClose();
	}

	// Token: 0x040001EE RID: 494
	[SerializeField]
	private TMP_InputField _textField;

	// Token: 0x040001EF RID: 495
	private EditableSign _currentlyEditingSign;
}
