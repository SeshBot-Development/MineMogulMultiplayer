using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionWheelUI : MonoBehaviour
{
	public GameObject interactionButtonPrefab;

	public Transform ContentTransform;

	public TMP_Text ObjectNameText;

	private List<GameObject> interactionButtons = new List<GameObject>();

	private Dictionary<Button, IInteractable> buttonInteractableMapping = new Dictionary<Button, IInteractable>();

	public void PopulateInteractionWheel(IInteractable interactable)
	{
		ObjectNameText.text = interactable.GetObjectName();
		foreach (Interaction interaction in interactable.GetInteractions())
		{
			GameObject gameObject = Object.Instantiate(interactionButtonPrefab, ContentTransform);
			gameObject.GetComponentInChildren<TMP_Text>().text = interaction.Name;
			interactionButtons.Add(gameObject);
			Button component = gameObject.GetComponent<Button>();
			component.onClick.AddListener(delegate
			{
				SelectInteraction(interaction, interactable);
			});
			buttonInteractableMapping[component] = interactable;
		}
	}

	public void OpenWheel()
	{
		base.gameObject.SetActive(value: true);
	}

	public void CloseWheel()
	{
		ClearInteractionWheel();
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			CloseWheel();
		}
	}

	private void SelectInteraction(Interaction selectedInteraction, IInteractable interactable)
	{
		interactable?.Interact(selectedInteraction);
		CloseWheel();
	}

	public void ClearInteractionWheel()
	{
		foreach (Button key in buttonInteractableMapping.Keys)
		{
			key.onClick.RemoveAllListeners();
		}
		buttonInteractableMapping.Clear();
		foreach (Transform item in ContentTransform)
		{
			Object.Destroy(item.gameObject);
		}
		interactionButtons.Clear();
	}
}
