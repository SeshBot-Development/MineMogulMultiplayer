using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000004 RID: 4
public class AutoMiner : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	// Token: 0x17000007 RID: 7
	// (get) Token: 0x06000016 RID: 22 RVA: 0x00002470 File Offset: 0x00000670
	// (set) Token: 0x06000017 RID: 23 RVA: 0x00002478 File Offset: 0x00000678
	public float TimeUntilNextSpawn { get; protected set; }

	// Token: 0x06000018 RID: 24 RVA: 0x00002481 File Offset: 0x00000681
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x06000019 RID: 25 RVA: 0x00002484 File Offset: 0x00000684
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x0600001A RID: 26 RVA: 0x0000248C File Offset: 0x0000068C
	public string GetObjectName()
	{
		return this._objectName;
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00002494 File Offset: 0x00000694
	private void Start()
	{
		this.TimeUntilNextSpawn = this.SpawnRate;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x000024A4 File Offset: 0x000006A4
	protected virtual void OnEnable()
	{
		this._rotationAxis = (this.RotateZ ? Vector3.back : (this.RotateY ? Vector3.down : Vector3.right));
		if (this.ResourceDefinition != null)
		{
			this.ConfigureFromDefinition();
		}
		else
		{
			BuildingObject componentInParent = base.GetComponentInParent<BuildingObject>();
			foreach (BuildingPlacementNode buildingPlacementNode in BuildingPlacementNode.All)
			{
				if (buildingPlacementNode.RequirementType == componentInParent.PlacementNodeRequirement && Vector3.Distance(buildingPlacementNode.transform.position, base.transform.position) < 1f)
				{
					buildingPlacementNode.AttachBuilding(base.GetComponent<BuildingObject>());
					break;
				}
			}
		}
		if (this.ResourceDefinition == null)
		{
			this.Toggle(false);
			base.StartCoroutine(this.WaitThenCheckIfValid());
			return;
		}
		this.Toggle(this.Enabled);
	}

	// Token: 0x0600001D RID: 29 RVA: 0x000025A4 File Offset: 0x000007A4
	private IEnumerator WaitThenCheckIfValid()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		if (base.gameObject == null)
		{
			yield break;
		}
		if (base.GetComponentInParent<BuildingObject>().IsGhost)
		{
			yield break;
		}
		if (this.ResourceDefinition == null)
		{
			Singleton<UIManager>.Instance.ShowInfoMessagePopup("New Update!", "The game has had a new update! \nOne or more " + this.GetObjectName() + " spots have been moved. \nThe affected Miner(s) have been packed into boxes and can be replaced.");
			base.GetComponent<BuildingObject>().Pack();
		}
		yield break;
	}

	// Token: 0x0600001E RID: 30 RVA: 0x000025B3 File Offset: 0x000007B3
	public virtual void ConfigureFromDefinition()
	{
		if (this.ResourceDefinition != null)
		{
			this.SpawnProbability = this.ResourceDefinition.SpawnProbability;
			this.SpawnRate = this.ResourceDefinition.SpawnRate;
			return;
		}
		Debug.Log("AutoMiner doesn't have a resource definition!");
	}

	// Token: 0x0600001F RID: 31 RVA: 0x000025F0 File Offset: 0x000007F0
	protected virtual void Update()
	{
		if (this.Enabled && this.SpawnRate > 0f)
		{
			if (this.ResourceDefinition == null)
			{
				base.GetComponent<BuildingObject>().Pack();
				return;
			}
			float num = 360f / (this.SpawnRate * (float)this.OresPerRotation) * Time.deltaTime;
			this.Rotator.transform.Rotate(this._rotationAxis, num);
			this.TimeUntilNextSpawn -= Time.deltaTime;
			this.TimeUntilNextSpawn = Mathf.Min(this.TimeUntilNextSpawn, this.SpawnRate);
			if (this.TimeUntilNextSpawn <= 0f)
			{
				this.TrySpawnOre();
				this.TimeUntilNextSpawn += this.SpawnRate * Singleton<OreLimitManager>.Instance.GetAutoMinerSpawnTimeMultiplier();
			}
		}
	}

	// Token: 0x06000020 RID: 32 RVA: 0x000026C0 File Offset: 0x000008C0
	protected virtual void TrySpawnOre()
	{
		if (Singleton<OreLimitManager>.Instance.ShouldBlockOreSpawning())
		{
			return;
		}
		if (Random.Range(0f, 100f) <= this.SpawnProbability)
		{
			OrePiece orePiece = this.ResourceDefinition.GetOrePrefab(this.CanProduceGems);
			if (orePiece == null)
			{
				orePiece = this.FallbackOrePrefab;
			}
			if (orePiece != null)
			{
				Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(orePiece, this.OreSpawnPoint.position, this.OreSpawnPoint.rotation, null);
			}
		}
	}

	// Token: 0x06000021 RID: 33 RVA: 0x0000273F File Offset: 0x0000093F
	public void Toggle(bool on)
	{
		if (on)
		{
			this.TurnOn();
			return;
		}
		this.TurnOff();
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00002751 File Offset: 0x00000951
	protected virtual void TurnOn()
	{
		this.Enabled = true;
		this._audioSource_Loop.Play();
		this.ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
	}

	// Token: 0x06000023 RID: 35 RVA: 0x00002775 File Offset: 0x00000975
	protected virtual void TurnOff()
	{
		this.Enabled = false;
		this._audioSource_Loop.Pause();
		this.ChangeLightMaterial(Singleton<BuildingManager>.Instance.RedLightMaterial);
	}

	// Token: 0x06000024 RID: 36 RVA: 0x0000279C File Offset: 0x0000099C
	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightMeshRenderer.sharedMaterials;
		sharedMaterials[1] = material;
		this._lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x06000025 RID: 37 RVA: 0x000027C8 File Offset: 0x000009C8
	public virtual void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Turn On")
		{
			this.TurnOn();
			return;
		}
		if (!(name == "Turn Off"))
		{
			return;
		}
		this.TurnOff();
	}

	// Token: 0x06000026 RID: 38 RVA: 0x00002804 File Offset: 0x00000A04
	public virtual void LoadFromSave(string json)
	{
		AutoMinerSaveData autoMinerSaveData = JsonUtility.FromJson<AutoMinerSaveData>(json);
		if (autoMinerSaveData == null)
		{
			autoMinerSaveData = new AutoMinerSaveData();
		}
		this.Toggle(autoMinerSaveData.IsOn);
	}

	// Token: 0x06000027 RID: 39 RVA: 0x0000282D File Offset: 0x00000A2D
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new AutoMinerSaveData
		{
			IsOn = this.Enabled
		});
	}

	// Token: 0x04000023 RID: 35
	[SerializeField]
	private string _objectName = "Auto-Miner Mk1";

	// Token: 0x04000024 RID: 36
	public GameObject Rotator;

	// Token: 0x04000025 RID: 37
	public bool RotateY;

	// Token: 0x04000026 RID: 38
	public bool RotateZ;

	// Token: 0x04000027 RID: 39
	public Transform OreSpawnPoint;

	// Token: 0x04000028 RID: 40
	public bool Enabled = true;

	// Token: 0x04000029 RID: 41
	public int OresPerRotation = 12;

	// Token: 0x0400002A RID: 42
	public OrePiece FallbackOrePrefab;

	// Token: 0x0400002B RID: 43
	public bool CanProduceGems = true;

	// Token: 0x0400002D RID: 45
	[SerializeField]
	protected LoopingSoundPlayer _audioSource_Loop;

	// Token: 0x0400002E RID: 46
	[SerializeField]
	private Renderer _lightMeshRenderer;

	// Token: 0x0400002F RID: 47
	private Vector3 _rotationAxis;

	// Token: 0x04000030 RID: 48
	[SerializeField]
	private List<Interaction> _interactions;

	// Token: 0x04000031 RID: 49
	public AutoMinerResourceDefinition ResourceDefinition;

	// Token: 0x04000032 RID: 50
	[Header("--- Set from Resource Definition ---")]
	[Range(0f, 100f)]
	public float SpawnProbability = 80f;

	// Token: 0x04000033 RID: 51
	[Range(0f, 20f)]
	public float SpawnRate = 2f;
}
