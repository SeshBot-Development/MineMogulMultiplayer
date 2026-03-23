using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x0200004B RID: 75
public class EditableSign : MonoBehaviour, ICustomSaveDataProvider, IInteractable
{
	// Token: 0x06000201 RID: 513 RVA: 0x0000A564 File Offset: 0x00008764
	private void Awake()
	{
		int num = Random.Range(0, this._defaultTextChoices.Length);
		this.SignText.text = Singleton<KeybindManager>.Instance.ReplaceKeybindTokens(this._defaultTextChoices[num]);
		this.SignText.enabled = true;
	}

	// Token: 0x06000202 RID: 514 RVA: 0x0000A5A9 File Offset: 0x000087A9
	public void UpdateText(string input)
	{
		this.SignText.enabled = !string.IsNullOrEmpty(input);
		this.SignText.text = input;
	}

	// Token: 0x06000203 RID: 515 RVA: 0x0000A5CC File Offset: 0x000087CC
	public virtual void LoadFromSave(string json)
	{
		EditableSignSaveData editableSignSaveData = JsonUtility.FromJson<EditableSignSaveData>(json);
		if (editableSignSaveData == null)
		{
			editableSignSaveData = new EditableSignSaveData();
		}
		this.UpdateText(editableSignSaveData.DisplayText);
	}

	// Token: 0x06000204 RID: 516 RVA: 0x0000A5F5 File Offset: 0x000087F5
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new EditableSignSaveData
		{
			DisplayText = this.SignText.text
		});
	}

	// Token: 0x06000205 RID: 517 RVA: 0x0000A612 File Offset: 0x00008812
	public void StartEditingText()
	{
		Singleton<UIManager>.Instance.StartEditingText(this);
	}

	// Token: 0x06000206 RID: 518 RVA: 0x0000A61F File Offset: 0x0000881F
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x06000207 RID: 519 RVA: 0x0000A622 File Offset: 0x00008822
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000208 RID: 520 RVA: 0x0000A62A File Offset: 0x0000882A
	public string GetObjectName()
	{
		return this.UIDisplayName;
	}

	// Token: 0x06000209 RID: 521 RVA: 0x0000A632 File Offset: 0x00008832
	public virtual void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Edit Text")
		{
			this.StartEditingText();
		}
	}

	// Token: 0x040001E9 RID: 489
	public string UIDisplayName = "Arrow Sign";

	// Token: 0x040001EA RID: 490
	public TMP_Text SignText;

	// Token: 0x040001EB RID: 491
	[SerializeField]
	private string[] _defaultTextChoices;

	// Token: 0x040001EC RID: 492
	[SerializeField]
	private List<Interaction> _interactions;
}
