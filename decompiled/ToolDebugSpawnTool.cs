using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class ToolDebugSpawnTool : BaseHeldTool
{
	public float LaunchForce = 5f;

	public float AngleSpread = 5f;

	public float SpinForce = 2f;

	public float SpawnRate = 0.2f;

	public float SpawnRange = 25f;

	public LayerMask HitLayers;

	public SoundDefinition CloneSound;

	public ResourceType SelectedResourceType;

	public PieceType SelectedPieceType;

	public bool SelectedIsPolished;

	[SerializeField]
	private TMP_Text _selectedObjectText;

	[SerializeField]
	private Image _selectedObjectImage;

	private TimeSince _timeSinceLastSpawn;

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateScreenUI();
	}

	public override string GetControlsText()
	{
		return "Drop Tool - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nSpawn Single - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack) + "\nSpawn Multiple - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack) + "\nClone Object - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.RotateObject) + "\nOpen Menu - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.MirrorObject);
	}

	public void SpawnObject()
	{
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (!(componentInChildren == null))
		{
			RaycastHit hitInfo;
			Vector3 vector = ((!Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out hitInfo, SpawnRange, HitLayers)) ? (componentInChildren.transform.position + componentInChildren.transform.forward * SpawnRange) : hitInfo.point);
			Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(SelectedResourceType, SelectedPieceType, SelectedIsPolished, vector - componentInChildren.transform.forward * 0.25f, Quaternion.identity);
		}
	}

	public void LaunchObject()
	{
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 position = componentInChildren.transform.position + componentInChildren.transform.forward * 1f;
		Vector3 forward = Quaternion.Euler(Random.Range(0f - AngleSpread, AngleSpread), Random.Range(0f - AngleSpread, AngleSpread), 0f) * componentInChildren.transform.forward;
		Quaternion rotation = Quaternion.LookRotation(forward);
		OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(SelectedResourceType, SelectedPieceType, SelectedIsPolished, position, rotation);
		if (!(orePiece == null))
		{
			Rigidbody component = orePiece.GetComponent<Rigidbody>();
			if (!(component == null))
			{
				component.AddForce(forward.normalized * LaunchForce, ForceMode.Impulse);
				Vector3 torque = new Vector3(Random.Range(0f - SpinForce, SpinForce), Random.Range(0f - SpinForce, SpinForce), Random.Range(0f - SpinForce, SpinForce));
				component.AddTorque(torque, ForceMode.Impulse);
			}
		}
	}

	public string GetSelectedObjectText()
	{
		if (Singleton<OreManager>.Instance == null)
		{
			return "Unknown";
		}
		return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(SelectedResourceType, SelectedPieceType, SelectedIsPolished);
	}

	private void SelectLookedAtObject()
	{
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null || !Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out var hitInfo, SpawnRange, HitLayers) || !(hitInfo.collider != null))
		{
			return;
		}
		OrePiece component = hitInfo.collider.GetComponent<OrePiece>();
		if (component != null)
		{
			SelectedResourceType = component.ResourceType;
			SelectedPieceType = component.PieceType;
			SelectedIsPolished = component.IsPolished;
			Singleton<SoundManager>.Instance.PlayUISound(CloneSound);
			UpdateScreenUI();
			return;
		}
		OreNode component2 = hitInfo.collider.GetComponent<OreNode>();
		if (component2 != null)
		{
			OrePiece firstOrePrefab = component2.GetFirstOrePrefab();
			SelectedResourceType = firstOrePrefab.ResourceType;
			SelectedPieceType = firstOrePrefab.PieceType;
			SelectedIsPolished = firstOrePrefab.IsPolished;
			Singleton<SoundManager>.Instance.PlayUISound(CloneSound);
			UpdateScreenUI();
		}
	}

	public void SetOreFromPrefab(OrePiece orePrefab)
	{
		if (!(orePrefab == null))
		{
			SelectedResourceType = orePrefab.ResourceType;
			SelectedPieceType = orePrefab.PieceType;
			SelectedIsPolished = orePrefab.IsPolished;
			Singleton<SoundManager>.Instance.PlayUISound(CloneSound);
			UpdateScreenUI();
		}
	}

	private void OpenMenu()
	{
		Singleton<UIManager>.Instance.OreSpawnerSelectOreUI.StartSelectingOre(this);
	}

	public void UpdateScreenUI()
	{
		_selectedObjectText.text = GetSelectedObjectText();
		_selectedObjectImage.sprite = Singleton<OreManager>.Instance.GetIconForOre(SelectedResourceType, SelectedPieceType, SelectedIsPolished);
	}

	public override void QButtonPressed()
	{
		OpenMenu();
	}

	public override void PrimaryFire()
	{
		SpawnObject();
	}

	public override void Reload()
	{
		SelectLookedAtObject();
	}

	public override void SecondaryFireHeld()
	{
		if (_timeSinceLastSpawn > SpawnRate)
		{
			_timeSinceLastSpawn = 0f;
			LaunchObject();
		}
	}

	public override void LoadFromSave(string json)
	{
		ToolDebugSpawnToolSaveData toolDebugSpawnToolSaveData = JsonUtility.FromJson<ToolDebugSpawnToolSaveData>(json);
		if (toolDebugSpawnToolSaveData == null)
		{
			toolDebugSpawnToolSaveData = new ToolDebugSpawnToolSaveData();
		}
		if (toolDebugSpawnToolSaveData.IsInPlayerInventory)
		{
			StartCoroutine(WaitThenAddToInventory(toolDebugSpawnToolSaveData.InventorySlotIndex));
		}
		SelectedResourceType = toolDebugSpawnToolSaveData.OreResourceType;
		SelectedPieceType = toolDebugSpawnToolSaveData.OrePieceType;
		SelectedIsPolished = toolDebugSpawnToolSaveData.OreIsPolished;
		UpdateScreenUI();
	}

	public override string GetCustomSaveData()
	{
		ToolDebugSpawnToolSaveData toolDebugSpawnToolSaveData = new ToolDebugSpawnToolSaveData
		{
			IsInPlayerInventory = (Owner != null)
		};
		if (toolDebugSpawnToolSaveData.IsInPlayerInventory)
		{
			toolDebugSpawnToolSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		toolDebugSpawnToolSaveData.OrePieceType = SelectedPieceType;
		toolDebugSpawnToolSaveData.OreResourceType = SelectedResourceType;
		toolDebugSpawnToolSaveData.OreIsPolished = SelectedIsPolished;
		return JsonUtility.ToJson(toolDebugSpawnToolSaveData);
	}
}
