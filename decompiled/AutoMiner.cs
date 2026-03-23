using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMiner : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	[SerializeField]
	private string _objectName = "Auto-Miner Mk1";

	public GameObject Rotator;

	public bool RotateY;

	public bool RotateZ;

	public Transform OreSpawnPoint;

	public bool Enabled = true;

	public int OresPerRotation = 12;

	public OrePiece FallbackOrePrefab;

	public bool CanProduceGems = true;

	[SerializeField]
	protected LoopingSoundPlayer _audioSource_Loop;

	[SerializeField]
	private Renderer _lightMeshRenderer;

	private Vector3 _rotationAxis;

	[SerializeField]
	private List<Interaction> _interactions;

	public AutoMinerResourceDefinition ResourceDefinition;

	[Header("--- Set from Resource Definition ---")]
	[Range(0f, 100f)]
	public float SpawnProbability = 80f;

	[Range(0f, 20f)]
	public float SpawnRate = 2f;

	public float TimeUntilNextSpawn { get; protected set; }

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

	private void Start()
	{
		TimeUntilNextSpawn = SpawnRate;
	}

	protected virtual void OnEnable()
	{
		_rotationAxis = (RotateZ ? Vector3.back : (RotateY ? Vector3.down : Vector3.right));
		if (ResourceDefinition != null)
		{
			ConfigureFromDefinition();
		}
		else
		{
			BuildingObject componentInParent = GetComponentInParent<BuildingObject>();
			foreach (BuildingPlacementNode item in BuildingPlacementNode.All)
			{
				if (item.RequirementType == componentInParent.PlacementNodeRequirement && Vector3.Distance(item.transform.position, base.transform.position) < 1f)
				{
					item.AttachBuilding(GetComponent<BuildingObject>());
					break;
				}
			}
		}
		if (ResourceDefinition == null)
		{
			Toggle(on: false);
			StartCoroutine(WaitThenCheckIfValid());
		}
		else
		{
			Toggle(Enabled);
		}
	}

	private IEnumerator WaitThenCheckIfValid()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		if (!(base.gameObject == null) && !GetComponentInParent<BuildingObject>().IsGhost && ResourceDefinition == null)
		{
			Singleton<UIManager>.Instance.ShowInfoMessagePopup("New Update!", "The game has had a new update! \nOne or more " + GetObjectName() + " spots have been moved. \nThe affected Miner(s) have been packed into boxes and can be replaced.");
			GetComponent<BuildingObject>().Pack();
		}
	}

	public virtual void ConfigureFromDefinition()
	{
		if (ResourceDefinition != null)
		{
			SpawnProbability = ResourceDefinition.SpawnProbability;
			SpawnRate = ResourceDefinition.SpawnRate;
		}
		else
		{
			Debug.Log("AutoMiner doesn't have a resource definition!");
		}
	}

	protected virtual void Update()
	{
		if (!Enabled || !(SpawnRate > 0f))
		{
			return;
		}
		if (ResourceDefinition == null)
		{
			GetComponent<BuildingObject>().Pack();
			return;
		}
		float angle = 360f / (SpawnRate * (float)OresPerRotation) * Time.deltaTime;
		Rotator.transform.Rotate(_rotationAxis, angle);
		TimeUntilNextSpawn -= Time.deltaTime;
		TimeUntilNextSpawn = Mathf.Min(TimeUntilNextSpawn, SpawnRate);
		if (TimeUntilNextSpawn <= 0f)
		{
			TrySpawnOre();
			TimeUntilNextSpawn += SpawnRate * Singleton<OreLimitManager>.Instance.GetAutoMinerSpawnTimeMultiplier();
		}
	}

	protected virtual void TrySpawnOre()
	{
		if (!Singleton<OreLimitManager>.Instance.ShouldBlockOreSpawning() && Random.Range(0f, 100f) <= SpawnProbability)
		{
			OrePiece orePiece = ResourceDefinition.GetOrePrefab(CanProduceGems);
			if (orePiece == null)
			{
				orePiece = FallbackOrePrefab;
			}
			if (orePiece != null)
			{
				Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece, OreSpawnPoint.position, OreSpawnPoint.rotation);
			}
		}
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
		_audioSource_Loop.Play();
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
	}

	protected virtual void TurnOff()
	{
		Enabled = false;
		_audioSource_Loop.Pause();
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.RedLightMaterial);
	}

	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[1] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	public virtual void Interact(Interaction selectedInteraction)
	{
		string text = selectedInteraction.Name;
		if (!(text == "Turn On"))
		{
			if (text == "Turn Off")
			{
				TurnOff();
			}
		}
		else
		{
			TurnOn();
		}
	}

	public virtual void LoadFromSave(string json)
	{
		AutoMinerSaveData autoMinerSaveData = JsonUtility.FromJson<AutoMinerSaveData>(json);
		if (autoMinerSaveData == null)
		{
			autoMinerSaveData = new AutoMinerSaveData();
		}
		Toggle(autoMinerSaveData.IsOn);
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new AutoMinerSaveData
		{
			IsOn = Enabled
		});
	}
}
