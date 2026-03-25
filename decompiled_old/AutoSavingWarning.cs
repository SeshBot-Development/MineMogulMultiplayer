using System.Collections;
using TMPro;
using UnityEngine;

public class AutoSavingWarning : MonoBehaviour
{
	public TMP_Text Text;

	public bool IsSaving = true;

	private float _timer;

	private int _dotCount;

	public void OnEnable()
	{
		IsSaving = true;
		Text.text = "Auto Saving";
		_timer = 0f;
		_dotCount = 0;
	}

	private void Update()
	{
		if (IsSaving)
		{
			_timer += Time.deltaTime;
			float num = 0.25f;
			if (_timer >= num)
			{
				_timer = 0f;
				_dotCount = (_dotCount + 1) % 4;
				string text = new string('.', _dotCount);
				Text.text = "Auto Saving" + text;
			}
		}
	}

	public void OnSavingFinished()
	{
		if (IsSaving)
		{
			IsSaving = false;
			if (base.isActiveAndEnabled)
			{
				StartCoroutine(WaitThenClose());
			}
		}
	}

	private IEnumerator WaitThenClose()
	{
		Text.text = "Auto Save Complete!";
		yield return new WaitForSeconds(2.5f);
		base.gameObject.SetActive(value: false);
	}
}
