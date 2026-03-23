using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class KeybindTokenText : MonoBehaviour
{
	[TextArea]
	public string TextTemplate;

	private TMP_Text _text;

	private void Awake()
	{
		_text = GetComponent<TMP_Text>();
		if (string.IsNullOrEmpty(TextTemplate))
		{
			TextTemplate = _text.text;
		}
	}

	public void SetText(string newTemplate)
	{
		TextTemplate = newTemplate;
		Refresh();
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		if (!(_text == null))
		{
			_text.text = Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(TextTemplate);
		}
	}
}
