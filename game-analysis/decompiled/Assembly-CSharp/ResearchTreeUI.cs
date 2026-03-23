using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000A6 RID: 166
public class ResearchTreeUI : MonoBehaviour
{
	// Token: 0x06000498 RID: 1176 RVA: 0x00018FCF File Offset: 0x000171CF
	private void Update()
	{
	}

	// Token: 0x06000499 RID: 1177 RVA: 0x00018FD1 File Offset: 0x000171D1
	private void Start()
	{
		this.PreviewResearch(null);
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x00018FDA File Offset: 0x000171DA
	private void OnEnable()
	{
		Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.OpenResearchTree, 1);
		this.PopulateQuestTree();
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += this.OnResearchTicketsUpdated;
	}

	// Token: 0x0600049B RID: 1179 RVA: 0x00019005 File Offset: 0x00017205
	private void OnDisable()
	{
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= this.OnResearchTicketsUpdated;
	}

	// Token: 0x0600049C RID: 1180 RVA: 0x0001901D File Offset: 0x0001721D
	private void OnResearchTicketsUpdated(int amount)
	{
		this.RefreshActivateButton();
	}

	// Token: 0x0600049D RID: 1181 RVA: 0x00019025 File Offset: 0x00017225
	public void PreviewResearch(ResearchItemDefinition researchItemDefinition)
	{
		this._currentlyPreviewedResearch = researchItemDefinition;
		this._previewResearchInfoUI.Initialize(researchItemDefinition);
		this.RefreshActivateButton();
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x00019040 File Offset: 0x00017240
	private void PopulateQuestTree()
	{
		foreach (ResearchItemButton researchItemButton in this._researchTreeItemsContainer.GetComponentsInChildren<ResearchItemButton>().ToList<ResearchItemButton>())
		{
			if (researchItemButton.ResearchItemDefinition == null)
			{
				Debug.Log("Research button without ResearchItemDefinition!");
			}
			else
			{
				researchItemButton.Initialize(this);
				this._researchItemButtons.Add(researchItemButton);
			}
		}
		base.StartCoroutine(this.DrawConnectionsNextFrame());
	}

	// Token: 0x0600049F RID: 1183 RVA: 0x000190D0 File Offset: 0x000172D0
	private IEnumerator DrawConnectionsNextFrame()
	{
		yield return null;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(this._researchTreeItemsContainer);
		using (List<ResearchItemButton>.Enumerator enumerator = this._researchItemButtons.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ResearchItemButton researchItemButton = enumerator.Current;
				researchItemButton.DrawConnections(this._researchTreeItemsContainer);
			}
			yield break;
		}
		yield break;
	}

	// Token: 0x060004A0 RID: 1184 RVA: 0x000190E0 File Offset: 0x000172E0
	public ResearchItemButton GetButtonForResearchDefinition(ResearchItemDefinition researchItemDefinition)
	{
		return this._researchItemButtons.FirstOrDefault((ResearchItemButton button) => button.ResearchItemDefinition == researchItemDefinition);
	}

	// Token: 0x060004A1 RID: 1185 RVA: 0x00019114 File Offset: 0x00017314
	public void OnBuyResearchPressed()
	{
		if (!this._currentlyPreviewedResearch.IsLocked() && this._currentlyPreviewedResearch.CanAfford())
		{
			Singleton<ResearchManager>.Instance.ResearchItem(this._currentlyPreviewedResearch);
			this.RefreshActivateButton();
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._unlockResearchSound, Singleton<SoundManager>.Instance.PlayerTransform.position, 1f, 1f, true, false);
			Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.ResearchSomething, 1);
		}
	}

	// Token: 0x060004A2 RID: 1186 RVA: 0x0001918C File Offset: 0x0001738C
	public void RefreshActivateButton()
	{
		this._purchaseButton.SetActive(false);
		this._cantAffordButton.SetActive(false);
		this._alreadyResearchedButton.SetActive(false);
		this._lockedButton.SetActive(false);
		if (this._currentlyPreviewedResearch == null)
		{
			return;
		}
		if (this._currentlyPreviewedResearch.IsResearched())
		{
			this._alreadyResearchedButton.SetActive(true);
			return;
		}
		if (this._currentlyPreviewedResearch.IsLocked())
		{
			this._lockedButton.SetActive(true);
			return;
		}
		if (!this._currentlyPreviewedResearch.CanAfford())
		{
			this._cantAffordButton.SetActive(true);
			return;
		}
		this._purchaseButton.SetActive(true);
	}

	// Token: 0x0400052C RID: 1324
	[SerializeField]
	private RectTransform _researchTreeItemsContainer;

	// Token: 0x0400052D RID: 1325
	[SerializeField]
	private ResearchTreeSelectedResearchInfoUI _previewResearchInfoUI;

	// Token: 0x0400052E RID: 1326
	[SerializeField]
	private GameObject _purchaseButton;

	// Token: 0x0400052F RID: 1327
	[SerializeField]
	private GameObject _cantAffordButton;

	// Token: 0x04000530 RID: 1328
	[SerializeField]
	private GameObject _alreadyResearchedButton;

	// Token: 0x04000531 RID: 1329
	[SerializeField]
	private GameObject _lockedButton;

	// Token: 0x04000532 RID: 1330
	[SerializeField]
	private SoundDefinition _unlockResearchSound;

	// Token: 0x04000533 RID: 1331
	private List<ResearchItemButton> _researchItemButtons = new List<ResearchItemButton>();

	// Token: 0x04000534 RID: 1332
	private ResearchItemDefinition _currentlyPreviewedResearch;
}
