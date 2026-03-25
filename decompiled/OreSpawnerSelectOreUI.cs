using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OreSpawnerSelectOreUI : MonoBehaviour
{
	[SerializeField]
	private Transform _buttonContainer;

	[SerializeField]
	private OreSpawnerOreButton _oreButtonPrefab;

	[SerializeField]
	private GameObject _spawnRatePanel;

	[SerializeField]
	private TMP_Text _itemsPerMinuteText;

	[SerializeField]
	private Slider _itemsPerMinuteSlider;

	private ToolDebugSpawnTool _selectedDebugSpawnerTool;

	private OreSpawnerMacine _selectedOreSpawnerMacine;

	private bool _hasPopulatedUI;

	private float _selectedSpawnRate = 2f;

	public void PopulateButtons()
	{
		foreach (Transform item in _buttonContainer)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (OrePiece allOrePiecePrefab in Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs)
		{
			if (allOrePiecePrefab != null)
			{
				Object.Instantiate(_oreButtonPrefab, _buttonContainer).Initialize(allOrePiecePrefab, this);
			}
		}
		_hasPopulatedUI = true;
	}

	public void OnEnable()
	{
		if (!_hasPopulatedUI)
		{
			PopulateButtons();
		}
		_itemsPerMinuteSlider.onValueChanged.AddListener(OnSliderChanged);
	}

	public void OnDisable()
	{
		if (_selectedOreSpawnerMacine != null)
		{
			_selectedOreSpawnerMacine.SetSpawnRate(_selectedSpawnRate);
		}
		_selectedDebugSpawnerTool = null;
		_selectedOreSpawnerMacine = null;
		_itemsPerMinuteSlider.onValueChanged.RemoveListener(OnSliderChanged);
	}

	private void OnSliderChanged(float value)
	{
		SetSelectedSpawnRate(value);
	}

	private void SetSelectedSpawnRate(float spawnRate)
	{
		spawnRate = Mathf.Round(spawnRate);
		_selectedSpawnRate = 60f / spawnRate;
		_itemsPerMinuteText.text = $"{spawnRate} Items / Minute";
	}

	public void StartSelectingOre(ToolDebugSpawnTool spawnerTool)
	{
		_spawnRatePanel.SetActive(value: false);
		_selectedDebugSpawnerTool = spawnerTool;
		_selectedOreSpawnerMacine = null;
		base.gameObject.SetActive(value: true);
	}

	public void StartSelectingOreAndSpawnRate(OreSpawnerMacine oreSpawnerMacine)
	{
		float num = 60f / oreSpawnerMacine.SpawnRate;
		SetSelectedSpawnRate(num);
		_itemsPerMinuteSlider.value = num;
		_spawnRatePanel.SetActive(value: true);
		_selectedDebugSpawnerTool = null;
		_selectedOreSpawnerMacine = oreSpawnerMacine;
		base.gameObject.SetActive(value: true);
	}

	public void OnCloseButtonPressed()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnOreSelected(OrePiece orePrefab)
	{
		if (_selectedDebugSpawnerTool != null)
		{
			_selectedDebugSpawnerTool.SetOreFromPrefab(orePrefab);
		}
		if (_selectedOreSpawnerMacine != null)
		{
			_selectedOreSpawnerMacine.SetOrePrefab(orePrefab);
		}
		base.gameObject.SetActive(value: false);
	}
}
