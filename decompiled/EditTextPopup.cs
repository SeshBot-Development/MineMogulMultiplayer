using TMPro;
using UnityEngine;

public class EditTextPopup : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _textField;

	private EditableSign _currentlyEditingSign;

	public void StartEditingText(EditableSign editableSign)
	{
		base.gameObject.SetActive(value: true);
		_currentlyEditingSign = editableSign;
		_textField.text = _currentlyEditingSign.SignText.text;
		_textField.Select();
		_textField.ActivateInputField();
		_textField.onValueChanged.AddListener(UpdateTextOnSign);
	}

	private void UpdateTextOnSign(string text)
	{
		if (_currentlyEditingSign == null)
		{
			FinishAndClose();
		}
		else
		{
			_currentlyEditingSign.SignText.text = text;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			FinishAndClose();
		}
	}

	public void FinishAndClose()
	{
		_textField.onValueChanged.RemoveListener(UpdateTextOnSign);
		if (!(_currentlyEditingSign == null))
		{
			_currentlyEditingSign.UpdateText(_textField.text);
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void OnDisable()
	{
		FinishAndClose();
	}
}
