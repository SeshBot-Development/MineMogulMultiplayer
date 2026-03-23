using TMPro;
using UnityEngine;

public abstract class BaseSettingOption : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	protected string displayName = "Unnamed Setting";

	protected virtual void OnEnable()
	{
		UpdateLabel();
	}

	protected virtual void OnValidate()
	{
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		if (_label != null)
		{
			_label.text = displayName;
		}
	}
}
