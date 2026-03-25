using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class InventoryUIManager : Singleton<InventoryUIManager>
{
	public GameObject InventoryPanel;

	public GameObject HotbarPanel;

	public GameObject InventorySlotsPanel;

	public GameObject InventorySlotPrefab;

	public TMP_Text MoneyText;

	public GameObject DragGhostIcon;

	public Image DragGhostImage;

	public TMP_Text DragGhostAmountText;

	public GameObject SelectedItemInfo;

	public BaseHeldTool SelectedTool;

	public TMP_Text SelectedItemNameText;

	public KeybindTokenText SelectedItemDescriptionText;

	public TMP_Text SelectedItemAmountText;

	public Image SelectedItemIcon;

	[SerializeField]
	private TMP_Text _researchTicketsCountText;

	[SerializeField]
	private TMP_Text _availableQuestsText;

	[SerializeField]
	private GameObject _availableQuestsObject;

	[SerializeField]
	private TMP_Text _equipToolButtonText;

	private float _displayedMoney;

	private float _updateTimer;

	private int UpdatesPerSecond = 15;

	public event Action InventoryOpened;

	public event Action InventoryClosed;

	private void Start()
	{
		DragGhostIcon.SetActive(value: false);
	}

	private void OnEnable()
	{
		UpdateAvailableQuestCount();
		UpdateResearchTicketCount();
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated += UpdateResearchTicketCount;
		Singleton<QuestManager>.Instance.QuestActivated += UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestPaused += UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestCompleted += UpdateAvailableQuestCount;
	}

	private void OnDisable()
	{
		Singleton<ResearchManager>.Instance.ResearchTicketsUpdated -= UpdateResearchTicketCount;
		Singleton<QuestManager>.Instance.QuestActivated -= UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestPaused -= UpdateAvailableQuestCount;
		Singleton<QuestManager>.Instance.QuestCompleted -= UpdateAvailableQuestCount;
	}

	public void ToggleInventory()
	{
		ShowInventory(!InventoryPanel.activeSelf);
	}

	public void ShowInventory(bool show)
	{
		if (show)
		{
			UpdateSelectedItemInfo(null);
			Singleton<QuestManager>.Instance.ActivateQuestTrigger(TriggeredQuestRequirementType.OpenInventory);
			this.InventoryOpened?.Invoke();
		}
		else
		{
			DragGhostIcon.SetActive(value: false);
			this.InventoryClosed?.Invoke();
		}
		InventoryPanel.SetActive(show);
	}

	public bool IsShowingInventory()
	{
		return InventoryPanel.activeSelf;
	}

	public void UpdateSelectedItemInfo(BaseHeldTool tool)
	{
		if (tool == null)
		{
			SelectedTool = null;
			SelectedItemInfo.SetActive(value: false);
			return;
		}
		SelectedItemInfo.SetActive(value: true);
		SelectedItemNameText.text = tool.Name;
		SelectedItemDescriptionText.SetText(tool.Description);
		SelectedItemAmountText.text = ((tool.Quantity > 1) ? tool.Quantity.ToString() : "");
		SelectedItemIcon.sprite = tool.GetIcon();
		SelectedTool = tool;
		if (SelectedTool is ToolBuilder)
		{
			_equipToolButtonText.text = "Build";
		}
		else
		{
			_equipToolButtonText.text = "Equip";
		}
	}

	public void DropSelectedTool()
	{
		if (SelectedTool != null)
		{
			SelectedTool.DropItem();
		}
		UpdateSelectedItemInfo(null);
	}

	public void EquipSelectedTool()
	{
		if (!(SelectedTool == null))
		{
			UnityEngine.Object.FindObjectOfType<PlayerInventory>().SwitchTool(SelectedTool);
			ShowInventory(show: false);
		}
	}

	private void Update()
	{
		_updateTimer += Time.deltaTime;
		float num = 1f / (float)Mathf.Max(UpdatesPerSecond, 1);
		if (_updateTimer >= num)
		{
			_updateTimer = 0f;
			float money = Singleton<EconomyManager>.Instance.Money;
			_displayedMoney = Mathf.Lerp(_displayedMoney, money, 0.5f);
			MoneyText.text = $"${_displayedMoney:#,##0.00}";
		}
	}

	public void UpdateAvailableQuestCount(Quest quest = null)
	{
		int count = Singleton<QuestManager>.Instance.GetAvailableQuests().Count;
		if (count == 0)
		{
			_availableQuestsObject.SetActive(value: false);
			return;
		}
		_availableQuestsObject.SetActive(value: true);
		_availableQuestsText.text = string.Format("{0} Quest{1} Available!", count, (count > 1) ? "s" : "");
	}

	public void UpdateResearchTicketCount(int amount = 0)
	{
		if (Singleton<ResearchManager>.Instance.ResearchTickets == 0)
		{
			_researchTicketsCountText.gameObject.SetActive(value: false);
			return;
		}
		_researchTicketsCountText.gameObject.SetActive(value: true);
		_researchTicketsCountText.color = Singleton<UIManager>.Instance.ResearchTicketsTextColor;
		_researchTicketsCountText.text = $"Research Tickets: ¤{Singleton<ResearchManager>.Instance.ResearchTickets}";
	}
}
