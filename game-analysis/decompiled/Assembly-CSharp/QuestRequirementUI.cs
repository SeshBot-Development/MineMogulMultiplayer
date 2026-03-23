using System;
using TMPro;
using UnityEngine;

// Token: 0x02000099 RID: 153
public class QuestRequirementUI : MonoBehaviour
{
	// Token: 0x06000421 RID: 1057 RVA: 0x00016969 File Offset: 0x00014B69
	public void Initialize(QuestRequirement requirement)
	{
		this._requirement = requirement;
		this.RefreshDisplay();
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x00016978 File Offset: 0x00014B78
	public void RefreshDisplay()
	{
		this.NameText.text = this._requirement.GetRequirementText();
		this.CompletedCheckmark.SetActive(this._requirement.IsCompleted());
	}

	// Token: 0x040004B7 RID: 1207
	public TMP_Text NameText;

	// Token: 0x040004B8 RID: 1208
	public GameObject CompletedCheckmark;

	// Token: 0x040004B9 RID: 1209
	private QuestRequirement _requirement;
}
