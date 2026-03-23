using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005A RID: 90
public class InteractionWheelUI : MonoBehaviour
{
	// Token: 0x06000238 RID: 568 RVA: 0x0000AFF8 File Offset: 0x000091F8
	public void PopulateInteractionWheel(IInteractable interactable)
	{
		this.ObjectNameText.text = interactable.GetObjectName();
		using (List<Interaction>.Enumerator enumerator = interactable.GetInteractions().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Interaction interaction = enumerator.Current;
				GameObject gameObject = Object.Instantiate<GameObject>(this.interactionButtonPrefab, this.ContentTransform);
				gameObject.GetComponentInChildren<TMP_Text>().text = interaction.Name;
				this.interactionButtons.Add(gameObject);
				Button component = gameObject.GetComponent<Button>();
				component.onClick.AddListener(delegate
				{
					this.SelectInteraction(interaction, interactable);
				});
				this.buttonInteractableMapping[component] = interactable;
			}
		}
	}

	// Token: 0x06000239 RID: 569 RVA: 0x0000B0F8 File Offset: 0x000092F8
	public void OpenWheel()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x0600023A RID: 570 RVA: 0x0000B106 File Offset: 0x00009306
	public void CloseWheel()
	{
		this.ClearInteractionWheel();
		base.gameObject.SetActive(false);
	}

	// Token: 0x0600023B RID: 571 RVA: 0x0000B11A File Offset: 0x0000931A
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			this.CloseWheel();
		}
	}

	// Token: 0x0600023C RID: 572 RVA: 0x0000B12B File Offset: 0x0000932B
	private void SelectInteraction(Interaction selectedInteraction, IInteractable interactable)
	{
		if (interactable != null)
		{
			interactable.Interact(selectedInteraction);
		}
		this.CloseWheel();
	}

	// Token: 0x0600023D RID: 573 RVA: 0x0000B140 File Offset: 0x00009340
	public void ClearInteractionWheel()
	{
		foreach (Button button in this.buttonInteractableMapping.Keys)
		{
			button.onClick.RemoveAllListeners();
		}
		this.buttonInteractableMapping.Clear();
		foreach (object obj in this.ContentTransform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this.interactionButtons.Clear();
	}

	// Token: 0x0400020F RID: 527
	public GameObject interactionButtonPrefab;

	// Token: 0x04000210 RID: 528
	public Transform ContentTransform;

	// Token: 0x04000211 RID: 529
	public TMP_Text ObjectNameText;

	// Token: 0x04000212 RID: 530
	private List<GameObject> interactionButtons = new List<GameObject>();

	// Token: 0x04000213 RID: 531
	private Dictionary<Button, IInteractable> buttonInteractableMapping = new Dictionary<Button, IInteractable>();
}
