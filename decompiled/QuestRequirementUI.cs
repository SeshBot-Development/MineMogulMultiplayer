using TMPro;
using UnityEngine;

public class QuestRequirementUI : MonoBehaviour
{
	public TMP_Text NameText;

	public GameObject CompletedCheckmark;

	private QuestRequirement _requirement;

	public void Initialize(QuestRequirement requirement)
	{
		_requirement = requirement;
		RefreshDisplay();
	}

	public void RefreshDisplay()
	{
		NameText.text = _requirement.GetRequirementText();
		CompletedCheckmark.SetActive(_requirement.IsCompleted());
	}
}
