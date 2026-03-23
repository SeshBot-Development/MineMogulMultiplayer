using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005F RID: 95
[DefaultExecutionOrder(-1)]
public class InventoryUIManager : Singleton<InventoryUIManager>
{
	// Token: 0x14000008 RID: 8
	// (add) Token: 0x0600025E RID: 606 RVA: 0x0000BFF4 File Offset: 0x0000A1F4
	// (remove) Token: 0x0600025F RID: 607 RVA: 0x0000C02C File Offset: 0x0000A22C
	public event Action InventoryOpened;

	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06000260 RID: 608 RVA: 0x0000C064 File Offset: 0x0000A264
	// (remove) Token: 0x06000261 RID: 609 RVA: 0x0000C09C File Offset: 0x0000A29C
	public event Action InventoryClosed;

	// Token: 0x06000262 RID: 610 RVA: 0x0000C0D1 File Offset: 0x0000A2D1
	private void Start()
	{
		this.DragGhostIcon.SetActive(false);
	}

	// Token: 0x06000263 RID: 611 RVA: 0x0000C0E0 File Offset: 0x0000A2E0
	private void OnEnable()
	{
		this.UpdateAvailableQuestCount(null);
		this.UpdateResearchTicketCount(0);
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += this.UpdateResearchTicketCount;
		Singleton<QuestManager>.Instance.QuestActivated += this.UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestPaused += this.UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestCompleted += this.UpdateAvailableQuestCount;
	}

	// Token: 0x06000264 RID: 612 RVA: 0x0000C154 File Offset: 0x0000A354
	private void OnDisable()
	{
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= this.UpdateResearchTicketCount;
		Singleton<QuestManager>.Instance.QuestActivated -= this.UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestPaused -= this.UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestCompleted -= this.UpdateAvailableQuestCount;
	}

	// Token: 0x06000265 RID: 613 RVA: 0x0000C1B9 File Offset: 0x0000A3B9
	public void ToggleInventory()
	{
		this.ShowInventory(!this.InventoryPanel.activeSelf);
	}

	// Token: 0x06000266 RID: 614 RVA: 0x0000C1D0 File Offset: 0x0000A3D0
	public void ShowInventory(bool show)
	{
		if (show)
		{
			this.UpdateSelectedItemInfo(null);
			Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.OpenInventory, 1);
			Action inventoryOpened = this.InventoryOpened;
			if (inventoryOpened != null)
			{
				inventoryOpened();
			}
		}
		else
		{
			this.DragGhostIcon.SetActive(false);
			Action inventoryClosed = this.InventoryClosed;
			if (inventoryClosed != null)
			{
				inventoryClosed();
			}
		}
		this.InventoryPanel.SetActive(show);
	}

	// Token: 0x06000267 RID: 615 RVA: 0x0000C230 File Offset: 0x0000A430
	public bool IsShowingInventory()
	{
		return this.InventoryPanel.activeSelf;
	}

	// Token: 0x06000268 RID: 616 RVA: 0x0000C240 File Offset: 0x0000A440
	public void UpdateSelectedItemInfo(BaseHeldTool tool)
	{
		if (tool == null)
		{
			this.SelectedTool = null;
			this.SelectedItemInfo.SetActive(false);
			return;
		}
		this.SelectedItemInfo.SetActive(true);
		this.SelectedItemNameText.text = tool.Name;
		this.SelectedItemDescriptionText.SetText(tool.Description);
		this.SelectedItemAmountText.text = ((tool.Quantity > 1) ? tool.Quantity.ToString() : "");
		this.SelectedItemIcon.sprite = tool.GetIcon();
		this.SelectedTool = tool;
		if (this.SelectedTool is ToolBuilder)
		{
			this._equipToolButtonText.text = "Build";
			return;
		}
		this._equipToolButtonText.text = "Equip";
	}

	// Token: 0x06000269 RID: 617 RVA: 0x0000C304 File Offset: 0x0000A504
	public void DropSelectedTool()
	{
		if (this.SelectedTool != null)
		{
			this.SelectedTool.DropItem();
		}
		this.UpdateSelectedItemInfo(null);
	}

	// Token: 0x0600026A RID: 618 RVA: 0x0000C326 File Offset: 0x0000A526
	public void EquipSelectedTool()
	{
		if (this.SelectedTool == null)
		{
			return;
		}
		Object.FindObjectOfType<PlayerInventory>().SwitchTool(this.SelectedTool);
		this.ShowInventory(false);
	}

	// Token: 0x0600026B RID: 619 RVA: 0x0000C350 File Offset: 0x0000A550
	private void Update()
	{
		this._updateTimer += Time.deltaTime;
		float num = 1f / (float)Mathf.Max(this.UpdatesPerSecond, 1);
		if (this._updateTimer >= num)
		{
			this._updateTimer = 0f;
			float money = Singleton<EconomyManager>.Instance.Money;
			this._displayedMoney = Mathf.Lerp(this._displayedMoney, money, 0.5f);
			this.MoneyText.text = string.Format("${0:#,##0.00}", this._displayedMoney);
		}
	}

	// Token: 0x0600026C RID: 620 RVA: 0x0000C3DC File Offset: 0x0000A5DC
	public void UpdateAvailableQuestCount(Quest quest = null)
	{
		int count = Singleton<QuestManager>.Instance.GetAvailableQuests().Count;
		if (count == 0)
		{
			this._availableQuestsObject.SetActive(false);
			return;
		}
		this._availableQuestsObject.SetActive(true);
		this._availableQuestsText.text = string.Format("{0} Quest{1} Available!", count, (count > 1) ? "s" : "");
	}

	// Token: 0x0600026D RID: 621 RVA: 0x0000C440 File Offset: 0x0000A640
	public void UpdateResearchTicketCount(int amount = 0)
	{
		if (Singleton<ResearchManager>.Instance.ResearchTickets == 0)
		{
			this._researchTicketsCountText.gameObject.SetActive(false);
			return;
		}
		this._researchTicketsCountText.gameObject.SetActive(true);
		this._researchTicketsCountText.color = Singleton<UIManager>.Instance.ResearchTicketsTextColor;
		this._researchTicketsCountText.text = string.Format("Research Tickets: ¤{0}", Singleton<ResearchManager>.Instance.ResearchTickets);
	}

	// Token: 0x0400022C RID: 556
	public GameObject InventoryPanel;

	// Token: 0x0400022D RID: 557
	public GameObject HotbarPanel;

	// Token: 0x0400022E RID: 558
	public GameObject InventorySlotsPanel;

	// Token: 0x0400022F RID: 559
	public GameObject InventorySlotPrefab;

	// Token: 0x04000230 RID: 560
	public TMP_Text MoneyText;

	// Token: 0x04000231 RID: 561
	public GameObject DragGhostIcon;

	// Token: 0x04000232 RID: 562
	public Image DragGhostImage;

	// Token: 0x04000233 RID: 563
	public TMP_Text DragGhostAmountText;

	// Token: 0x04000234 RID: 564
	public GameObject SelectedItemInfo;

	// Token: 0x04000235 RID: 565
	public BaseHeldTool SelectedTool;

	// Token: 0x04000236 RID: 566
	public TMP_Text SelectedItemNameText;

	// Token: 0x04000237 RID: 567
	public KeybindTokenText SelectedItemDescriptionText;

	// Token: 0x04000238 RID: 568
	public TMP_Text SelectedItemAmountText;

	// Token: 0x04000239 RID: 569
	public Image SelectedItemIcon;

	// Token: 0x0400023A RID: 570
	[SerializeField]
	private TMP_Text _researchTicketsCountText;

	// Token: 0x0400023B RID: 571
	[SerializeField]
	private TMP_Text _availableQuestsText;

	// Token: 0x0400023C RID: 572
	[SerializeField]
	private GameObject _availableQuestsObject;

	// Token: 0x0400023D RID: 573
	[SerializeField]
	private TMP_Text _equipToolButtonText;

	// Token: 0x04000240 RID: 576
	private float _displayedMoney;

	// Token: 0x04000241 RID: 577
	private float _updateTimer;

	// Token: 0x04000242 RID: 578
	private int UpdatesPerSecond = 15;
}
