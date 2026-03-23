using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditableSign : MonoBehaviour, ICustomSaveDataProvider, IInteractable
{
	public string UIDisplayName = "Arrow Sign";

	public TMP_Text SignText;

	[SerializeField]
	private string[] _defaultTextChoices;

	[SerializeField]
	private List<Interaction> _interactions;

	private void Awake()
	{
		int num = Random.Range(0, _defaultTextChoices.Length);
		SignText.text = Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(_defaultTextChoices[num]);
		SignText.enabled = true;
	}

	public void UpdateText(string input)
	{
		SignText.enabled = !string.IsNullOrEmpty(input);
		SignText.text = input;
	}

	public virtual void LoadFromSave(string json)
	{
		EditableSignSaveData editableSignSaveData = JsonUtility.FromJson<EditableSignSaveData>(json);
		if (editableSignSaveData == null)
		{
			editableSignSaveData = new EditableSignSaveData();
		}
		UpdateText(editableSignSaveData.DisplayText);
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new EditableSignSaveData
		{
			DisplayText = SignText.text
		});
	}

	public void StartEditingText()
	{
		Singleton<UIManager>.Instance.StartEditingText(this);
	}

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public string GetObjectName()
	{
		return UIDisplayName;
	}

	public virtual void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Edit Text")
		{
			StartEditingText();
		}
	}
}
