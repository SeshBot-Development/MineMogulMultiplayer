using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OreSpawnerMacine : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	[SerializeField]
	private string _objectName = "Ore Spawner Machine";

	[SerializeField]
	private TMP_Text _oreNameText;

	[SerializeField]
	private TMP_Text _spawnRateText;

	public Transform OreSpawnPoint;

	public bool Enabled = true;

	public OrePiece OrePrefab;

	[SerializeField]
	private Renderer _lightMeshRenderer;

	[Range(0f, 20f)]
	public float SpawnRate = 2f;

	[SerializeField]
	private List<Interaction> _interactions;

	public float TimeUntilNextSpawn { get; protected set; }

	private void Start()
	{
		TimeUntilNextSpawn = SpawnRate;
	}

	private void OnEnable()
	{
		SetOrePrefab(OrePrefab);
	}

	protected virtual void Update()
	{
		if (Enabled && SpawnRate > 0f)
		{
			TimeUntilNextSpawn -= Time.deltaTime;
			TimeUntilNextSpawn = Mathf.Min(TimeUntilNextSpawn, SpawnRate);
			if (TimeUntilNextSpawn <= 0f)
			{
				TrySpawnOre();
				TimeUntilNextSpawn += SpawnRate * Singleton<OreLimitManager>.Instance.GetAutoMinerSpawnTimeMultiplier();
			}
		}
	}

	public void SetOrePrefab(OrePiece orePrefab)
	{
		OrePrefab = orePrefab;
		_oreNameText.text = ((orePrefab != null) ? Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(OrePrefab.ResourceType, OrePrefab.PieceType, OrePrefab.IsPolished) : "No Ore Selected?");
	}

	protected virtual void TrySpawnOre()
	{
		if (!Singleton<OreLimitManager>.Instance.ShouldBlockOreSpawning())
		{
			OrePiece orePrefab = OrePrefab;
			if (orePrefab != null)
			{
				Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePrefab, OreSpawnPoint.position, OreSpawnPoint.rotation);
			}
		}
	}

	public void SetSpawnRate(float spawnRate)
	{
		SpawnRate = spawnRate;
		_spawnRateText.text = $"{60f / SpawnRate} items / min";
	}

	public void Toggle(bool on)
	{
		if (on)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	protected virtual void TurnOn()
	{
		Enabled = true;
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
	}

	protected virtual void TurnOff()
	{
		Enabled = false;
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.RedLightMaterial);
	}

	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[1] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public string GetObjectName()
	{
		return _objectName;
	}

	public virtual void Interact(Interaction selectedInteraction)
	{
		switch (selectedInteraction.Name)
		{
		case "Turn On":
			TurnOn();
			break;
		case "Turn Off":
			TurnOff();
			break;
		case "Configure":
			Singleton<UIManager>.Instance.ShowOreSpawnerSelectOreUI(this);
			break;
		}
	}

	public virtual void LoadFromSave(string json)
	{
		OreSpawnerMachineSaveData oreSpawnerMachineSaveData = JsonUtility.FromJson<OreSpawnerMachineSaveData>(json);
		if (oreSpawnerMachineSaveData == null)
		{
			oreSpawnerMachineSaveData = new OreSpawnerMachineSaveData();
		}
		OrePiece orePiecePrefab = Singleton<SavingLoadingManager>.Instance.GetOrePiecePrefab(oreSpawnerMachineSaveData.OreResourceType, oreSpawnerMachineSaveData.OrePieceType, oreSpawnerMachineSaveData.OreIsPolished);
		if (orePiecePrefab != null)
		{
			SetOrePrefab(orePiecePrefab);
		}
		SetSpawnRate(oreSpawnerMachineSaveData.SpawnRate);
		Toggle(oreSpawnerMachineSaveData.IsOn);
	}

	public virtual string GetCustomSaveData()
	{
		OreSpawnerMachineSaveData oreSpawnerMachineSaveData = new OreSpawnerMachineSaveData();
		oreSpawnerMachineSaveData.IsOn = Enabled;
		oreSpawnerMachineSaveData.SpawnRate = SpawnRate;
		if (OrePrefab != null)
		{
			oreSpawnerMachineSaveData.OrePieceType = OrePrefab.PieceType;
			oreSpawnerMachineSaveData.OreResourceType = OrePrefab.ResourceType;
			oreSpawnerMachineSaveData.OreIsPolished = OrePrefab.IsPolished;
		}
		return JsonUtility.ToJson(oreSpawnerMachineSaveData);
	}
}
