using System;
using TMPro;
using UnityEngine;

// Token: 0x02000064 RID: 100
[RequireComponent(typeof(TMP_Text))]
public class KeybindTokenText : MonoBehaviour
{
	// Token: 0x06000294 RID: 660 RVA: 0x0000CA41 File Offset: 0x0000AC41
	private void Awake()
	{
		this._text = base.GetComponent<TMP_Text>();
		if (string.IsNullOrEmpty(this.TextTemplate))
		{
			this.TextTemplate = this._text.text;
		}
	}

	// Token: 0x06000295 RID: 661 RVA: 0x0000CA6D File Offset: 0x0000AC6D
	public void SetText(string newTemplate)
	{
		this.TextTemplate = newTemplate;
		this.Refresh();
	}

	// Token: 0x06000296 RID: 662 RVA: 0x0000CA7C File Offset: 0x0000AC7C
	private void OnEnable()
	{
		this.Refresh();
	}

	// Token: 0x06000297 RID: 663 RVA: 0x0000CA84 File Offset: 0x0000AC84
	public void Refresh()
	{
		if (this._text == null)
		{
			return;
		}
		this._text.text = Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(this.TextTemplate);
	}

	// Token: 0x04000272 RID: 626
	[TextArea]
	public string TextTemplate;

	// Token: 0x04000273 RID: 627
	private TMP_Text _text;
}
